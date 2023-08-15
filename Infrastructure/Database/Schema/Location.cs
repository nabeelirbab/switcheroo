using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Database.Schema
{
    public class Location : Audit
    {

        public Location(bool isActive, decimal? latitude, decimal? longitude, Guid itemsId)
        {
            IsActive = isActive;
            Latitude = latitude;
            Longitude = longitude;
            ItemsId = itemsId;
        }

        [Required]
        public Guid Id { get; set; }

        [Required]
        public decimal? Latitude { get; private set; }
        [Required]
        public decimal? Longitude { get; private set; }
        [Required]
        public Guid ItemsId { get; set; }
        [Required]
        public bool IsActive { get; set; }
        
        public Item? Item { get; set; }


        public void FromDomain(Domain.Locations.Location location)
        {
            IsActive = location.IsActive;
            Latitude = location.Latitude;
            Longitude = location.Longitude;
        }

        public static Expression<Func<Location, Domain.Locations.Location>> ToDomain =>
            location => new Domain.Locations.Location(
                location.Id,
                location.CreatedByUserId,
                location.UpdatedByUserId,
                location.Latitude != null ? location.Latitude : null,
                location.Longitude != null ? location.Longitude : null,
                location.ItemsId ,
                location.IsActive
                );
    }
}
