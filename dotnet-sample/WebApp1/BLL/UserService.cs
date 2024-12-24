using Azure.Core;
using Common;
using Common.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using System.Net;
using WebApp1.Model;
using static Common.ConfigurationSections;

namespace WebApp1.BLL
{
    public interface IUserService
    {
        Task<UserSearchModel> GetUserModel(string email);
        Task<IActionResult> Patch(Guid objectId, UpdateUserRequest updateUserRequest, HttpContext httpContext);
    }

    public class UserService : ControllerBase, IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IGraphApiService _graphApiService = null;
        private readonly AzureADIdentityProvider _identityProvider;
        //private readonly ILogClientService _logClient;


        public UserService(ILogger<UserService> logger, HttpClient httpClient, IGraphApiService graphService, AzureADIdentityProvider identityProvider)
        {
            _logger = logger;
            _graphApiService = graphService;
            _identityProvider = identityProvider;
            //_logClient = logClient;
        }

        /// <summary>
        /// Gets the user model after by querying the GraphApi.
        /// </summary>
        /// <param name="username">The email.</param>
        /// <returns></returns>
        public async Task<UserSearchModel> GetUserModel(string username)
        {
            var model = new UserSearchModel() { UserFound = false };

            var authScheme = _identityProvider.AuthSchemeName;
            var tenantDomain = _identityProvider.AzureAD.Domain;
            var AppClientId = new System.Guid();
            try
            {
                AppClientId = Guid.Parse(Common.Constants.AppClientID);
            }
            catch (FormatException ex)
            {
                _logger.LogError(EventCodes.Users.Errors.USERS_APPCLIENTID_FORMATEXCEPTION, "Invalid guid format for appsetting AppClientId, Graph API extensions require guids without hypens", ex);
                throw;
            }

            User user;
            try
            {
                var users = await _graphApiService.GetUserSearch(username, AppClientId, tenantDomain);
                user = users.First();
            }
            catch (Exception ex)
            {
                _logger.LogTrace(EventCodes.WebApp1.UI.Trace.UI_WEBAPP1_USER_TOOL_USER_NOT_FOUND, "User Not Found.");
                return model;
            }

            var lastLogIn = await _graphApiService.GetUserLastLogInTime(username);

            model.Created = user.CreatedDateTime?.ToString("dd/MM/yyyy hh:mm:ss tt zzz");
            model.LastLogIn = (lastLogIn != null) ? lastLogIn?.ToString("dd/MM/yyyy hh:mm:ss tt zzz") : "No Last Sign In";
            model.AccountEnabled = user.AccountEnabled.HasValue ? user.AccountEnabled.Value.ToString() : "False";
            model.FirstName = user.AdditionalData.GetValue<string>(AppClientId.RemoveHyphens(), UserExtensionKeys.FirstName); ;
            model.UserFound = true;
            model.ObjectId = user.Id;

            _logger.LogTrace(EventCodes.WebApp1.UI.Trace.UI_WEBAPP1_USER_TOOL_USER_FOUND, "User Data Found.");

            return model;
        }

        public async Task<IActionResult> Patch(Guid objectId, UpdateUserRequest updateUserRequest, HttpContext httpContext)
        {
            _logger.LogTrace(EventCodes.Users.Trace.USERS_UPDATEUSERPATCHREQUEST, "Updating user Account with objectId: {objectId}.", objectId);

            var correlationId = Guid.Parse(updateUserRequest.CorrelationId);

            var AppClientId = Guid.Empty;
            try
            {
                AppClientId = Guid.Parse(Common.Constants.AppClientID);
            }
            catch (FormatException ex)
            {
                _logger.LogError(EventCodes.Users.Errors.USERS_APPCLIENTID_FORMATEXCEPTION, "Invalid guid format for appsetting AppClientId, Graph API extensions require guids without hypens", ex);
                throw;
            }

            User user;
            try
            {
                var updateUserObject = new User();
                updateUserObject.AccountEnabled = updateUserRequest.AccountEnabled;
                user = await _graphApiService.UpdateUser(objectId.ToString(), updateUserObject);
            }
            catch (ODataError ex)
            {
                if (ex.ResponseStatusCode is (int)HttpStatusCode.NotFound)
                {
                    _logger.LogTrace(EventCodes.Users.Trace.USERS_GETUSERREQUEST_NOTFOUND, $"User not found for objectId: {objectId}");
                    return NotFound($"User does not exist in B2C.");
                }
                if (ex.ResponseStatusCode == (int)HttpStatusCode.BadRequest && ex.Error is not null && ex.Error.Details.Any(_ => _.Code == "ConflictingObjects"))
                {
                    _logger.LogTrace(EventCodes.Users.Trace.USERS_UPDATEUSERPATCHREQUEST_USERNAMEALREADYEXISTS, $"User tried to update username to one that already exists on another account: {objectId}");
                    return Conflict("User name already exists on another account");
                }
                _logger.LogError(EventCodes.Users.Errors.USERS_UPDATEUSERPATCHFAILED, "Failed to update the user account correctly.", ex);
                return Conflict("Failed to update the user account correctly.");
            }
            catch (Exception ex)
            {
                _logger.LogError(EventCodes.Users.Errors.USERS_UPDATEUSERPATCHFAILED, "Failed to update the user account correctly.", ex);
                return Conflict("Failed to update the user account correctly.");
            }

            var updatedUser = user.AsUserResponse(httpContext, AppClientId);

            return new OkObjectResult(updatedUser);
        }
    }
}
