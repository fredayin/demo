using Common.Entities;
using Common;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Graph;
using System.Net;
using Azure;
using Microsoft.Extensions.Logging;

namespace API.Controllers
{
    [ApiVersion("1.0")]
    [Route("/v{version:apiVersion}/[controller]")]
    [Route("[controller]")]
    [AllowAnonymous]
    [ApiController]
    public class UserDetailsController : ControllerBase
    {
        private readonly ILogger<UserDetailsController> _logger;
        private readonly IOldUserService _oldUserService;

        public UserDetailsController(IOldUserService oldUserService,
            ILogger<UserDetailsController> logger) 
        {
            _oldUserService = oldUserService;
            _logger = logger;
        }

        [MapToApiVersion("1.0")]
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(UserResponse), 200)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserDetails(string uuid, string username)
        {
            _logger.LogInformation(EventCodes.Users.Trace.USERS_GETUSERREQUEST, "New view users request");

            if (!string.IsNullOrEmpty(uuid) && !string.IsNullOrEmpty(username))
            {
                var ex = new ArgumentException("Too many parameters passed");
                _logger.LogError(EventCodes.UserDetails.Errors.USERDETAILS_GETUSERDETAILS_ARGUMENTEXCEPTION, ex, "Value of UUID:{uuid}, value of username:{username}", uuid, username);

                throw ex;
            }

            
            OldUserResponse response = null;

            if (!string.IsNullOrEmpty(uuid))
            {
                response = await _oldUserService.GetUserByID(uuid);
            }
            else if (!string.IsNullOrEmpty(username))
            {
                response = await _oldUserService.GetUser(username);
            } else
            {
                var ex = new ArgumentException("Missing parameters");
                _logger.LogError(EventCodes.UserDetails.Errors.USERDETAILS_GETUSERDETAILS_MISSINGARGUMENTEXCEPTION, ex, "UUID or Username must be provided");

                throw ex;
            }

            if (response.Success && string.IsNullOrEmpty(response.Reason))
            {
                _logger.LogInformation(EventCodes.UserDetails.Trace.USERDETAILS_NEWGETREQUEST_SUCCESS, "Successfully obtained user details");

                return new OkObjectResult(response.UserInfoResponse);
            }
            else
            {
                if (response.Reason == System.Net.HttpStatusCode.NotFound.ToString())
                {
                    _logger.LogTrace(EventCodes.UserDetails.Trace.USERDETAILS_NEWGETREQUEST_NOTFOUND, "User details not found");

                    return new OkObjectResult(new UserInfoResponse());
                }
                else
                {
                    _logger.LogTrace(EventCodes.UserDetails.Trace.USERDETAILS_NEWGETREQUEST_FAILURE, "Failure to call user details");

                    return Conflict(new DemoExceptionResponse()
                    {
                        UserMessage = "User Detail API Failed",
                        DeveloperMessage = response.Reason,
                        Status = StatusCodes.Status409Conflict
                    });
                }
            }

        }
    }
}
