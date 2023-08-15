using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Items
{
    public interface IItemRepository
    {
         Task<Item> CreateItemAsync(Item item);

         Task<Item> UpdateItemAsync(Item item);
         Task<string> UpdateItemLocation(Guid userId, decimal? latitude, decimal? longitude);

         Task<bool> ArchiveItemAsync(Guid itemId, Guid updatedByUserId);

         Task<bool> DismissItemAsync(DismissedItem dismissedItem);

         Task<IEnumerable<Item>> GetItemsByUserId(Guid userId);

         Task<Item> GetItemByItemId(Guid itemId);

        Task<Paginated<Item>> GetItems(Guid userId, decimal? amount, string[]? categories, int limit, string? cursor, decimal? latitude, decimal? longitude, decimal? distance, bool? inMiles);
    }
}
