using System;
using System.Threading.Tasks;

namespace Domain.Locations
{
    public interface ILocationRepository
    {
        Task<Location> AddLocationAsync(Location newlocation);

        Task<Location> GetLocationByid(Guid id);

        Task DeleteLocationAsync(Guid Id);

        Task<Paginated<Location>> GetLocations(Guid userId, decimal? latitude, decimal? longitude, Guid itemId, bool? isActive);
    }
}
