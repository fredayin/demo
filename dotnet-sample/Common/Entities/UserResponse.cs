using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static Common.ConfigurationSections;

namespace Common.Entities
{
    public class UserResponse
    {
        [JsonPropertyName("id")]
        public string ID { get; set; }

        [JsonPropertyName("href")]
        public string Href { get; set; }

        [JsonPropertyName("validFor")]
        public ValidForDates ValidFor { get; set; }

        [JsonPropertyName("credential")]
        public Credentials Credential { get; set; }

        public class ValidForDates
        {
            [JsonPropertyName("startDateTime")]
            public DateTimeOffset? StartDateTime { get; set; }

            [JsonPropertyName("endDateTime")]
            public DateTimeOffset? EndDateTime { get; set; }
        }

        public class Credentials
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("state")]
            public string State { get; set; }

            [JsonPropertyName("firstName")]
            public string FirstName { get; set; }

            [JsonPropertyName("lastName")]
            public string LastName { get; set; }

            [JsonPropertyName("contactNumber")]
            public string ContactNumber { get; set; }

            [JsonPropertyName("validFor")]
            public ValidForDates ValidFor { get; set; }

        }

    }

    

    
}
