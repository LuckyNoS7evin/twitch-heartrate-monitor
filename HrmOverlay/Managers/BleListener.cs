using HrmOverlay.Extensions;
using HrmOverlay.Hubs;
using HrmOverlay.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace HrmOverlay.Managers
{
    public class BleListener
    {
        private readonly IMemoryCache _memoryCache;
        private readonly List<BluetoothLEDevice> _devices;
        private readonly Dictionary<string, IReadOnlyList<GattDeviceService>> _services;
        private readonly Dictionary<string, IReadOnlyList<GattCharacteristic>> _characteristics;
        private readonly Dictionary<string, GattCharacteristic> _listeners;
        private readonly IHubContext<HeartrateHub> _browserHubContext;

        public BleListener(IMemoryCache memoryCache, IHubContext<HeartrateHub> browserHubContext)
        {
            _memoryCache = memoryCache;
            _devices = new List<BluetoothLEDevice>();
            _services = new Dictionary<string, IReadOnlyList<GattDeviceService>>();
            _characteristics = new Dictionary<string, IReadOnlyList<GattCharacteristic>>();
            _listeners = new Dictionary<string, GattCharacteristic>();
            _browserHubContext = browserHubContext;
        }

        public async Task<List<DeviceModel>> GetDevices()
        {
            var devices = new List<DeviceModel>();
            var result = await DeviceInformation.FindAllAsync(BluetoothLEDevice.GetDeviceSelectorFromPairingState(true));
            foreach (var item in result)
            {
                devices.Add(new DeviceModel
                {
                    Id = item.Id,
                    Name = item.Name
                });

            }
            return devices;
        }
        
        public async Task<DeviceModel> GetDevice(string id)
        {
            if (!_memoryCache.TryGetValue<DeviceModel>(id, out var device))
            {
                var result = await BluetoothLEDevice.FromIdAsync(id);
                _devices.Add(result);
                device = new DeviceModel
                {
                    Id = result.DeviceId,
                    Name = result.Name
                };

                var gattResult = await result.GetServices();
                if (gattResult.Status == GattCommunicationStatus.Success)
                {
                    var services = gattResult.Services;

                    _services.Add(device.Id, services);

                    device.Services = new List<ServiceModel>();
                    foreach (var service in services)
                    {
                        var serviceModel = new ServiceModel
                        {
                            Id = service.Uuid.ToString(),
                            Name = service.GetServiceName()
                        };

                        var chars = await service.GetCharacteristics();
                        if (chars != null)
                        {

                            _characteristics.Add($"{device.Id}::{serviceModel.Name}", chars);

                            foreach (var character in chars)
                            {
                                serviceModel.Characteristics.Add(character.GetCharacteristicName());
                            }
                            device.Services.Add(serviceModel);
                        }
                    }
                }
                _memoryCache.Set(id, device);
            }

            return device;
        }

        public List<ListenerModel> GetListeners()
        {
            var result = new List<ListenerModel>();

            foreach (var key in _listeners.Keys)
            {
                var split = key.Split("::");

                result.Add(new ListenerModel { Type = split[0], Id = split[1], Service = split[2], Characteristic = split[3] });
            }
            return result;
        }

        public async Task Unlisten(string type,string id, string serviceName, string characteristicName)
        {
            if (_listeners.ContainsKey($"{type}::{id}::{serviceName}::{characteristicName}"))
            {
                var listener = _listeners[$"{type}::{id}::{serviceName}::{characteristicName}"];
                await listener.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
                listener.ValueChanged -= Characteristic_ValueChanged;
                _listeners.Remove($"{type}::{id}::{serviceName}::{characteristicName}");
            }
        }

        public async Task Listen(string type, string id, string serviceName, string characteristicName)
        {
            //var device = _devices.FirstOrDefault(dev => dev.DeviceId == id);
            //var service = _services[id].FirstOrDefault(ser => ser.GetServiceName() == serviceName);
            var characteristic = _characteristics[$"{id}::{serviceName}"].FirstOrDefault(cha => cha.GetCharacteristicName() == characteristicName);

            var cccdValue = GattClientCharacteristicConfigurationDescriptorValue.None;
            if (characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Indicate))
            {
                cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Indicate;
            }
            else if (characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
            {
                cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Notify;
            }

            var result = await characteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
            if (result.Status == GattCommunicationStatus.Success)
            {
                Console.WriteLine(FormatAsString(result.Value));
            }
            // BT_Code: Must write the CCCD in order for server to send indications.
            // We receive them in the ValueChanged event handler.
            var listener = await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(cccdValue);
            if (listener == GattCommunicationStatus.Success)
            {
                characteristic.ValueChanged += Characteristic_ValueChanged;
                _listeners.Add($"{type}::{id}::{serviceName}::{characteristicName}", characteristic);
            }

        }

        private async void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            var characteristicName = sender.GetCharacteristicName();
            var serviceName = sender.Service.GetServiceName();
            var deviceId = sender.Service.Device.DeviceId;

            var heartRateKey = $"HeartRate::{deviceId}::{serviceName}::{characteristicName}";
            var batteryLevelKey = $"BatteryLevel::{deviceId}::{serviceName}::{characteristicName}";

            if (_listeners.ContainsKey(heartRateKey))
            {
                await _browserHubContext.Clients.All.SendAsync("heartrate", FormatValueHeartRateMeasurement(args.CharacteristicValue));
            }

            if (_listeners.ContainsKey(batteryLevelKey))
            {
                await _browserHubContext.Clients.All.SendAsync("battery-level", FormatValueBattery(args.CharacteristicValue));
            }
        }

        private string FormatAsString(IBuffer buffer)
        {
            CryptographicBuffer.CopyToByteArray(buffer, out byte[] data);

            try
            {
                 return $"{data}";
            }
            catch (ArgumentException)
            {
                return "";
            }
        }

        private string FormatValueBattery(IBuffer buffer)
        {
            // BT_Code: For the purpose of this sample, this function converts only UInt32 and
            // UTF-8 buffers to readable text. It can be extended to support other formats if your app needs them.
            CryptographicBuffer.CopyToByteArray(buffer, out byte[] data);

            try
            {
                // battery level is encoded as a percentage value in the first byte according to
                // https://www.bluetooth.com/specifications/gatt/viewer?attributeXmlFile=org.bluetooth.characteristic.battery_level.xml
                return $"{data[0]}%";
            }
            catch (ArgumentException)
            {
                return "0%";
            }
        }

        private string FormatValueHeartRateMeasurement(IBuffer buffer)
        {
            // BT_Code: For the purpose of this sample, this function converts only UInt32 and
            // UTF-8 buffers to readable text. It can be extended to support other formats if your app needs them.
            CryptographicBuffer.CopyToByteArray(buffer, out byte[] data);
            try
            {
                return ParseHeartRateValue(data).ToString();
            }
            catch (ArgumentException)
            {
                return "0";
            }
        }

        private static ushort ParseHeartRateValue(byte[] data)
        {
            // Heart Rate profile defined flag values
            const byte heartRateValueFormat = 0x01;

            byte flags = data[0];
            bool isHeartRateValueSizeLong = ((flags & heartRateValueFormat) != 0);

            if (isHeartRateValueSizeLong)
            {
                return BitConverter.ToUInt16(data, 1);
            }
            else
            {
                return data[1];
            }
        }

    }
}
