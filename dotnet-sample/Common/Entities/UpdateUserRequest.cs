using Azure.Messaging.ServiceBus.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.Entities
{
    public class UpdateUserRequest
    {
        [JsonPropertyName("accountEnabled")]
        public bool? AccountEnabled { get; set; }

        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }

        [JsonPropertyName("LastName")]
        public string? LastName { get; set; }

        public string? CorrelationId { get; set; }
    }
}
