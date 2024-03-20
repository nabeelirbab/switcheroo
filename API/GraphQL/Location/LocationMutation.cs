using API.GraphQL.CommonServices;
using API.GraphQL.Location.Model;
using Domain.Locations;
using Domain.Users;
using HotChocolate;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace API.GraphQL
{
    public partial class Mutation
    {
        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<Location.Model.Location> AddLocation(
            [Service] UserContextService userContextService,
            [Service] ILocationRepository locationRepository,
            LocationInput locationInput
        )
        {
            var requestUserId = userContextService.GetCurrentUserId();
            var newlocation = await locationRepository.AddLocationAsync(Domain.Locations.Location.AddNewLocation(
                requestUserId,
                locationInput.Latitude,
                locationInput.Longitude,
                locationInput.ItemId,
                locationInput.IsActive

                ));
            return Location.Model.Location.FromDomain(newlocation);
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<Location.Model.Location> DeleteLocation(
            [Service] ILocationRepository locationRepository,
            Guid id
        )
        {
            try
            {
                await locationRepository.DeleteLocationAsync(id);
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while deleting the location: {ex.Message}");
            }
        }
    }
}
