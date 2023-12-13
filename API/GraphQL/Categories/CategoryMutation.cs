using Domain.Categories;
using Domain.Users;
using HotChocolate;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.GraphQL
{
    public partial class Mutation
    {
        public async Task<bool> CreateCategories(
           [Service] IHttpContextAccessor httpContextAccessor,
           [Service] IUserAuthenticationService userAuthenticationService,
           [Service] ICategoryRepository categoryRepository,
           List<string> name
       )
        {
            var userCp = httpContextAccessor?.HttpContext?.User;

            if (userCp == null) throw new ApiException("Not authenticated");
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(userCp);
            if (!user.Id.HasValue) throw new ApiException("Database failure");

            await categoryRepository.CreateCategories(name);

            return true;
        }
    }
}
