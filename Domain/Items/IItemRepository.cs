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
        Task<bool> DeleteItemAsync(Guid itemId,Guid deletedByUserId);
        Task<bool> DeleteItemPermanentlyAsync(Guid itemId);

        Task<bool> DismissItemAsync(DismissedItem dismissedItem);

        Task<IEnumerable<Item>> GetItemsByUserId(Guid userId);

        Task<Item> GetItemByItemId(Guid itemId);

        Task<List<Item>> GetItemByOfferId(Guid offerId, Guid? userId);

        Task<List<Item>> GetTargetItem(Guid offerId, Guid? userId);

        Task<List<Item>> GetTargetItemById(Guid? itemId);

        Task<List<Item>> GetSourceItem(Guid offerId, Guid? userId);

        Task<Paginated<Item>> GetItems(Guid userId, Guid itemId, decimal? amount, string[]? categories, int limit, string? cursor, decimal? latitude, decimal? longitude, decimal? distance, bool? inMiles);
        Task<Paginated<Item>> GetCashItems(Guid userId, int limit, string? cursor, decimal? latitude, decimal? longitude, decimal? distance, bool? inMiles = false);
        Task<Paginated<Item>> GetAllItems(Guid userId, int limit, string? cursor);
        Task<Paginated<Item>> GetAllItemsByUserForAdmin(Guid userId, int limit, string? cursor);
        Task<Paginated<Item>> GetAllItems(int limit, string? cursor);

        Task<List<KeyValue>> GetCategoriesItemCount();
        Task<int> GetItemCount();
        Task<IEnumerable<Domain.Items.Item>> GetItems(List<Guid> itemIds);
    }
}
