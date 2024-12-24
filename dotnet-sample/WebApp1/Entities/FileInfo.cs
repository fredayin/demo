using Microsoft.AspNetCore.Http;

namespace Common.Entities
{
    public enum FileStatus
    {
        Processing,
        NotProcessed,
        Completed,
        Error
    }

    public class FileInfo
    {
        private DateTimeOffset uploadAtUTC;
        private string partitionKey = "UploadFile";

        public Guid Id { get; set; }
        public string PartitionKey { get { return partitionKey; } set { partitionKey = value; } }
        [NonKeyProperty]
        public string? Name { get; set; }
        [NonKeyProperty]
        public DateTimeOffset UploadAt { get { return uploadAtUTC; } set { uploadAtUTC = value; } }
        [NonKeyProperty]
        public int RecordCount { get; set; }
        [NonKeyProperty]
        public bool ReportOnly { get; set; }
        [NonKeyProperty]
        public int RecordsProcessed { get; set; }
        [NonKeyProperty]
        public DateTimeOffset CompletedAt { get; set; }
        [NonKeyProperty]
        public FileStatus Status { get; set; }

        public FileInfo()
        {

        }

        public FileInfo(IFormFile file)
        {
            Id = Guid.Empty;
            Name = file.FileName.Replace(".txt", String.Empty);
            uploadAtUTC = DateTimeOffset.Now;
            Status = FileStatus.NotProcessed;
        }
    }
}
