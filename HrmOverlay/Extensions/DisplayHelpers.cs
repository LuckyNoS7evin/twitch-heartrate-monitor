using HrmOverlay.Enums;
using System;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace HrmOverlay.Extensions
{
    public static class DisplayHelpers
    {
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
