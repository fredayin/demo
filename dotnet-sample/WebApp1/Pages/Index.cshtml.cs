using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Common.BLL;
using Common;
using ServiceStack.Text;
using System.Text;

namespace WebApp1.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IFileProcessor _fileProcessor;
        private readonly ITableStorageService _tableStorageService;

        public IndexModel(ILogger<IndexModel> logger,
            IFileProcessor fileProcessor,
            ITableStorageService tableStorageService)
        {
            _logger = logger;
            _fileProcessor = fileProcessor;
            _tableStorageService = tableStorageService;
        }

        public IFormFile FormFile { get; set; }

        public bool ReportOnly { get; set; }

        public List<Common.Entities.FileInfo> UploadedFiles { get; set; }

        public async Task<ActionResult> OnGet()
        {
            UploadedFiles = _tableStorageService.GetAllFileInfo().ToList();

            return Page();
        }

        public ActionResult OnPost()
        {
            if (this.ModelState.IsValid)
            {
                _fileProcessor.Start(FormFile, ReportOnly);
                return RedirectToPage("/index");
            }

            UploadedFiles = _tableStorageService.GetAllFileInfo().ToList();

            return Page();
        }

        public IActionResult OnGetDownloadCsv(string fileName, Guid fileId)
        {
            try
            {

                var successes = _tableStorageService.GetAllSuccessForFile(fileId);

                var serializedString = CsvSerializer.SerializeToString(successes);
                var stream = new MemoryStream(Encoding.UTF8.GetBytes(serializedString));
                FileStreamResult result = new FileStreamResult(stream, "text/csv")
                {
                    FileDownloadName = $"Download-Report.csv"
                };
                _logger.LogInformation(EventCodes.WebApp1.UI.Trace.UI_WEBAPP1_CSV_DOWNLOAD, "filename: {fileName} downloaded by user: {user}", fileName, User.Identity?.Name);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogInformation(EventCodes.WebApp1.UI.Errors.UI_WEBAPP1_FAILURE_DOWNLOADING_FILE, "user: {user} encountered an error downloading {filename}. Error message: {errorMessage}", User.Identity?.Name, fileName, ex.Message);
                return Page();
            }
        }
    }
}
