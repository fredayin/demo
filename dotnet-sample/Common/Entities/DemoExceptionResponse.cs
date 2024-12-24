using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.Entities
{
    public class DemoExceptionResponse
    {
        [JsonPropertyName("version")]
        public string Version { get; set; } = "1.0";

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("requestId")]
        public string RequestId { get; set; }

        [JsonPropertyName("userMessage")]
        public string UserMessage { get; set; }

        [JsonPropertyName("developerMessage")]
        public string DeveloperMessage { get; set; }

        [JsonPropertyName("moreInfo")]
        public string MoreInfo { get; set; }
    }
}
