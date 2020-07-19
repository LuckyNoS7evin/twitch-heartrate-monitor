using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HrmOverlay.Extensions;
using HrmOverlay.Managers;
using HrmOverlay.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;

namespace HrmOverlay.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly BleListener _bleListener;
        public DeviceController(BleListener bleListener)
        {
            _bleListener = bleListener;
        }

        [HttpGet]
        public Task<List<DeviceModel>> Get()
        {
            return _bleListener.GetDevices();
        }

        [HttpGet("{id}")]
        public Task<DeviceModel> Get(string id)
        {
            return _bleListener.GetDevice(id);
        }

        [HttpGet("Listeners")]
        public List<ListenerModel> Listeners(string id)
        {
            return _bleListener.GetListeners();
        }

        [HttpPost("HeartRate/{id}/{service}/{characteristic}")]
        public Task PostHeartRate(string id, string service, string characteristic)
        {
            return _bleListener.Listen("HeartRate", id, service, characteristic);
        }

        [HttpPost("BatteryLevel/{id}/{service}/{characteristic}")]
        public Task PostBatteryLevel(string id, string service, string characteristic)
        {
            return _bleListener.Listen("BatteryLevel", id, service, characteristic);
        }

        [HttpDelete("HeartRate/{id}/{service}/{characteristic}")]
        public Task DeleteHeartRate(string id, string service, string characteristic)
        {
            return _bleListener.Unlisten("HeartRate", id, service, characteristic);
        }

        [HttpDelete("BatteryLevel/{id}/{service}/{characteristic}")]
        public Task DeleteBatteryLevel(string id, string service, string characteristic)
        {
            return _bleListener.Unlisten("BatteryLevel", id, service, characteristic);
        }
    }
}
