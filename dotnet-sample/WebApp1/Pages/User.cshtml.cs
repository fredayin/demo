using Ardalis.GuardClauses;
using Azure.Messaging.ServiceBus;
using Common;
using Common.Entities;
using Common.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Azure;
using System.Net;
using WebApp1.BLL;

namespace WebApp1.Pages
{
    public class UserModel : PageModel
    {

        [BindProperty]
        public string UserName { get; set; }

        public bool? UserFound { get; set; }
        public string CreatedDateTime { get; set; }
        public string LastLogInDateTime { get; set; }
        public string LastPasswordChangeDateTime { get; set; }
        public string AccountEnabled { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ObjectId { get; set; }

        private readonly IUserService _userService;
        private readonly ILogger<UserModel> _logger;
        private readonly ServiceBusClient _serviceBusClient;
        private ServiceBusSender _serviceBusSender;


        public UserModel(IUserService userService,
            ILogger<UserModel> logger,
            ServiceBusConfig serviceBusConfig,
            IAzureClientFactory<ServiceBusClient> serviceBusClientFactory)
        {
            _userService = userService;
            _logger = logger;
            _serviceBusClient = serviceBusClientFactory.CreateClient(Constants.ServiceBusIdentifier);
        }

        public void OnGet()
        {
            Page();
        }

        public async Task<IActionResult> OnPost(string username)
        {
            var model = await _userService.GetUserModel(username.ToLower());
            _logger.LogInformation(EventCodes.WebApp1.UI.Trace.UI_WEBAPP1_USER_TOOL_USER_SEARCHED, "user: {user} searched for username: {username}", User.Identity?.Name, username);
            if (model.UserFound)
            {
                UserFound = true;
                ObjectId = model.ObjectId;
                CreatedDateTime = model.Created;
                LastLogInDateTime = model.LastLogIn;
                AccountEnabled = model.AccountEnabled;
                FirstName = model.FirstName;
                LastName = model.LastName;
            }
            else
            {
                UserFound = false;
            }

            return Page();
        }

        public async Task<IActionResult> OnGetMarkDisabled(string objectId)
        {
            var result = new HttpResponseMessage();

            Guard.Against.NullOrEmpty(objectId, nameof(objectId), $"Username must be provided");

            var request = new UpdateUserRequest()
            {
                CorrelationId = Guid.NewGuid().ToString(),
                AccountEnabled = false
            };

            try
            {

                var response = await _userService.Patch(Guid.Parse(objectId), request, HttpContext);
                _logger.LogInformation(EventCodes.WebApp1.UI.Trace.UI_WEBAPP1_USER_TOOL_ACCOUNT_MARKED_AS_DISABLED, "user by objectId : {objectId} marked as disabled", objectId);

                //result.Content = new StringContent(JsonConvert.SerializeObject(response));

                return new OkResult();
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogInformation(EventCodes.WebApp1.UI.Errors.UI_WEBAPP1_USER_TOOL_ACCOUNT_MARKED_AS_DISABLED_FAILED, "Failed to disable user. Request: {request}. Error: {error} StatusCode: {statusCode}", request.ToJson(), httpEx.Message, httpEx.StatusCode);

                return HandleMarkingResponse(httpEx.StatusCode.Value);

            }
            catch (Exception ex)
            {
                _logger.LogInformation(EventCodes.WebApp1.UI.Errors.UI_WEBAPP1_USER_TOOL_ACCOUNT_MARKED_AS_DISABLED_FAILED, "Failed to disable user. Request: {request}. Error: {error}", request.ToJson(), ex.Message);

                return HandleMarkingResponse(HttpStatusCode.BadRequest);
            }
        }

        private IActionResult HandleMarkingResponse(HttpStatusCode statusCode)
        {
            var httpResponse = new ObjectResult(new { message = "http request exception" });
            httpResponse.StatusCode = (int)statusCode;
            httpResponse.ContentTypes.Add("application/json");// Set the Content-Type header to application/json

            if (statusCode == HttpStatusCode.Unauthorized)
            {
                httpResponse.StatusCode = (int)HttpStatusCode.Unauthorized;
                httpResponse.Value = HttpStatusCode.Unauthorized.ToString();
                return httpResponse;
            }

            if (statusCode == HttpStatusCode.Forbidden)
            {
                httpResponse.StatusCode = (int)HttpStatusCode.Forbidden;
                httpResponse.Value = HttpStatusCode.Forbidden.ToString();

                return httpResponse;

            }

            return httpResponse;

        }
    }
}
