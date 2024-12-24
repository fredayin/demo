using Asp.Versioning;
using Common;
using Common.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Route("v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly UserService _validateService;

        public AuthController(ILogger<AuthController> logger,
                              UserService validateService)
        {
            _logger = logger;
            _validateService = validateService;

            _logger.LogInformation(EventCodes.AuthCheck.Trace.AUTHCONTROLLER_CONSTRUCTOR, "Authentication Controller");
        }

        [MapToApiVersion("1.0")]
        [HttpPost]
        [ProducesResponseType(typeof(ValidateResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ConflictObjectResult), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> PostAsync([FromBody] ValidateRequest content)
        {
            _logger.LogInformation(EventCodes.AuthCheck.Trace.VALIDATECREDENTIALS_NEWREQUEST, "Authentication Request Received {Content}", content.ToJson());
            ValidateResponse result;

            try
            {
                HttpContext.Response.Cookies.Append("_authenticated", content.Username);
                result = await _validateService.SendValidationRequest(content, content.CorrelationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(EventCodes.AuthCheck.Errors.VALIDATECREDENTIALS_NEWREQUEST_FAILURE, "Unhandled exception", ex);

                if (ex.InnerException is UnauthorizedAccessException)
                    return BuildConflict("Unauthorised access to the Authentication API", ex.Message, content.CorrelationId.ToString());
                else
                    return BuildConflict("Unexpected Exception calling the Authentication API", ex.Message, content.CorrelationId.ToString());
            }

            if (result.IsValid || (!string.IsNullOrEmpty(result.Code) && result.Code.ToUpper().Equals("VC-800")))
            {
                _logger.LogInformation(EventCodes.AuthCheck.Trace.VALIDATECREDENTIALS_NEWREQUEST_SUCCESS, "Validate Credentials Success");

                return Ok(result);
            }
            else
            {
                _logger.LogInformation(EventCodes.AuthCheck.Information.VALIDATECREDENTIALS_NEWREQUEST_FAILURE, "Validate Credentials Failure {result}", result);

                return BuildConflict(result.UserMessage, result.Errors.ToJson(), content.CorrelationId);

            }
        }

        private ConflictObjectResult BuildConflict(string userMessage, string developerMessage, string requestId)
        {
            return Conflict(new DemoExceptionResponse()
            {
                UserMessage = userMessage,
                DeveloperMessage = developerMessage,
                RequestId = requestId,
                Status = StatusCodes.Status409Conflict
            });
        }

    }
}
