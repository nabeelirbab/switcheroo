using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Domain;
using Domain.Categories;
using Domain.Items;
using Domain.Users;
using Infrastructure.Database;
using Infrastructure.Database.Schema;
using Microsoft.EntityFrameworkCore;
using Amazon.S3;
using Amazon.S3.Transfer;
using System.IO;
using Domain.Offers;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Amazon.S3.Model;

namespace Infrastructure.Items
{
    public class ItemRepository : IItemRepository
    {
        private readonly SwitcherooContext db;
        private readonly ICategoryRepository categoryRepository;

        public ItemRepository(SwitcherooContext db, ICategoryRepository categoryRepository)
        {
            this.db = db;
            this.categoryRepository = categoryRepository;
        }

        public async Task<bool> ArchiveItemAsync(Guid itemId, Guid updatedByUserId)
        {
            var existingItem = await db.Items
                .SingleOrDefaultAsync(z => z.Id == itemId);

            if (existingItem == null) return false;

            var now = DateTime.UtcNow;

            existingItem.ArchivedAt = now;
            existingItem.UpdatedAt = now;
            existingItem.UpdatedByUserId = updatedByUserId;

            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DismissItemAsync(Domain.Items.DismissedItem item)
        {
            var now = DateTime.UtcNow;

            var existing = await db.DismissedItem
                .SingleOrDefaultAsync(x => x.SourceItemId == item.SourceItemId && x.TargetItemId == item.TargetItemId);

            if (existing != null) return false;

            if (!item.CreatedByUserId.HasValue)
                throw new InfrastructureException("No createdByUserId provided");

            var newDismissedItem = new Database.Schema.DismissedItem(item.SourceItemId, item.TargetItemId)
            {
                CreatedByUserId = item.CreatedByUserId.Value,
                UpdatedByUserId = item.CreatedByUserId.Value,
                CreatedAt = now,
                UpdatedAt = now
            };

            await db.DismissedItem.AddAsync(newDismissedItem);
            await db.SaveChangesAsync();

            return true;
        }

        public async Task<Domain.Items.Item> CreateItemAsync(Domain.Items.Item item)
        {
            var now = DateTime.UtcNow;

            if (!item.CreatedByUserId.HasValue)
                throw new InfrastructureException("No createdByUserId provided");

            var newDbItem = new Database.Schema.Item(
                item.Title,
                item.Description,
                item.AskingPrice,
                item.IsHidden,
                item.IsSwapOnly,
                item.Latitude,
                item.Longitude
            )
            {
                CreatedByUserId = item.CreatedByUserId.Value,
                UpdatedByUserId = item.CreatedByUserId.Value,
                CreatedAt = now,
                UpdatedAt = now
            };

            await db.Items.AddAsync(newDbItem);

            // Add item categories
            var dbCategories = await categoryRepository.GetCategoriesByNames(item.Categories);
            newDbItem.ItemCategories.AddRange(dbCategories
                .Select(dbCat => new Database.Schema.ItemCategory(newDbItem.Id, dbCat.Id))
                .ToList());

            // Add item images
            newDbItem.ItemImages.AddRange(item.ImageUrls
                .Select(url => new Database.Schema.ItemImage(url, newDbItem.Id))
                .ToList());

            await db.SaveChangesAsync();

            return await GetItemByItemId(newDbItem.Id);
        }

        public async Task<IEnumerable<Domain.Items.Item>> GetItemsByUserId(Guid userId)
        {
            return await db.Items
                .Where(item => item.CreatedByUserId == userId)
                .Select(Database.Schema.Item.ToDomain)
                .ToListAsync();
        }

        public async Task<Domain.Items.Item> GetItemByItemId(Guid itemId)
        {
            var item = await db.Items
                .Where(z => z.Id == itemId)
                .Select(Database.Schema.Item.ToDomain)
                .SingleOrDefaultAsync();

            if (item == null) throw new InfrastructureException($"Unable to locate item {itemId}");

            return item;
        }

        public async Task<Domain.Items.Item> UpdateItemAsync(Domain.Items.Item item)
        {
            var now = DateTime.UtcNow;
            var existingDbItem = await db.Items
                .Include(z => z.ItemCategories)
                .Include(z => z.ItemImages)
                .SingleOrDefaultAsync(i => i.Id == item.Id);

            if (existingDbItem == null)
            {
                throw new InfrastructureException($"Item not found {item.Id}");
            }

            if (!item.UpdatedByUserId.HasValue)
                throw new InfrastructureException("No updatedByUserId provided");

            existingDbItem.FromDomain(item);
            existingDbItem.UpdatedAt = now;
            existingDbItem.UpdatedByUserId = item.UpdatedByUserId.Value;

            // Item categories
            var dbCategories = await categoryRepository.GetCategoriesByNames(item.Categories);
            existingDbItem.ItemCategories.RemoveAll(z => true);
            existingDbItem.ItemCategories.AddRange(dbCategories
                .Select(dbCat => new Database.Schema.ItemCategory(existingDbItem.Id, dbCat.Id)));

            // Item images
            existingDbItem.ItemImages.RemoveAll(z => true);
            existingDbItem.ItemImages.AddRange(item.ImageUrls
                .Select(url => new Database.Schema.ItemImage(url, existingDbItem.Id))
                .ToList());

            await db.SaveChangesAsync();

            return await GetItemByItemId(existingDbItem.Id);
        }

        public static bool IsDistanceWithinRange(double sourceLatitude, double sourceLongitude, double destinationLatitude, double destinationLongitude, double desiredDistance, bool isUnitMiles = false)
        {
            const double earthRadiusKm = 6371; // Earth's radius in kilometers
            const double milesPerKm = 0.621371; // Conversion factor from kilometers to miles

            // Convert coordinates to radians
            double lat1 = DegreeToRadian(sourceLatitude);
            double lon1 = DegreeToRadian(sourceLongitude);
            double lat2 = DegreeToRadian(destinationLatitude);
            double lon2 = DegreeToRadian(destinationLongitude);

            Console.Write($"lat lngs in rads {lat1} {lon1}  {lat2}  {lon2} ");

            // Calculate differences in latitude and longitude
            double dLat = lat2 - lat1;
            double dLon = lon2 - lon1;

            Console.Write($"lat lngs distances {dLat} {dLon} ");


            // Calculate the Haversine formula components
            double a = Math.Pow(Math.Sin(dLat / 2), 2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(dLon / 2), 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double distanceKm = earthRadiusKm * c;
            double distanceMiles = distanceKm * milesPerKm;

            Console.Write($"What is a: {a}, c: {c} dKm: {distanceKm} dM: {distanceMiles}");


            // Check if the distance is within the desired range
            return isUnitMiles ? distanceMiles <= desiredDistance : distanceKm <= desiredDistance;
        }

        private static double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        public async Task<Paginated<Domain.Items.Item>> GetItems(Guid userId,Guid itemId, decimal? amount, string[]? categories, int limit, string? cursor, decimal? latitude, decimal? longitude, decimal? distance, bool? inMiles = false)
        {
            try
            {
                Console.Clear();
                var myDismissedItems = await db.DismissedItem
                    .Where(z => z.SourceItemId == itemId)
                    .Select(z => z.TargetItemId)
                    .ToListAsync();

                // For 40% limit
                var lowerAmountLimit = Decimal.Multiply((decimal)amount, (decimal)0.60);
                var upperAmountBound = Decimal.Multiply((decimal)amount, (decimal)1.40);
                /*Expression<Func<Database.Schema.Item, bool>> searchPredicate =
                     x =>
                     // If there is an amount it must be within the range of the item in question
                     // (amount == null || x.AskingPrice >= lowerAmountLimit && x.AskingPrice <= upperAmountBound)
                     // If there are categories, they must be on this item
                     // && 
                     (categories == null || x.ItemCategories.Select(z => z.Category.Name).Any(y => categories.Contains(y)))

                     // Skip dismissed items
                     && !myDismissedItems.Contains(x.Id)

                     // Skip hidden items
                     && !x.IsHidden;*/

                // Order by newest created
                var filteredItems = await db.Items
                .AsNoTracking()
                //.Where(searchPredicate)
                .Where(z => z.CreatedByUserId != userId)
                .Where(z => z.AskingPrice >= lowerAmountLimit && z.AskingPrice <= upperAmountBound)
                .Where(x => !myDismissedItems.Contains(x.Id) && !x.IsHidden && x.CreatedByUserId != userId)
                .OrderBy(x => x.Id)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new { x.Id, x.Latitude, x.Longitude })
                .ToListAsync();

                if (filteredItems.Count == 0)
                {
                    throw new InfrastructureException($"No Item found against this price range");
                }

                // Check distance if latitude, longitude, and distance values are provided
                if (latitude.HasValue && longitude.HasValue && distance.HasValue)
                {
                    filteredItems.RemoveAll(x => !IsDistanceWithinRange(
                        (double)latitude.Value,
                        (double)longitude.Value,
                        (double)x.Latitude,
                        (double)x.Longitude,
                        (double)distance.Value,
                        (bool)inMiles));

                }

                if (filteredItems.Count == 0)
                {
                    throw new InfrastructureException($"no filteredItems found in this distance against this filter");
                }

                var itemIdsSorted = filteredItems.Select(x => x.Id).ToList();
                IEnumerable<Guid> requiredIds;

                if (cursor != null)
                {
                    requiredIds = itemIdsSorted
                    .SkipWhile(x => cursor != "" && x.ToString() != cursor)
                    .Skip(1)
                    .Take(limit);
                }
                else
                {
                    requiredIds = itemIdsSorted.Take(limit);
                }

                var totalCount = itemIdsSorted.Count();

                var data = await db.Items
                    .AsNoTracking()
                    .Where(x => requiredIds.Contains(x.Id))
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(Database.Schema.Item.ToDomain)
                    .ToListAsync();

                if (data.Count == 0)
                {
                    throw new InfrastructureException($"No data found against");
                }

                var newCursor = data.Count > 0 ? data.Last().Id.ToString() : "";
                Console.WriteLine($"\nnewCursor:, {newCursor}");

                return new Paginated<Domain.Items.Item>(data, newCursor ?? "", totalCount, data.Count == limit);
            }
            catch (Exception ex)
            {
                throw new InfrastructureException(ex.Message);
            }
        }

        public async Task<Paginated<Domain.Items.Item>> GetAllItems(Guid userId, int limit, string? cursor)
        {
            var myDismissedItems = await db.DismissedItem
                .Where(z => z.CreatedByUserId == userId)
                .Select(z => z.TargetItemId)
                .ToListAsync();


            var filteredItems = await db.Items
                .Where(item =>
                item.CreatedByUserId != userId &&
                !myDismissedItems.Contains(item.Id) && // Skip dismissed items
                !item.IsHidden) // Skip hidden items
                .ToListAsync();

            var itemIdsSorted = filteredItems.Select(x => x.Id).ToList();
            IEnumerable<Guid> requiredIds;

            if (cursor != null)
            {
                requiredIds = itemIdsSorted
                .SkipWhile(x => cursor != "" && x.ToString() != cursor)
                .Skip(1)
                .Take(limit);
            }
            else
            {
                requiredIds = itemIdsSorted.Take(limit);
            }

            var totalCount = itemIdsSorted.Count();

            var data = await db.Items
                .AsNoTracking()
                .Where(x => requiredIds.Contains(x.Id))
                .OrderByDescending(x => x.CreatedAt)
                .Select(Database.Schema.Item.ToDomain)
                .ToListAsync();

            var newCursor = data.Count > 0 ? data.Last().Id.ToString() : "";

            return new Paginated<Domain.Items.Item>(data, newCursor ?? "", totalCount, data.Count == limit);
        }

        public async Task<string> UpdateItemLocation(Guid userId, Guid itemId, decimal? latitude, decimal? longitude)
        {
            var items = await db.Items
                .Where(item => item.CreatedByUserId == userId)
                .ToListAsync();

            if (items.Count == 0)
            {
                return "No items found for the specified user.";
            }
            foreach (var item in items)
            {
                if (item.Id == itemId)
                {
                    // Update the latitude and longitude of each item
                    item.Latitude = latitude;
                    item.Longitude = longitude;

                    // Mark the item as modified in the context
                    db.Entry(item).State = EntityState.Modified;
                }
            }

            try
            {
                // Save the changes to the database
                await db.SaveChangesAsync();
                return "Item locations updated successfully.";
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during database save
                return $"An error occurred: {ex.Message}";
            }
        }
    }
}
