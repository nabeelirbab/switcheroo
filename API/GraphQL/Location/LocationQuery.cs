using Domain.Locations;
using Domain.Users;
using HotChocolate;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;
using HotChocolate.AspNetCore.Authorization;
using System.Linq;
using Domain.Items;
using API.GraphQL.CommonServices;

namespace API.GraphQL
{
    public partial class Query
    {

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<Paginated<Location.Model.Location>> GetLocation(
            [Service] UserContextService userContextService,
            [Service] ILocationRepository loactionRepository,
            decimal? latitude,
            decimal? longitude,
            Guid itemId,
            bool? isActive = false
        )
        {
            var requestUserId = userContextService.GetCurrentUserId();
            var paginatedlocations = await loactionRepository.GetLocations(requestUserId, latitude, longitude, itemId, isActive);
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
