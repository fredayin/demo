using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Entities
{
    public class AzureADIdentityItem
    {
        public string Instance { get; set; }

        public string Domain { get; set; }

        public Guid TenantId { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string Scope { get; set; }

        public string AuthorityHostUrl { get; set; }
        
        public string Url { get; set; }
    }
}
