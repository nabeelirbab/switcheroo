using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain.Locations
{
    public class Location
    {
        public Location(Guid? id, Guid? createdByUserId, Guid? updatedByUserId, decimal? latitude, decimal? longitude, Guid itemsid, bool isActive)
        {
            Id = id;
            CreatedByUserId = createdByUserId;
            UpdatedByUserId = updatedByUserId;
            Latitude = latitude;
            Longitude = longitude;
            ItemsId = itemsid;
            IsActive = isActive;
        }
        [Required]
        public Guid? Id { get; private set; }

        [Required]
        public decimal? Latitude { get; private set; }
        [Required]
        public decimal? Longitude { get; private set; }

        public Guid? CreatedByUserId { get; private set; }

        public Guid? UpdatedByUserId { get; private set; }

        public Guid ItemsId { get; private set; }

        public bool IsActive { get; private set; }


        public static Location AddNewLocation(
           Guid createdByUserId,
           decimal? latitude,
           decimal? longitude,
           Guid itemsid,
           bool isActive
       )
        {
            return new Location(
                null,
                createdByUserId,
                null,
                latitude,
                longitude,
                itemsid,
                isActive
            );
        }
    }
}
