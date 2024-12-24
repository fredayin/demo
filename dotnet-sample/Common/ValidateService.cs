using Common.Entities;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights;
using Common.Framework;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Net.Http.Headers;


namespace Common
{

    public interface UserService
    {
        Task<ValidateResponse> SendValidationRequest(ValidateRequest validateRequest, string trackingId);
    }

    public class ValidateService : UserService
    {
        private readonly IConfiguration _configuration;
        private readonly Dictionary<string, string> _headers;
        private readonly IAESEncryption _encryption;
        private readonly ILogger<ValidateService> _logger;
        private readonly IHttpClientFactory _clientFactory;
        private readonly TelemetryClient _telemetryClient;
        public event EventHandler UnauthorisedBearerToken;
        public event EventHandler<UserEventArgs> InvalidCredentials;
        private IBearerTokenManager _bearerTokenManager;
        public ValidateService(IHttpClientFactory httpClientFactory,
                                            IConfiguration configuration,
                                            IAESEncryption encryption,
                                            ILogger<ValidateService> logger,
                                            TelemetryClient telemetryClient,
                                            IBearerTokenManager bearerTokenManager
        )
        {
            _clientFactory = httpClientFactory;
            _encryption = encryption;
            _configuration = configuration;
            _logger = logger;
            _bearerTokenManager = bearerTokenManager;

            _headers = new Dictionary<string, string>();
            var clientId = _configuration["ValidateServiceSettings:ClientId"];
            var clientSecret = _configuration["ValidateServiceSettings:ClientSecret"];
            _headers.Add("Authorization", $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"))}");

            _telemetryClient = telemetryClient;
            UnauthorisedBearerToken += ValidateService_OnUnauthorised;
            InvalidCredentials += ValidateService_InvalidCredentials;
        }

        private void ValidateService_InvalidCredentials(object? sender, UserEventArgs e)
        {
            var props = new Dictionary<string, string>();
            props.Add("login", e.UserName);
            WriteEntry("ValidateService-InvalidCredentialsException", props);
            
        }

        private void ValidateService_OnUnauthorised(object? sender, EventArgs e)
        {
            WriteEntry("ValidateService-UnauthorizedAccessException");
        }

        private void WriteEntry(string eventName)
        {
            WriteEntry(eventName, new Dictionary<string, string>());
        }

        private void WriteEntry(string eventName, Dictionary<string, string> props)
        {
            if (_telemetryClient is not null)
            {
                props.Add("correlationId", Guid.NewGuid().ToString());

                _telemetryClient.TrackEvent(eventName, props);
            }
        }

        public async Task<ValidateResponse> SendValidationRequest(ValidateRequest validateRequest, string trackingId)
        {
            EncryptPasswordAsync(validateRequest);

            var timer = new Stopwatch();
            timer.Start();

            BearerResponse bearerToken = await _bearerTokenManager.GetBearer(
                  Constants.ValidationServiceName,
                  HttpMethod.Get,
                  "/oauth/accesstoken?grant_type=client_credentials",
                  _headers,
                  null
              );

            timer.Stop();
            TimeSpan timeTaken = timer.Elapsed;
            _logger.LogInformation($"Bearer token time taken: {timeTaken}, bearerToken: ", bearerToken.AccessToken);
            timer.Restart();

            HttpClient httpClient = _clientFactory.CreateClient("ValidationServiceName");
            _logger.LogInformation($"Validate Credentials selected httpclient: {"ValidationServiceName"}, correlationId: {validateRequest.CorrelationId}");

            timer.Stop();
            timeTaken = timer.Elapsed;

            _logger.LogInformation($"HTTP client time taken: {timeTaken}");

            string endpoint = _configuration["ValidateServiceSettings:Endpoint"];
            var url = $"{httpClient.BaseAddress}{endpoint}";

            var payload = new StringContent(validateRequest.ToJson(), null, "application/json");

            httpClient.DefaultRequestHeaders.Add("TackingId", trackingId);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken.AccessToken);


            timer.Restart();

            var postResponse = await httpClient.PostAsync(url, payload);

            timer.Stop();
            timeTaken = timer.Elapsed;

            _logger.LogInformation($"Http client time taken: {timeTaken}");

            if (postResponse.IsSuccessStatusCode)
            {
                var responseResult = await postResponse.Content.ReadFromJsonAsync<ValidateResponse>();

                if (!responseResult.Status.Equals("Success"))
                {
                    var response = new ValidateResponse()
                    {
                        IsValid = false,
                        HTTPStatus = 999,
                        UserMessage = responseResult.UserMessage
                    };

                    OnInvalidCredentials(new UserEventArgs() { UserName = responseResult.UserName, ErrorCode = responseResult.Code });

                    return response;
                }
                return responseResult;
            }
            else if (postResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {

                OnUnauthorisedBearerToken();

                return new ValidateResponse() { IsValid = false, UserMessage = "Unauthorized" };
            }
            else if (postResponse.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                var result = await postResponse.Content.ReadFromJsonAsync<ValidateResponse>();

                //Log the result if auth errors
                if (result.Code == "ERR001")
                {
                    OnInvalidCredentials(new UserEventArgs() { UserName = validateRequest.Username, ErrorCode = result.Code });
                }

                result.IsValid = false;
                return result;
            }
            else
            {
                var result = await postResponse.Content.ReadFromJsonAsync<ValidateResponse>();
                result.IsValid = false;
                return result;
            }
        }

        private void OnUnauthorisedBearerToken()
        {
            if (UnauthorisedBearerToken is not null)
                UnauthorisedBearerToken(this, EventArgs.Empty);
        }

        private void OnInvalidCredentials(UserEventArgs userEventArgs)
        {
            if (InvalidCredentials is not null)
                InvalidCredentials(this, userEventArgs);
        }

        private void EncryptPasswordAsync(ValidateRequest validateRequest)
        {
            var timer = new Stopwatch();
            timer.Start();

            validateRequest.Password = _encryption.EncryptString(Constants.EncryptionKey, validateRequest.Password);

            timer.Stop();
            TimeSpan timeTaken = timer.Elapsed;

            _logger.LogInformation($"Encryption time taken: {timeTaken}");
        }
    }
}
