using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Common
{
    public interface IGraphApiService
    {
        Task<IEnumerable<User>> GetUserSearch(string username, Guid AppClientId, string issuer, UserType userType = UserType.Member);

        Task<User> GetUserByUUID(string uuid, Guid AppClientId, UserType userType = UserType.Member);

        Task<DateTimeOffset?> GetUserLastLogInTime(string username);

        Task<User> UpdateUser(string objectId, User user);
    }
}
