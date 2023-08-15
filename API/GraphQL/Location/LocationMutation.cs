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
        public async Task<Location.Model.Location> AddLocation(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] ILocationRepository locationRepository,
            LocationInput locationInput
        )
        {
            var userCp = httpContextAccessor?.HttpContext?.User;

            if (userCp == null) throw new ApiException("Not authenticated");
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(userCp);
            if (!user.Id.HasValue) throw new ApiException("Database failure");

            var newlocation = await locationRepository.AddLocationAsync(Domain.Locations.Location.AddNewLocation(
                user.Id.Value,
                locationInput.Latitude,
                locationInput.Longitude,
                locationInput.ItemId,
                locationInput.IsActive
                
                ));  

            return Location.Model.Location.FromDomain(newlocation);
        }

        public async Task<Location.Model.Location> DeleteLocation(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] ILocationRepository locationRepository,
            Guid id
        )
        {
            var userCp = httpContextAccessor?.HttpContext?.User;

            if (userCp == null) throw new ApiException("Not authenticated");
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(userCp);
            if (!user.Id.HasValue) throw new ApiException("Database failure");

            try
            {
                // Attempt to delete the location
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
