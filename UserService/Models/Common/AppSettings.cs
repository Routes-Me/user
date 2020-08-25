using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserService.Models.Common
{
    public class AppSettings
    {
        public string Secret { get; set; }
        public string ValidAudience { get; set; }
        public string ValidIssuer { get; set; }
        public string VehicleEndpointUrl { get; set; }
    }
}
