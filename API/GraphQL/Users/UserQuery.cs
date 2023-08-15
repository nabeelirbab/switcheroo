using System.Threading.Tasks;
using Domain.Users;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace API.GraphQL
{
    public partial class Query
    {
        [Authorize]
        public async Task<Users.Models.User> GetMe(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService
        )
        {
            var claimsPrinciple = httpContextAccessor.HttpContext.User;
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(claimsPrinciple);

            if (user == null) throw new ApiException("Not logged in");
            if (!user.Id.HasValue) throw new ApiException("Fatal. Db entity doesn't have a primary key...or you fucked up");

            return Users.Models.User.FromDomain(user);
        }
    }
}
