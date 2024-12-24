using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.Entities
{
    public class ValidateResponse
    {
        [JsonIgnore]
        public bool IsValid { get; set; } = true;

        [JsonIgnore]
        public List<ValidationFailure> Errors { get; set; }

        [JsonPropertyName("httpStatus")]
        public int HTTPStatus { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("userName")]
        public string UserName { get; set; }

        [JsonPropertyName("userStatus")]
        public string UserStatus { get; set; }

        [JsonPropertyName("uuid")]
        public string UUID { get; set; }

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [JsonPropertyName("primaryEmail")]
        public string PrimaryEmail { get; set; }

        [JsonPropertyName("alternateEmail")]
        public string AlternateEmail { get; set; }

        [JsonPropertyName("recoveryEmail")]
        public string RecoveryEmail { get; set; }

        [JsonPropertyName("userMessage")]
        public string UserMessage { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }
    }
}
