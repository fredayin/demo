using Azure;
using Azure.Data.Tables;
using Common.Entities;

namespace Common.BLL
{
    public interface ITableStorageService
    {
        public IEnumerable<Entities.FileInfo> GetAllFileInfo();

        public Entities.FileInfo GetFileInfo(Guid fileId);

        public Guid SaveFileInfo(Entities.FileInfo fileInfo);

        public void DeleteFileInfo(Entities.FileInfo fileInfo);

        public IEnumerable<Success> GetAllSuccessForFile(Guid fileId);

        public void DeleteSuccess(Success success);

        public Guid SaveSuccess(Success success);

        public Task SaveUserCount(UserCountResult result);
    }

    public class TableStorageService : ITableStorageService
    {
        private readonly ILogger<TableStorageService> _logger;
        private TableClient _fileInfoTableClient;
        private TableClient _successTableClient;
        private readonly TableClient _userCountTableClient;
        private IFileInfoTableEntityMapper _fileInfoMapper;
        private ISuccessTableEntityMapper _successMapper;
        private ITableClientFactory _tableClientFactory;

        public TableStorageService(ILogger<TableStorageService> logger, ITableClientFactory tableClientFactory, IFileInfoTableEntityMapper fileInfoTableEntityMapper, ISuccessTableEntityMapper successTableEntityMapper)
        {
            _logger = logger;
            _tableClientFactory = tableClientFactory;
            _fileInfoTableClient = _tableClientFactory.GetTableClient(typeof(Entities.FileInfo));
            _successTableClient = _tableClientFactory.GetTableClient(typeof(Success));
            _userCountTableClient = _tableClientFactory.GetTableClient(typeof(UserCountResult));
            _fileInfoMapper = fileInfoTableEntityMapper;
            _successMapper = successTableEntityMapper;
        }

        public void DeleteFileInfo(Entities.FileInfo fileInfo)
        {
            _fileInfoTableClient.DeleteEntity(fileInfo.PartitionKey, fileInfo.Id.ToString());
            _logger.LogTrace(EventCodes.WebApp1.UI.Trace.UI_WEBAPP1_DELETE_FILE_INFO, $"File Info ({fileInfo.Id}) was deleted.");
        }

        public void DeleteSuccess(Success success)
        {
            _successTableClient.DeleteEntity(success.FileInfoId.ToString(), success.Id.ToString());
            _logger.LogTrace(EventCodes.WebApp1.UI.Trace.UI_WEBAPP1_DELETE_SUCCESS, $"Success ({success.Id}) was deleted.");
        }

        public IEnumerable<Entities.FileInfo> GetAllFileInfo()
        {
            Pageable<TableEntity> entities = _fileInfoTableClient.Query<TableEntity>();
            return entities.Select(e => _fileInfoMapper.MapToFileInfo(e)).OrderByDescending(x => x.UploadAt);
        }

        public Entities.FileInfo GetFileInfo(Guid fileId)
        {
            var entity = _fileInfoTableClient.GetEntity<TableEntity>("UploadFile", fileId.ToString()).Value;
            return _fileInfoMapper.MapToFileInfo(entity);
        }

        public IEnumerable<Success> GetAllSuccessForFile(Guid fileId)
        {
            Pageable<TableEntity> entities = _successTableClient.Query<TableEntity>($"PartitionKey eq '{fileId}'");
            return entities.Select(e => _successMapper.MapToSuccess(e)).OrderByDescending(x => x.DateMatched);
        }

        public Guid SaveFileInfo(Entities.FileInfo fileInfo)
        {
            var entity = _fileInfoMapper.MapToTableEntity(fileInfo);
            _fileInfoTableClient.UpsertEntity(entity);
            _logger.LogTrace(EventCodes.WebApp1.UI.Trace.UI_WEBAPP1_SAVE_FILE_INFO, $"File Info ({entity["RowKey"]}) was saved.");
            return Guid.Parse(entity["RowKey"].ToString()!);
        }

        public Guid SaveSuccess(Success success)
        {
            var entity = _successMapper.MapToTableEntity(success);
            _successTableClient.UpsertEntity(entity);
            _logger.LogTrace(EventCodes.WebApp1.UI.Trace.UI_WEBAPP1_SAVE_SUCCESS, $"Success ({entity["RowKey"]}) was saved.");
            return Guid.Parse(entity["RowKey"].ToString()!);
        }

        public async Task SaveUserCount(UserCountResult result)
        {
            var entity = new TableEntity();

            var id = Guid.NewGuid();
            entity.RowKey = id.ToString();
            entity.PartitionKey = result.TenantId.ToString();
            var properties = typeof(UserCountResult).GetProperties();
            foreach (var prop in properties)
            {
                entity[prop.Name] = prop.GetValue(result);
            }

            await _userCountTableClient.AddEntityAsync(entity);
        }

    }
}
