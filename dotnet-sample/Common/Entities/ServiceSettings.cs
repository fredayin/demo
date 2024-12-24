using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Entities
{
    public abstract class ServiceSettings
    {
        public string APIEndpointUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string BaseUrl { get; set; }
        public string Scope { get; set; }
    }
}
