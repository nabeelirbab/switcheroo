using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Items
{
    public interface IItemRepository
    {
        Task<Item> CreateItemAsync(Item item);

        Task<Item> UpdateItemAsync(Item item);
        Task<string> UpdateItemLocation(Guid itemId, decimal? latitude, decimal? longitude);

        Task<string> UpdateAllItemsLocation(Guid userId, decimal? latitude, decimal? longitude);

        Task<bool> ArchiveItemAsync(Guid itemId, Guid updatedByUserId);
        Task<bool> DeleteItemAsync(Guid itemId);

        Task<bool> DismissItemAsync(DismissedItem dismissedItem);

        Task<IEnumerable<Item>> GetItemsByUserId(Guid userId);

        Task<Item> GetItemByItemId(Guid itemId);

        Task<List<Item>> GetItemByOfferId(Guid offerId, Guid? userId);

        Task<List<Item>> GetTargetItem(Guid offerId, Guid? userId);

        Task<Paginated<Item>> GetItems(Guid userId, Guid itemId, decimal? amount, string[]? categories, int limit, string? cursor, decimal? latitude, decimal? longitude, decimal? distance, bool? inMiles);
        Task<Paginated<Item>> GetAllItems(Guid userId, int limit, string? cursor);
    }
}
