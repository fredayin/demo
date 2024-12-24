using Microsoft.ApplicationInsights;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Common.ConfigurationSections;

namespace Common
{
    public class GraphApiService : IGraphApiService
    {
        private readonly GraphServiceClient _graphServiceClient;
        public GraphApiService(GraphServiceClient graphServiceClient)
        {
            _graphServiceClient = graphServiceClient;
        }

        public async Task<User> GetUserByUUID(string uuid, Guid AppClientId, UserType userType = UserType.Member)
        {
            var formatedAppClient = AppClientId.RemoveHyphens();

            var userResponse = await _graphServiceClient.Users.GetAsync((requestConfiguration) =>
            {
                requestConfiguration.QueryParameters.Expand = new[] { "Extensions" };
                requestConfiguration.QueryParameters.Select = new string[] { "createdDateTime","id", "accountEnabled", "otherMails", "displayName",
                                                                            "refreshTokensValidFromDateTime",
                                                                            "signInSessionsValidFromDateTime", "identities","userType",
                                                                            $"extension_{formatedAppClient}_status",
                                                                            $"extension_{formatedAppClient}_pwdLastUpdatedDtime",
                                                                            $"extension_{formatedAppClient}_contactNumber",
                                                                            $"extension_{formatedAppClient}_firstName",
                                                                            $"extension_{formatedAppClient}_lastName",
                                                                            $"extension_{formatedAppClient}_uuid"
                                                                            };
                requestConfiguration.QueryParameters.Filter = $"userType eq '{userType}' and id eq '{uuid}'";

            });

            return userResponse.Value.FirstOrDefault()!;
        }

        public async Task<IEnumerable<User>> GetUserSearch(string username, Guid AppClientId, string issuer, UserType userType = UserType.Member)
        {
            var formatedAppClient = AppClientId.RemoveHyphens();

            var userResponse = await _graphServiceClient.Users.GetAsync((requestConfiguration) =>
            {
                requestConfiguration.QueryParameters.Expand = new[] { "Extensions" };
                requestConfiguration.QueryParameters.Select = new string[] { "createdDateTime","id", "accountEnabled", "otherMails", "displayName",
                                                                            "refreshTokensValidFromDateTime",
                                                                            "signInSessionsValidFromDateTime", "identities","userType",
                                                                            $"extension_{formatedAppClient}_status",
                                                                            $"extension_{formatedAppClient}_pwdLastUpdatedDtime",
                                                                            $"extension_{formatedAppClient}_contactNumber",
                                                                            $"extension_{formatedAppClient}_firstName",
                                                                            $"extension_{formatedAppClient}_lastName",
                                                                            $"extension_{formatedAppClient}_uuid"
                                                                            };
                requestConfiguration.QueryParameters.Filter = $"identities/any(c:c/issuerAssignedId eq '{username}' and c/issuer eq '{issuer}')";

            });

            var results = userResponse.Value.Where(_ => _.UserType == userType.ToString());

            return results;
        }

        public virtual async Task<DateTimeOffset?> GetUserLastLogInTime(string username)
        {
            var signIns = await _graphServiceClient.AuditLogs.SignIns.GetAsync((requestConfiguration) =>
            {
                requestConfiguration.QueryParameters.Top = 1;
                requestConfiguration.QueryParameters.Orderby = new string[] { "createdDateTime desc" };
                requestConfiguration.QueryParameters.Filter = $"userPrincipalName eq '{username}'";
            });

            var lastSignIn = signIns.Value.FirstOrDefault();
            if (lastSignIn is not null)
                return lastSignIn.CreatedDateTime;
            else
                return null;
        }

        public async Task<User> UpdateUser(string objectId, User user)
        {
            return await _graphServiceClient.Users[objectId].PatchAsync(user);
        }
    }
}
