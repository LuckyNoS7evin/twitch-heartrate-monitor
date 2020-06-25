using HrmOverlay.Extensions;
using HrmOverlay.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace HrmOverlay.HostedServices
{
    public class HostedService : IHostedService
    {
        private readonly IHubContext<HeartrateHub> _browserHubContext;
        private BluetoothLEDevice device;
        private GattCharacteristic hrmCharacteristic;
        private GattCharacteristic batteryCharacteristic;

        public HostedService(IHubContext<HeartrateHub> browserHubContext)
        {
            _browserHubContext = browserHubContext;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var result = await DeviceInformation.FindAllAsync(BluetoothLEDevice.GetDeviceSelectorFromPairingState(true));
            foreach (var item in result)
            {
                var hrm = await BluetoothLEDevice.FromIdAsync(item.Id);
                device = hrm;
                var gattResult = await device.GetGattServicesAsync(BluetoothCacheMode.Uncached);
                if (gattResult.Status == GattCommunicationStatus.Success)
                {
                    var services = gattResult.Services;
                    //rootPage.NotifyUser(String.Format("Found {0} services", services.Count), NotifyType.StatusMessage);
                    foreach (var service in services)
                    {
                        if (service.GetServiceName() == "Battery")
                        {
                            var chars = await GetCharacteristics(service);
                            foreach (var character in chars)
                            {
                                if (character.GetCharacteristicName() == "BatteryLevel")
                                {
                                    var cccdValue = GattClientCharacteristicConfigurationDescriptorValue.None;
                                    if (character.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Indicate))
                                    {
                                        cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Indicate;
                                    }

                                    else if (character.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
                                    {
                                        cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Notify;
                                    }

                                    // BT_Code: Must write the CCCD in order for server to send indications.
                                    // We receive them in the ValueChanged event handler.
                                    var statusBattery = await character.WriteClientCharacteristicConfigurationDescriptorAsync(cccdValue);
                                    if (statusBattery == GattCommunicationStatus.Success)
                                    {
                                        batteryCharacteristic = character;
                                        batteryCharacteristic.ValueChanged += BatteryCharacteristic_ValueChanged;
                                        //await ReadBattery();
                                    }

                                }
                            }
                        }

                        if (DisplayHelpers.GetServiceName(service) == "HeartRate")
                        {
                            var chars = await GetCharacteristics(service);
                            foreach (var character in chars)
                            {
                                if (DisplayHelpers.GetCharacteristicName(character) == "HeartRateMeasurement")
                                {
                                    var cccdValue = GattClientCharacteristicConfigurationDescriptorValue.None;
                                    if (character.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Indicate))
                                    {
                                        cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Indicate;
                                    }

                                    else if (character.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
                                    {
                                        cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Notify;
                                    }

                                    // BT_Code: Must write the CCCD in order for server to send indications.
                                    // We receive them in the ValueChanged event handler.
                                    var statusHR = await character.WriteClientCharacteristicConfigurationDescriptorAsync(cccdValue);
                                    if (statusHR == GattCommunicationStatus.Success)
                                    {
                                        hrmCharacteristic = character;
                                        hrmCharacteristic.ValueChanged += HrmCharacteristic_ValueChanged;
                                    }
                                }


                            }
                        }

                    }
                }
            }
            Console.WriteLine("Subscribing Done");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        //private async Task ReadBattery()
        //{
        //    // BT_Code: Read the actual value from the device by using Uncached.
        //    var result = await batteryCharacteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
        //    if (result.Status == GattCommunicationStatus.Success)
        //    {
        //        Console.WriteLine(FormatValueBattery(result.Value));
        //    }
        //}

        private async  void BatteryCharacteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            await _browserHubContext.Clients.All.SendAsync("battey-level", FormatValueBattery(args.CharacteristicValue));
        }

        private async void HrmCharacteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            await _browserHubContext.Clients.All.SendAsync("heartrate", FormatValueHeartRateMeasurement(args.CharacteristicValue));
        }

        private async Task<IReadOnlyList<GattCharacteristic>> GetCharacteristics(GattDeviceService service)
        {
            var accessStatus = await service.RequestAccessAsync();
            if (accessStatus == DeviceAccessStatus.Allowed)
            {
                // BT_Code: Get all the child characteristics of a service. Use the cache mode to specify uncached characterstics only 
                // and the new Async functions to get the characteristics of unpaired devices as well. 
                var result = await service.GetCharacteristicsAsync(BluetoothCacheMode.Uncached);
                if (result.Status == GattCommunicationStatus.Success)
                {
                    return result.Characteristics;
                }
            }
            return null;
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
