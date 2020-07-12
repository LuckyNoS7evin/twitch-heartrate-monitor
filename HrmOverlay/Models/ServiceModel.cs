using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HrmOverlay.Models
{
    public class ServiceModel
    {
        public ServiceModel ()
        {
            Characteristics = new List<string>();
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> Characteristics { get; set; }
    }
}
