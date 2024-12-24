using Common.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public interface IBearerTokenManager
    {
        Task<BearerResponse> GetBearer(string httpClientName, HttpMethod method, string path, Dictionary<string, string> headers, Dictionary<string, string> body);
    }

    public class BearerTokenManager : IBearerTokenManager
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger _logger;

        public BearerTokenManager(IHttpClientFactory clientFactory, IMemoryCache memoryCache, ILogger<BearerTokenManager> logger)
        {
            _clientFactory = clientFactory;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task<BearerResponse> GetBearer(string httpClientName, HttpMethod method, string path, Dictionary<string, string> headers, Dictionary<string, string> body)
        {
            return await GetBearer<BearerResponse>(httpClientName, method, path, headers, body);
        }

        private async Task<BearerResponse> GetBearer<T>(string httpClientName, HttpMethod method, string path, Dictionary<string, string> headers, Dictionary<string, string> body) where T : BearerResponse
        {
            _logger.LogInformation(EventCodes.BearerTokens.Trace.BEARERTOKENS_NEWREQUEST, "New bearer token request");

            if (null == body)
            {
                body = new Dictionary<string, string>();
            }

            T resEntry = (T)await GetBearerToken<T>(method, headers, body, httpClientName, path);

            return resEntry;
        }

        private async Task<BearerResponse> GetBearerToken<T>(HttpMethod method, Dictionary<string, string> headers, Dictionary<string, string> body, string httpClientName, string path) where T : BearerResponse
        {
            var client = _clientFactory.CreateClient(httpClientName);

            var url = $"{client.BaseAddress}{path}";

            if (headers is not null)
            {
                foreach (KeyValuePair<string, string> entry in headers)
                {
                    client.DefaultRequestHeaders.Add(entry.Key, entry.Value);
                }
            }

            var data = new FormUrlEncodedContent(body);

            HttpResponseMessage response;
            switch (method.ToString())
            {
                case "POST":
                    response = await client.PostAsync(url, data);
                    break;
                case "GET":
                    response = await client.GetAsync(url);
                    break;
                default:
                    throw new NotImplementedException();
            }

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(EventCodes.BearerTokens.Trace.BEARERTOKENS_OBTAINBEARERSUCCESS, "Successfully obtained bearer token");

                var bearer = await response.Content.ReadFromJsonAsync<T>();

                return bearer;
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var ex = new UnauthorizedAccessException(response.ReasonPhrase);
                    _logger.LogWarning(EventCodes.BearerTokens.Warn.BEARERTOKENS_UNAUTHORIZEDACCESSEXCEPTION, ex, "User is unauthorized");
                    throw ex;
                }

                var niEx = new NotImplementedException();
                _logger.LogError(EventCodes.BearerTokens.Errors.BEARERTOKENS_NOTIMPLEMENTED, niEx, "Not Implemented Exception");

                throw niEx;
            }
        }
    }
}
