using Azure.Data.Tables;
using Common.Entities;
using Microsoft.Extensions.Logging;


namespace Common.BLL
{
    public interface IFileInfoTableEntityMapper
    {
        public Entities.FileInfo MapToFileInfo(TableEntity tableEntity);
        public TableEntity MapToTableEntity(Entities.FileInfo fileInfo);
    }

    /// <summary>
    /// Maps fileinfo objects to and from table entity objects.
    /// </summary>
    public class FileInfoTableEntityMapper : IFileInfoTableEntityMapper
    {
        private readonly ILogger<FileInfoTableEntityMapper> _logger;

        public FileInfoTableEntityMapper(ILogger<FileInfoTableEntityMapper> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Maps table entity to fileinfo object.
        /// </summary>
        /// <param name="tableEntity">The table entity.</param>
        /// <returns></returns>
        public Entities.FileInfo MapToFileInfo(TableEntity tableEntity)
        {
            try
            {
                return new Entities.FileInfo()
                {
                    Id = Guid.Parse(tableEntity["RowKey"].ToString()!),
                    Name = tableEntity[nameof(Entities.FileInfo.Name)].ToString(),
                    UploadAt = DateTimeOffset.Parse(tableEntity[nameof(Entities.FileInfo.UploadAt)].ToString()!, default),
                    RecordCount = int.Parse(tableEntity[nameof(Entities.FileInfo.RecordCount)].ToString()!),
                    ReportOnly = bool.Parse(tableEntity[nameof(Entities.FileInfo.ReportOnly)].ToString()!),
                    Status = (FileStatus)Enum.Parse(typeof(FileStatus), tableEntity[nameof(Entities.FileInfo.Status)].ToString()!),
                    CompletedAt = DateTimeOffset.Parse(tableEntity[nameof(Entities.FileInfo.CompletedAt)].ToString()!, default),
                    RecordsProcessed = int.Parse(tableEntity[nameof(Entities.FileInfo.RecordsProcessed)].ToString()!)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(EventCodes.WebApp1.UI.Errors.UI_WEBAPP1_UNABLE_TO_MAP_TO_FILEINFO, ex, "Error Parsing Object.");

                return new Entities.FileInfo()
                {
                    Name = "Error Parsing Object",
                    RecordCount = 0,
                    UploadAt = DateTimeOffset.MinValue
                };
            }
        }

        /// <summary>
        /// Maps fileinfo object to  table entity.
        /// </summary>
        /// <param name="fileInfo">The fileinfo.</param>
        /// <returns></returns>
        public TableEntity MapToTableEntity(Entities.FileInfo fileInfo)
        {
            TableEntity entity = new TableEntity();

            var id = (fileInfo.Id == Guid.Empty) ? Guid.NewGuid() : fileInfo.Id;
            entity.RowKey = id.ToString();
            entity.PartitionKey = fileInfo.PartitionKey;

            var properties = typeof(Entities.FileInfo).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(NonKeyPropertyAttribute)));
            foreach (var prop in properties)
            {
                entity[prop.Name] = prop.GetValue(fileInfo).ToString();
            }

            return entity;
        }
    }
}
