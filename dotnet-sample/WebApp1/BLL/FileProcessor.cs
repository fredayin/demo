using Common.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.ServiceBus;
using WebApp1.Model;

namespace Common.BLL
{
    public interface IFileProcessor
    {
        public void Start(IFormFile file, bool reportOnly);
        public List<string> StartBatchSearch(IFormFile file);
    }

    public class FileProcessor : IFileProcessor
    {
        private readonly ILogger<FileProcessor> _logger;
        private readonly IStreamReader _reader;
        private readonly ITableStorageService _tableStorageService;
        private readonly ServiceBusSender _serviceBusSender;
        private readonly ServiceBusConfig _serviceBusConfig;
        private readonly WebApp1Options _options;

        public FileProcessor(ILogger<FileProcessor> logger,
            IStreamReader reader,
            ITableStorageService tableStorageService,
            IAzureClientFactory<ServiceBusClient> serviceBusClientFactory,
            WebApp1Options options,
            ServiceBusConfig serviceBusConfig)
        {
            _logger = logger;
            _reader = reader;
            _tableStorageService = tableStorageService;
            _serviceBusConfig = serviceBusConfig;
            ServiceBusClient serviceBusClient = serviceBusClientFactory.CreateClient(Common.Constants.ServiceBusIdentifier);
            _serviceBusSender = serviceBusClient.CreateSender(_serviceBusConfig.Queues.ProcessCompromisedFile);

            _options = options;
        }

        public void Start(IFormFile file, bool reportOnly)
        {
            using (var reader = _reader.GetReader(file.OpenReadStream()))
            {
                _logger.LogTrace(EventCodes.WebApp1.UI.Trace.UI_WEBAPP1_FILE_UPLOAD_STARTED, "File Upload Started.");

                var recordCount = 0;
                var fileInfo = new Entities.FileInfo(file)
                {
                    ReportOnly = reportOnly,
                    Status = FileStatus.NotProcessed,
                };
                fileInfo.Id = _tableStorageService.SaveFileInfo(fileInfo);

                while (reader.Peek() >= 0)
                {
                    var message = GenerateBatch(reader, fileInfo.Id, reportOnly);
                    recordCount += message.Count;
                    var serviceMessage = new ServiceBusMessage(System.Text.Json.JsonSerializer.Serialize(message));
                    try
                    {
                        _serviceBusSender.SendMessageAsync(serviceMessage);
                    }
                    catch (UnauthorizedAccessException unAuthEx)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(EventCodes.WebApp1.UI.Errors.UI_WEBAPP1_BATCH_MESSAGE_NOT_SENT, ex, "Batch not sent to queue. FileId: {fileId}. Message: {message}", fileInfo.Id, serviceMessage);
                    }
                }

                fileInfo.RecordCount = recordCount;
                _tableStorageService.SaveFileInfo(fileInfo);

                _logger.LogTrace(EventCodes.WebApp1.UI.Trace.UI_WEBAPP1_FILE_UPLOAD_FINISHED, "File processed, valid messages sent to queue. FileId: {fileId}.", fileInfo.Id);
            }
        }

        public List<string> StartBatchSearch(IFormFile file)
        {
            List<string> userNameList = new List<string>();
            using (var reader = _reader.GetReader(file.OpenReadStream()))
            {
                _logger.LogTrace(EventCodes.WebApp1.UI.Trace.UI_WEBAPP1_FILE_UPLOAD_STARTED, "File Upload Started.");

                while (reader.Peek() >= 0)
                {
                    var line = reader.ReadLine();
                    if (!String.IsNullOrEmpty(line))
                    {
                        userNameList.Add(line);
                    }
                }

                _logger.LogTrace(EventCodes.WebApp1.UI.Trace.UI_WEBAPP1_FILE_UPLOAD_FINISHED, "File processed, valid messages sent to queue. FileId: {fileId}.");
            }
            return userNameList;
        }



        private BatchToProcessMessage GenerateBatch(StreamReader reader, Guid FileId, bool reportOnly)
        {
            _logger.LogTrace(EventCodes.WebApp1.UI.Trace.UI_WEBAPP1_BATCH_GENERATION_STARTED, "Batch generation started. FileId: {fileId}.", FileId);

            var batchItems = new List<KeyValuePair<string, string>>();

            for (int i = 0; i < _options.MaxBatchSize; i++)
            {
                if (reader.Peek() <= 0)
                {
                    break;
                }
                else
                {
                    var line = reader.ReadLine();
                    if (line == String.Empty || !line.Contains(':'))
                    {
                        i--;
                        _logger.LogTrace(EventCodes.WebApp1.UI.Trace.UI_WEBAPP1_BATCH_GENERATION_LINE_SKIPPED, "Batch generation, line skipped. Line: '{lineskiped}'. FileId: {fileId}.", line, FileId);
                    }
                    else
                    {
                        batchItems.Add(CreateKeyValuePair(line));
                    }
                }
            }

            _logger.LogTrace(EventCodes.WebApp1.UI.Trace.UI_WEBAPP1_BATCH_GENERATION_FINISHED, "Batch generation finished. BatchSize: {batchSize}. FileId: {fileId}.", batchItems.Count, FileId);

            return new BatchToProcessMessage() { BatchId = Guid.NewGuid(), FileId = FileId, Items = batchItems, ReportOnly = reportOnly };
        }

        private KeyValuePair<string, string> CreateKeyValuePair(string row)
        {
            var values = row.Split(':', 2);

            return new KeyValuePair<string, string>(values[0], values[1]);
        }
    }
}
