using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models;
using Microsoft.Graph;
using System.Net;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Caching.Distributed;
using Common.Entities;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    [ApiVersion("1.0")]
    [Route("/v{version:apiVersion}/[controller]")]
    [Route("[controller]")]
    [AllowAnonymous]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly IGraphApiService _graphService;
        private readonly ILogger<UserController> _logger;
        private readonly AzureADIdentityProvider _identityProvider;

        public UserController(IGraphApiService graphService,
            ILogger<UserController> logger,
            AzureADIdentityProvider identityProvider,
            TelemetryClient telemetryClient)
        {
            _graphService = graphService;
            _logger = logger;
            _identityProvider = identityProvider;
        }

        [MapToApiVersion("1.0")]
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(UserResponse), 200)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserByUserNameOrUUID(string uuid, string username)
        {
            _logger.LogInformation(EventCodes.Users.Trace.USERS_GETUSERREQUEST, "New view users request");

            if (!string.IsNullOrEmpty(uuid) && !string.IsNullOrEmpty(username))
            {
                var ex = new ArgumentException("Too many parameters passed");
                _logger.LogError(EventCodes.Users.Errors.USERS_GETUSER_ARGUMENTEXCEPTION, ex, "Value of UUID:{uuid}, value of username:{username}", uuid, username);

                throw ex;
            }

            IEnumerable<User> graphSearchResult = null;
            User graphUUIDResult = null;
            UserResponse userResult = null;

            try
            {
                var AppClientId = Guid.Parse(Common.Constants.AppClientID);

                if (!string.IsNullOrEmpty(uuid))
                {
                    graphUUIDResult = await _graphService.GetUserByUUID(uuid, AppClientId);

                    if (graphUUIDResult != null)
                    {
                        userResult = graphUUIDResult.AsUserResponse(HttpContext, AppClientId);
                    }
                    else //Return 404 because graphResult is not null and there is no record count
                    {
                        _logger.LogInformation(EventCodes.Users.Trace.USERS_GETUSERREQUEST_NOTFOUND, "User infomation not found");
                        return NotFound();
                    }
                }

                if (!string.IsNullOrEmpty(username))
                {
                    var tenantDomain = _identityProvider.AzureAD.Domain;

                    if (string.IsNullOrEmpty(tenantDomain))
                    {
                        throw new Exception($"No tenant domain defined, can't continue");
                    }

                    graphSearchResult = await _graphService.GetUserSearch(username, AppClientId, tenantDomain);

                    if (graphSearchResult != null)
                    {
                        if (graphSearchResult.Count() == 1)
                        {
                            userResult = graphSearchResult.FirstOrDefault().AsUserResponse(HttpContext, AppClientId);
                        }
                        else if (graphSearchResult.Count() == 0) //Return 404 because graphResult is not null and there is no record count
                        {
                            _logger.LogInformation(EventCodes.Users.Trace.USERS_GETUSERREQUEST_NOTFOUND, "User infomation not found");
                            return NotFound();
                        }
                        else if (graphSearchResult.Count() > 1) //Return 400 because graphResult is not null and there is multiple records
                        {
                            var msg = "Multiple users returned from graph api based on the input value UUID:{uuid},username:{username}";
                            _logger.LogInformation(EventCodes.Users.Information.USERS_GETUSERREQUEST_MULTIPLERECORDS, msg, uuid, User);
                            return BadRequest(msg);
                        }
                    }

                }
            }
            catch (ServiceException se)
            {
                if (se.ResponseStatusCode == (int)HttpStatusCode.NotFound)
                {
                    _logger.LogTrace(EventCodes.Users.Trace.USERS_GETUSERREQUEST_NOTFOUND, "User not found");

                    return NotFound();
                }

                if (se.ResponseStatusCode == (int)HttpStatusCode.BadRequest)
                {
                    //Could be filter injection attack, caught because of escaped characters 
                    _logger.LogInformation(EventCodes.Users.Information.USERS_INVALID_GRAPHAPI_FILTER, "User not found, due to bad message request: {responseBody}", se.RawResponseBody);

                    return BadRequest();
                }

                _logger.LogError(EventCodes.Users.Errors.USERS_GETUSER_FAILURE, "Unhandled service exception occured", se);

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(EventCodes.Users.Errors.USERS_GETUSER_FAILURE, "exception occured", ex);

                throw;
            }

            if (userResult != null)
            {
                _logger.LogInformation(EventCodes.Users.Trace.USERS_GETUSERREQUEST_SUCCESS, "Get User Success");

                return new OkObjectResult(userResult);
            }
            else
            {
                _logger.LogInformation(EventCodes.Users.Trace.USERS_GETUSERREQUEST_FAILURE, "Result is null");

                return BadRequest();
            }

        }
    }
}
