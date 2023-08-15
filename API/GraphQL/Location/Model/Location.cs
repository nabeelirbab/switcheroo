using Domain.Users;
using HotChocolate;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace API.GraphQL.Location.Model
{
    public class Location
    {
        private Location(Guid? id, Guid createdByUserId, Guid updatedByUserId, decimal? latitude, decimal? longitude, Guid itemId, bool isActive)
        {
            Id = id;
            CreatedByUserId = createdByUserId;
            UpdatedByUserId = updatedByUserId;
            Latitude = latitude;
            Longitude = longitude;
            ItemsId = itemId;
            IsActive = isActive;
        }

        public Guid? Id { get; set; }

        public decimal? Latitude { get; private set; }
        public decimal? Longitude { get; private set; }
        public bool? IsActive { get; private set; }
        public Guid? ItemsId { get; private set; }

        public Guid CreatedByUserId { get; private set; }

        public async Task<Users.Models.User> GetCreatedByUser([Service] IUserRepository userRepository)
        {
            return await GetUserByUserId(userRepository, CreatedByUserId);
        }

        public Guid UpdatedByUserId { get; private set; }

        public async Task<Users.Models.User> GetUpdatedByUser([Service] IUserRepository userRepository)
        {
            return await GetUserByUserId(userRepository, UpdatedByUserId);
        }

        private async Task<Users.Models.User> GetUserByUserId(IUserRepository userRepository, Guid userId)
        {
            var domUser = await userRepository.GetById(userId);

            if (domUser == null) throw new ApiException($"Invalid UserId {userId}");

            return Users.Models.User.FromDomain(domUser);
        }

        // Mappers
        public static Location FromDomain(Domain.Locations.Location domLocation)
        {
            if (!domLocation.Id.HasValue) throw new ApiException("Mapping error. Id missing");
            if (!domLocation.CreatedByUserId.HasValue) throw new ApiException("Mapping error. CreatedByUserId missing");
            if (!domLocation.UpdatedByUserId.HasValue) throw new ApiException("Mapping error. UpdatedByUserId missing");

            return new Location(
                domLocation.Id.Value,
                domLocation.CreatedByUserId.Value,
                domLocation.UpdatedByUserId.Value,
                domLocation.Latitude,
                domLocation.Longitude,
                domLocation.ItemsId,
                domLocation.IsActive
                );
        }
    }
}
