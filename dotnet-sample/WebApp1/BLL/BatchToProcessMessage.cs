using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.BLL
{
    public class BatchToProcessMessage
    {
        [JsonPropertyName("batchId")]
        public Guid BatchId { get; set; }

        [JsonPropertyName("fileId")]
        public Guid FileId { get; set; }

        [JsonPropertyName("items")]
        public List<KeyValuePair<string, string>>? Items { get; set; }

        [JsonPropertyName("count")]
        public int Count => Items!.Count;

        [JsonPropertyName("reportOnly")]
        public bool ReportOnly { get; set; }

        [JsonPropertyName("fileMessagePosition")]
        public FileMessagePosition FileMessagePosition { get; set; }

    }

    public enum FileMessagePosition
    {
        [Description("First Message")]
        First = 0,

        [Description("Intermediate Message")]
        Intermediate = 1,

        [Description("Last Message")]
        Last = 2
    }
}
