using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HrmOverlay.Models
{
    public class DeviceModel
    {
        public DeviceModel()
        {
            Services = new List<ServiceModel>();
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public List<ServiceModel> Services { get; set; }
    }
}
