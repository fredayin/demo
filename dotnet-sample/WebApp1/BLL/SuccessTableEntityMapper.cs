using Azure.Data.Tables;
using Common.Entities;
using Microsoft.Extensions.Logging;


namespace Common.BLL
{
    public interface ISuccessTableEntityMapper
    {
        public Success MapToSuccess(TableEntity tableEntity);
        public TableEntity MapToTableEntity(Success success);
    }

    public class SuccessTableEntityMapper : ISuccessTableEntityMapper
    {
        private readonly ILogger<SuccessTableEntityMapper> _logger;

        public SuccessTableEntityMapper(ILogger<SuccessTableEntityMapper> logger)
        {
            _logger = logger;
        }

        public Success MapToSuccess(TableEntity tableEntity)
        {
            try
            {
                return new Success()
                {
                    Id = Guid.Parse(tableEntity["RowKey"].ToString()!),
                    FileInfoId = Guid.Parse(tableEntity["PartitionKey"].ToString()!),
                    BatchId = Guid.Parse(tableEntity[nameof(Success.BatchId)].ToString()!),
                    UserName = tableEntity[nameof(Success.UserName)].ToString()!,
                    DateMatched = DateTimeOffset.Parse(tableEntity[nameof(Success.DateMatched)].ToString()!, default)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(EventCodes.WebApp1.UI.Errors.UI_WEBAPP1_UNABLE_TO_MAP_TO_SUCCESS, ex, "Error Parsing Object.");

                return new Success()
                {
                    UserName = "Error Parsing Object.",
                };
            }
        }

        public TableEntity MapToTableEntity(Success success)
        {
            var entity = new TableEntity();

            var id = (success.Id == Guid.Empty) ? Guid.NewGuid() : success.Id;
            entity.RowKey = id.ToString();
            entity.PartitionKey = success.FileInfoId.ToString();

            var properties = typeof(Success).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(NonKeyPropertyAttribute)));
            foreach (var prop in properties)
            {
                entity[prop.Name] = prop.GetValue(success);
            }

            return entity;
        }
    }
}
