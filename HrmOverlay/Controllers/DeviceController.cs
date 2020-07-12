using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HrmOverlay.Extensions;
using HrmOverlay.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;

namespace HrmOverlay.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        [HttpGet]
        public async Task<List<DeviceModel>> Get()
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

        [HttpGet("{id}")]
        public async Task<DeviceModel> Get(string id)
        {
            var result = await BluetoothLEDevice.FromIdAsync(id);

            var device = new DeviceModel
            {
                Id = result.DeviceId,
                Name = result.Name
            };

            var gattResult = await result.GetGattServicesAsync(BluetoothCacheMode.Uncached);
            if (gattResult.Status == GattCommunicationStatus.Success)
            {
                var services = gattResult.Services;
                device.Services = new List<ServiceModel>();
                foreach (var service in services)
                {
                    var serviceModel = new ServiceModel
                    {
                        Id = service.Uuid.ToString(),
                        Name = service.GetServiceName()
                    };

                    var chars = await GetCharacteristics(service);
                    if (chars != null)
                    {
                        foreach (var character in chars)
                        {
                            serviceModel.Characteristics.Add(character.GetCharacteristicName());
                        }
                        device.Services.Add(serviceModel);
                    }
                }
            }

            return device;
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

    }
}
