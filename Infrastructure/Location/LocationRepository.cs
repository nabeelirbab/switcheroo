using Domain;
using Domain.Locations;
using Infrastructure.Database;
using Infrastructure.Database.Schema;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Locations
{
    public class LocationRepository : ILocationRepository
    {
        private readonly SwitcherooContext db;

        public LocationRepository(SwitcherooContext db)
        {
            this.db = db;
        }

        public async Task<Domain.Locations.Location> AddLocationAsync(Domain.Locations.Location newlocation)
        {
            
            var now = DateTime.UtcNow;
            if (!newlocation.CreatedByUserId.HasValue)

                throw new InfrastructureException("No createdByUserId provided");

            var dbLocation = new Database.Schema.Location(
                newlocation.IsActive,
                newlocation.Latitude,
                newlocation.Longitude,
                newlocation.ItemsId
            )
            {
                CreatedByUserId = newlocation.CreatedByUserId.Value,
                UpdatedByUserId = newlocation.CreatedByUserId.Value,
                CreatedAt = now,
                UpdatedAt = now,
                ItemsId = newlocation.ItemsId,
            };

            await db.Location.AddAsync(dbLocation);
            await db.SaveChangesAsync();
           

            return await GetLocationByid(dbLocation.Id);
        }

        public async Task DeleteLocationAsync(Guid Id)
        {
            var location = await db.Location.FindAsync(Id);

            if (location == null)
            {
                throw new InvalidOperationException($"Location with ID {Id} not found");
            }

            db.Location.Remove(location);
            await db.SaveChangesAsync();
        }

        public async Task<Domain.Locations.Location> GetLocationByid(Guid id)
        {
            var location = await db.Location
                .Where(location => location.Id == id)
                .Select(Database.Schema.Location.ToDomain)
                .SingleOrDefaultAsync();


            if (location == null) throw new InfrastructureException($"Unable to locate item {id}");

            return location;
        }

        public Task<Paginated<Domain.Locations.Location>> GetLocations(Guid userId, decimal? latitude, decimal? longitude, Guid itemId, bool? isActive)
        {
            throw new NotImplementedException();
        }
    }
}
