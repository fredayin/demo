using Common.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Common
{
    public interface IOldUserService
    {
        Task<OldUserResponse> GetUser(string userName);
        Task<OldUserResponse> GetUserByID(string uuid);
    }

    public class OldUserService : IOldUserService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OldUserService> _logger;
        private readonly IBearerTokenManager _bearerTokenManager;
        private readonly Dictionary<string, string> _headers;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public OldUserService(IHttpClientFactory clientFactory,
            ILogger<OldUserService> logger,
            IConfiguration configuration,
            IBearerTokenManager bearerTokenManager,
            JsonSerializerOptions jsonSerializerOptions)
        {
            _clientFactory = clientFactory;
            _logger = logger;
            _bearerTokenManager = bearerTokenManager;
            _configuration = configuration;
            _jsonSerializerOptions = jsonSerializerOptions;

            _headers = new Dictionary<string, string>();
            var clientId = _configuration["ValidateServiceSettings:ClientId"];
            var clientSecret = _configuration["ValidateServiceSettings:ClientSecret"];
            _headers.Add("Authorization", $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"))}");
        }

        public async Task<OldUserResponse> GetUser(string userName)
        {
            _logger.LogInformation(EventCodes.UserDetails.Trace.USERDETAILS_NEWGETREQUEST, "User details Request Received {Content}", userName);

            var bearerToken = await _bearerTokenManager.GetBearer(
                  Constants.ValidationServiceName,
                  HttpMethod.Get,
                  "/oauth/accesstoken?grant_type=client_credentials",
                  _headers,
                  null
              );

            var client = _clientFactory.CreateClient(Constants.UserDetailsServiceName);
            var url = $"{client.BaseAddress}/v1/user-service/user-details?username={userName}";

            client.DefaultRequestHeaders.Add("TackingId", Guid.NewGuid().ToString());
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken.AccessToken);

            var postResponse = await client.GetAsync(url);

            if (postResponse.IsSuccessStatusCode)
            {
                var responseResult = await postResponse.Content.ReadFromJsonAsync<UserInfoResponse>(_jsonSerializerOptions);

                _logger.LogInformation(EventCodes.UserDetails.Trace.USERDETAILS_NEWGETREQUEST_SUCCESS, "Successfully obtained user details");

                return new OldUserResponse() { UserInfoResponse = responseResult, Success = true };
            }
            else if (postResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation(EventCodes.UserDetails.Trace.USERDETAILS_NEWGETREQUEST_NOTFOUND, "User details not found");

                return new OldUserResponse() { UserInfoResponse = null, Success = true, Reason = System.Net.HttpStatusCode.NotFound.ToString() };
            }
            else
            {
                _logger.LogInformation(EventCodes.UserDetails.Trace.USERDETAILS_NEWGETREQUEST_FAILURE, "Failure to call user details");

                return new OldUserResponse() { UserInfoResponse = null, Success = false, Reason = postResponse.ReasonPhrase };

            }
        }

        public async Task<OldUserResponse> GetUserByID(string uuid)
        {
            _logger.LogInformation(EventCodes.UserDetails.Trace.USERDETAILS_NEWGETREQUEST, "User details Request Received {Content}", uuid);

            var bearerToken = await _bearerTokenManager.GetBearer(
                  Constants.ValidationServiceName,
                  HttpMethod.Get,
                  "/oauth/accesstoken?grant_type=client_credentials",
                  _headers,
                  null
              );

            var client = _clientFactory.CreateClient(Constants.UserDetailsServiceName);
            var url = $"{client.BaseAddress}/v1/user-service/{uuid}/user-details";

            client.DefaultRequestHeaders.Add("TackingId", Guid.NewGuid().ToString());
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken.AccessToken);

            var postResponse = await client.GetAsync(url);

            if (postResponse.IsSuccessStatusCode)
            {
                var responseResult = await postResponse.Content.ReadFromJsonAsync<UserInfoResponse>(_jsonSerializerOptions);

                _logger.LogInformation(EventCodes.UserDetails.Trace.USERDETAILS_NEWGETREQUEST_SUCCESS, "Successfully obtained user details");

                return new OldUserResponse() { UserInfoResponse = responseResult, Success = true };
            }
            else if (postResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation(EventCodes.UserDetails.Trace.USERDETAILS_NEWGETREQUEST_NOTFOUND, "User details not found");

                return new OldUserResponse() { UserInfoResponse = null, Success = true, Reason = System.Net.HttpStatusCode.NotFound.ToString() };
            }
            else
            {
                _logger.LogInformation(EventCodes.UserDetails.Trace.USERDETAILS_NEWGETREQUEST_FAILURE, "Failure to call user details");

                return new OldUserResponse() { UserInfoResponse = null, Success = false, Reason = postResponse.ReasonPhrase };

            }
        }
    }



        public class OldUserResponse
    {
        /// <summary>
        /// User info of successful call to DNP
        /// </summary>
        public UserInfoResponse UserInfoResponse { get; set; }

        /// <summary>
        /// Success or failure of the call.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Reason of failure
        /// </summary>
        public string Reason { get; set; }
    }
}
