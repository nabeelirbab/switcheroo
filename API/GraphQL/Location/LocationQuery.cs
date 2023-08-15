using Domain.Locations;
using Domain.Users;
using HotChocolate;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;
using HotChocolate.AspNetCore.Authorization;
using System.Linq;
using Domain.Items;

namespace API.GraphQL
{
    public partial class Query
    {

        [Authorize]
        public async Task<Paginated<Location.Model.Location>> GetLocation(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] ILocationRepository loactionRepository,
            decimal? latitude,
            decimal? longitude,
            Guid itemId,
            bool? isActive = false
        )
        {
            var claimsPrinciple = httpContextAccessor.HttpContext.User;
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(claimsPrinciple);

            if (user == null) throw new ApiException("Not logged in");
            if (!user.Id.HasValue) throw new ApiException("Fatal. Db entity doesn't have a primary key...or you fucked up");


            var paginatedlocations = await loactionRepository.GetLocations(user.Id.Value, latitude, longitude, itemId, isActive);

            return new Paginated<Location.Model.Location>(
                paginatedlocations.Data
                    .Select(Location.Model.Location.FromDomain)
                    .ToList(),
                paginatedlocations.Cursor,
                paginatedlocations.TotalCount,
                paginatedlocations.HasNextPage);
        }

    }
}
