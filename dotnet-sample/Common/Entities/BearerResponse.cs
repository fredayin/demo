using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.Entities
{
    public class BearerResponse
    {
        public BearerResponse() { this.received = DateTime.UtcNow; }
        public BearerResponse(DateTime received) { this.received = received; }

        private DateTime received;

        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }

        [JsonPropertyName("tokenType")]
        public string TokenType { get; set; }

        /// <summary>
        /// The OAuth 2.0 standard, RFC 6749, defines the expires_in field as the number of seconds to expiration: 
        /// </summary>
        [JsonPropertyName("expiresIn")]
        public int ExpiresIn { get; set; }

        [JsonIgnore]
        public DateTime Received { get { return this.received; } }

        [JsonIgnore]
        public bool HasExpired { get { return DateTime.UtcNow > this.received.AddSeconds(this.ExpiresIn); } }

        [JsonIgnore]
        public DateTime ExpiresAt { get { return this.received.AddSeconds(this.ExpiresIn); } }
    }
}
