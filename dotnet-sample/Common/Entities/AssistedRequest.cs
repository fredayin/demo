using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FluentValidation.Results;

namespace Common.Entities
{
    public class AssistedRequest
    {
        [JsonIgnore]
        public bool IsValid { get; set; } = true;

        [JsonIgnore]
        public List<ValidationFailure> Errors { get; set; }

        [JsonPropertyName("httpStatus")]
        public int HTTPStatus { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("referenceId")]
        public string ReferenceId { get; set; }

        [JsonPropertyName("serviceName")]
        public string ServiceName { get; set; }

        [JsonPropertyName("clientId")]
        public Guid? ClientId { get; set; }

        [JsonPropertyName("redirectUri")]
        public string RedirectUri { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; }
    }
}
