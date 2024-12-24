using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Entities
{
    public class AzureADIdentityProvider
    {
        public string AuthSchemeName { get; set; }
        public AzureADIdentityItem AzureAD { get; set; }
        public string ApplicationHostUrl { get; set; }

        public AzureADIdentityProvider()
        {
            AzureAD = new AzureADIdentityItem();
        }
    }
}
