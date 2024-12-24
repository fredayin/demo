using Azure.Data.Tables;
using Azure.Identity;
using Common.Entities;


namespace Common.BLL
{
    public interface ITableClientFactory
    {
        TableClient GetTableClient(Type entityType);
    }

    public class TableClientFactory : ITableClientFactory
    {
        private readonly TableStorageServiceOptions _options;

        public TableClientFactory(TableStorageServiceOptions options)
        {
            _options = options;
        }

        public TableClient GetTableClient(Type entityType)
        {
            var fileInfo = new Uri($"https://{_options.AccountName}.table.core.windows.net/FileInfo");
            var successUrl = new Uri($"https://{_options.AccountName}.table.core.windows.net/Success");
            var userCountUrl = new Uri($"https://{_options.AccountName}.table.core.windows.net/UserCount");
            var tokenCreds = new ChainedTokenCredential(new AzureCliCredential(), new ManagedIdentityCredential());


            if (entityType == typeof(Entities.FileInfo))
            {
                return new TableClient(fileInfo, "FileInfo", tokenCredential: tokenCreds);
            }

            if (entityType == typeof(Success))
            {
                return new TableClient(successUrl, "Success", tokenCredential: tokenCreds);
            }

            if (entityType == typeof(UserCountResult))
            {
                return new TableClient(userCountUrl, "UserCount", tokenCredential: tokenCreds);
            }

            throw new ArgumentException("A non supported entity type was used");

        }
    }
}
