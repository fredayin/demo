using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.Entities
{
    public class UserInfoResponse
    {
        [JsonPropertyName("uuid")]
        public string UUID { get; set; }

        [JsonPropertyName("userName")]
        public string UserName { get; set; }

        [JsonPropertyName("userStatus")]
        public string UserStatus { get; set; }

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [JsonPropertyName("primaryEmail")]
        public string PrimaryEmail { get; set; }

        [JsonPropertyName("alternateEmail")]
        public string AlternateEmail { get; set; }
    }
}
