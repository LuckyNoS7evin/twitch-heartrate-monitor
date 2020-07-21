using HrmOverlay.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;

namespace HrmOverlay.Extensions
{
    public static class DisplayHelpers
    {
        public static async Task<GattDeviceServicesResult> GetServices(this BluetoothLEDevice device)
        {
            var result = await device.GetGattServicesAsync(BluetoothCacheMode.Uncached);
            return result;
        }

        public static async Task<IReadOnlyList<GattCharacteristic>> GetCharacteristics(this GattDeviceService service)
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

        public static string GetServiceName(this GattDeviceService service)
        {
            if (service.Uuid.IsSigDefinedUuid())
            {
                GattNativeServiceUuid serviceName;
                if (Enum.TryParse(service.Uuid.ConvertUuidToShortId().ToString(), out serviceName))
                {
                    return serviceName.ToString();
                }
            }
            return "Custom Service: " + service.Uuid;
        }

        public static string GetCharacteristicName(this GattCharacteristic characteristic)
        {
            if (characteristic.Uuid.IsSigDefinedUuid())
            {
                GattNativeCharacteristicUuid characteristicName;
                if (Enum.TryParse(characteristic.Uuid.ConvertUuidToShortId().ToString(),
                    out characteristicName))
                {
                    return characteristicName.ToString();
                }
            }

            if (!string.IsNullOrEmpty(characteristic.UserDescription))
            {
                return characteristic.UserDescription;
            }

            else
            {
                return "Custom Characteristic: " + characteristic.Uuid;
            }
        }
    }
}
