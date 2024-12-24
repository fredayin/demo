using Common.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Common.Entities.UserResponse;

namespace Common
{
    public static class UserExtensions
    {
        public static UserResponse AsUserResponse(this User user, HttpContext context, Guid AppClientId)
        {
            var response = new UserResponse();

            var additionData = user.AdditionalData;
            var formatedAppClient = AppClientId.RemoveHyphens();

            var defaultValidFor = new UserResponse.ValidForDates()
            {
                StartDateTime = user.CreatedDateTime,
                EndDateTime = user.DeletedDateTime,
            };

            response.ID = user.Id;
            response.Href = $"{context.Request.Scheme}://{context.Request.Host.Value}{context.Request.Path}";
            response.ValidFor = defaultValidFor;

            response.Credential = new Credentials();
            Credentials credentialItem = SetCredentialDetails(user, response, formatedAppClient);
            credentialItem.FirstName = additionData.GetValue<string>(formatedAppClient, UserExtensionKeys.FirstName);
            credentialItem.LastName = additionData.GetValue<string>(formatedAppClient, UserExtensionKeys.LastName);
            credentialItem.ContactNumber = additionData.GetValue<string>(formatedAppClient, UserExtensionKeys.ContactNumber);

            return response;

        }

        public static T GetValue<T>(this IDictionary<string, object> collection, string AppClient, string keyName)
        {
            var t = typeof(T);
            var fullKeyName = $"extension_{AppClient}_{keyName}";

            if (collection.ContainsKey(fullKeyName))
            {
                var element = collection[fullKeyName];

                if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                {
                    t = Nullable.GetUnderlyingType(t);
                }

                return (T)Convert.ChangeType(element.ToString(), t);
            }

            return default(T);
        }

        private static UserResponse.Credentials SetCredentialDetails(User user, UserResponse response, string formatedAppClient)
        {
            var credentialItem = response.Credential;

            credentialItem.SetCredentialId(user);
            credentialItem.State = GetCredentialState(user, formatedAppClient);

            credentialItem.ValidFor = new UserResponse.ValidForDates()
            {
                StartDateTime = user.LastPasswordChangeDateTime == null ? user.CreatedDateTime : user.LastPasswordChangeDateTime,
                EndDateTime = user.DeletedDateTime,
            };

            return credentialItem;

        }

        private static string GetCredentialState(User user, string formatedAppClient)
        {

            if (user.AccountEnabled == false)
            {
                return "disabled";
            }

            return "active";
        }

        private static void SetCredentialId(this UserResponse.Credentials credentialItem, User user)
        {
            var allowedSignInTypesByPrecedence = new string[] { "emailAddress", "userName" };
            var identityList = user.Identities
                .Where(identity => allowedSignInTypesByPrecedence.Contains(identity.SignInType))
                .OrderBy(identity => Array.IndexOf(allowedSignInTypesByPrecedence, identity.SignInType))
                .FirstOrDefault();

            //Will be the first one in the list of precedence.
            credentialItem.Id = identityList?.IssuerAssignedId;
        }
    }
}
