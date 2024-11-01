using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Domain.Categories;
using Domain.Items;
using Infrastructure.Database;
using Infrastructure.Database.Schema;
using Microsoft.EntityFrameworkCore;
using Amazon.S3;
using System.IO;
using Amazon.S3.Model;
using Amazon.Runtime.Internal.Util;
using System.Linq.Expressions;
using Domain.Services;
using Infrastructure.Services;
using Domain.Offers;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;

using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Domain.Users;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
namespace Infrastructure.Items
{
    public class ItemRepository : IItemRepository
    {
        private readonly SwitcherooContext db;
        private readonly ICategoryRepository categoryRepository;
        private readonly ILoggerManager _loggerManager;
        private readonly IUserRoleProvider _userRoleProvider;

        public ItemRepository(SwitcherooContext db, ICategoryRepository categoryRepository, ILoggerManager loggerManager, IUserRoleProvider userRoleProvider)
        {
            this.db = db;
            this.categoryRepository = categoryRepository;
            _loggerManager = loggerManager;
            _userRoleProvider = userRoleProvider;
        }

        public async Task<bool> ArchiveItemAsync(Guid itemId, Guid updatedByUserId)
        {
            var query = db.Items.Where(i => i.Id == itemId);
            if (_userRoleProvider.IsAdminOrSuperAdmin) query = query.IgnoreQueryFilters();
            var existingItem = await query
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
            try
            {

                var now = DateTime.UtcNow;
                Database.Schema.DismissedItem existing;
                if (item.SourceItemId == item.TargetItemId)
                {
                    existing = await db.DismissedItem
                    .SingleOrDefaultAsync(x => x.SourceItemId == item.SourceItemId && x.TargetItemId == item.TargetItemId && x.CreatedByUserId == item.CreatedByUserId);
                }
                else
                {
                    existing = await db.DismissedItem
                        .SingleOrDefaultAsync(x => x.SourceItemId == item.SourceItemId && x.TargetItemId == item.TargetItemId);
                }

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
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, ex.InnerException);
                throw;
            }
        }

        public async Task<Domain.Items.Item> CreateItemAsync(Domain.Items.Item item)
        {
            try
            {
                var now = DateTime.UtcNow;

                if (!item.CreatedByUserId.HasValue)
                    throw new InfrastructureException("No createdByUserId provided");
                if (item.AskingPrice.Equals(null))
                    throw new InfrastructureException($"Item price not be null");


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

                var uploadTasks = new List<Task<string>>();
                // Main image upload task
                if (!string.IsNullOrEmpty(item.MainImageUrl))
                {
                    uploadTasks.Add(ConvertAndUploadImageAsync(item.MainImageUrl));
                }
                // Additional images upload tasks
                if (item.ImageUrls?.Count > 0 && !String.IsNullOrWhiteSpace(item.ImageUrls[0]) && !String.IsNullOrEmpty(item.ImageUrls[0]))
                {
                    uploadTasks.AddRange(item.ImageUrls.Select(ConvertAndUploadImageAsync));
                }
                var uploadedImageUrls = await Task.WhenAll(uploadTasks);

                List<string> s3Urls = new List<string>();
                if (!string.IsNullOrEmpty(item.MainImageUrl))
                {
                    newDbItem.MainImageUrl = uploadedImageUrls.FirstOrDefault();
                    s3Urls = uploadedImageUrls.Skip(1).ToList();
                }
                else
                {
                    s3Urls = uploadedImageUrls.ToList();
                }
                await db.Items.AddAsync(newDbItem);

                // Add item categories
                var dbCategories = await categoryRepository.GetCategoriesByNames(item.Categories);
                newDbItem.ItemCategories.AddRange(dbCategories
                    .Select(dbCat => new ItemCategory(newDbItem.Id, dbCat.Id))
                    .ToList());

                // Add item images
                newDbItem.ItemImages.AddRange(s3Urls
                    .Select(url => new ItemImage(url, newDbItem.Id))
                    .ToList());

                await db.SaveChangesAsync();

                return await GetItemByItemId(newDbItem.Id);
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Infrastructure Exception {ex.Message}");
                throw new InfrastructureException($"Infrastructure Exception {ex.Message}");
            }
        }
        private async Task<string> ConvertAndUploadImageAsync(string base64Image)
        {
            string base64 = base64Image.Split(',').LastOrDefault()?.Trim();
            byte[] imageBytes = Convert.FromBase64String(base64);

            using var image = SixLabors.ImageSharp.Image.Load(imageBytes);
            using var ms = new MemoryStream();
            var encoder = new SixLabors.ImageSharp.Formats.Webp.WebpEncoder { Quality = 100 };
            image.Save(ms, encoder);
            byte[] webPImageBytes = ms.ToArray();

            return await UploadImageToS3Async(webPImageBytes, "image/webp");
        }
        public async Task<IEnumerable<Domain.Items.Item>> GetItemsByUserId(Guid userId)
        {
            try
            {
                var query = db.Items
                    .Where(item => item.CreatedByUserId == userId);
                if (_userRoleProvider.IsAdminOrSuperAdmin) query = query.IgnoreQueryFilters();
                var items = await query
                    .Select(Database.Schema.Item.ToDomain)
                    .ToListAsync();

                foreach (var item in items)
                {
                    // Create a new list excluding the main image URL for each item
                    item.ImageUrls = item.ImageUrls.Where(url => url != item.MainImageUrl).ToList();
                }
                return items;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException($"Exception {ex.Message}");
            }
        }

        public async Task<IEnumerable<Domain.Items.Item>> GetItems(List<Guid> itemIds)
        {
            try
            {
                var query = db.Items
                    .Where(x => itemIds.Contains(x.Id));
                if (_userRoleProvider.IsAdminOrSuperAdmin) query = query.IgnoreQueryFilters();
                var items = await query
                    .Select(Database.Schema.Item.ToDomain)
                    .ToListAsync();


                return items;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException($"Exception {ex.Message}");
            }
        }

        public async Task<List<Domain.Items.Item>> GetItemByOfferId(Guid offerId, Guid? userId)
        {
            try
            {
                var query = db.Offers.Where(o => o.Id.Equals(offerId));
                if (_userRoleProvider.IsAdminOrSuperAdmin) query = query.IgnoreQueryFilters();
                var itemIds = await query
                .Select(o => new
                {
                    SourceItemId = o.SourceItemId,
                    TargetItemId = o.TargetItemId
                }).FirstOrDefaultAsync();

                var sourceItemId = itemIds.SourceItemId;
                var targetItemId = itemIds.TargetItemId;

                var itemsQuery = db.Items
                    .Where(item => (item.Id == sourceItemId || item.Id == targetItemId) && item.CreatedByUserId == userId);
                if (_userRoleProvider.IsAdminOrSuperAdmin) query = query.IgnoreQueryFilters();
                var items = await itemsQuery
                    .Select(Database.Schema.Item.ToDomain)
                    .ToListAsync();

                return items;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException($"Exception {ex.Message}");
            }
        }

        public async Task<List<Domain.Items.Item>> GetTargetItem(Guid offerId, Guid? userId)
        {
            try
            {
                var query = db.Offers.Where(x => x.Id == offerId);
                if (_userRoleProvider.IsAdminOrSuperAdmin) query = query.IgnoreQueryFilters();
                var offer = await query.FirstOrDefaultAsync();
                if (offer.Cash != null)
                {

                    var itemIdsQuery = db.Offers.Where(o => o.Id.Equals(offerId));
                    if (_userRoleProvider.IsAdminOrSuperAdmin) itemIdsQuery = itemIdsQuery.IgnoreQueryFilters();
                    var itemIds = await itemIdsQuery.Select(o => new
                    {
                        SourceItemId = o.SourceItemId,
                        TargetItemId = o.TargetItemId
                    }).FirstOrDefaultAsync();

                    var sourceItemId = itemIds.SourceItemId;
                    var targetItemId = itemIds.TargetItemId;

                    var itemsQuery = db.Items
                    .Where(item => (item.Id == sourceItemId || item.Id == targetItemId));
                    if (_userRoleProvider.IsAdminOrSuperAdmin) itemsQuery = itemsQuery.IgnoreQueryFilters();
                    var items = await itemsQuery
                    .Select(Database.Schema.Item.ToDomain)
                    .ToListAsync();

                    return items;
                }
                else
                {

                    var itemIdsQuery = db.Offers.Where(o => o.Id.Equals(offerId));
                    if (_userRoleProvider.IsAdminOrSuperAdmin) itemIdsQuery = itemIdsQuery.IgnoreQueryFilters();
                    var itemIds = await itemIdsQuery.Select(o => new
                    {
                        SourceItemId = o.SourceItemId,
                        TargetItemId = o.TargetItemId
                    }).FirstOrDefaultAsync();

                    var sourceItemId = itemIds.SourceItemId;
                    var targetItemId = itemIds.TargetItemId;

                    var itemsQuery = db.Items
                        .Where(item => (item.Id == sourceItemId || item.Id == targetItemId) && item.CreatedByUserId != userId);
                    var items = await itemsQuery
                        .Select(Database.Schema.Item.ToDomain)
                        .ToListAsync();

                    return items;
                }
            }
            catch (Exception ex)
            {
                throw new InfrastructureException($"Exception {ex.Message}");
            }
        }

        public async Task<List<Domain.Items.Item>> GetTargetItemById(Guid? itemId)
        {
            try
            {

                var query = db.Items
                    .Where(item => item.Id == itemId);
                if (_userRoleProvider.IsAdminOrSuperAdmin) query = query.IgnoreQueryFilters();
                var items = await query
                    .Select(Database.Schema.Item.ToDomain)
                    .ToListAsync();

                return items;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException($"Exception {ex.Message}");
            }
        }

        public async Task<List<KeyValue>> GetCategoriesItemCount()
        {
            try
            {
                //if (_userRoleProvider.IsAdminOrSuperAdmin)
                //{
                //    return await db.ItemCategories
                //            .IgnoreQueryFilters()
                //            .GroupBy(itemCategory => itemCategory.Category.Name)
                //            .Select(group => new KeyValue(group.Key, group.Count()))
                //            .ToListAsync();
                //}
                var keyValueList = await db.ItemCategories
                                   .Where(ic => ic.Item.IsDeleted != true)
                                   .GroupBy(itemCategory => itemCategory.Category.Name)
                                   .Select(group => new KeyValue(group.Key, group.Count()))
                                   .ToListAsync();

                return keyValueList;

            }
            catch (Exception ex)
            {
                throw new InfrastructureException($"Exception {ex.Message}");
            }
        }
        public async Task<int> GetItemCount()
        {
            try
            {
                //if (_userRoleProvider.IsAdminOrSuperAdmin)
                //{
                //    return await db.Items.IgnoreQueryFilters().CountAsync();
                //}
                int itemsCount = await db.Items.CountAsync();
                return itemsCount;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException($"Exception {ex.Message}");
            }
        }


        public async Task<List<Domain.Items.Item>> GetSourceItem(Guid offerId, Guid? userId)
        {
            try
            {
                var offerQuery = db.Offers.Where(x => x.Id == offerId);
                if (_userRoleProvider.IsAdminOrSuperAdmin) offerQuery = offerQuery.IgnoreQueryFilters();
                var offer = await offerQuery.FirstOrDefaultAsync();
                if (offer?.Cash != null) return null;
                else
                {

                    var itemIdsQuery = db.Offers.Where(o => o.Id.Equals(offerId));
                    if (_userRoleProvider.IsAdminOrSuperAdmin) itemIdsQuery = itemIdsQuery.IgnoreQueryFilters();

                    var itemIds = await itemIdsQuery.Select(o => new
                    {
                        SourceItemId = o.SourceItemId,
                        TargetItemId = o.TargetItemId
                    }).FirstOrDefaultAsync();

                    var sourceItemId = itemIds.SourceItemId;
                    var targetItemId = itemIds.TargetItemId;

                    var itemsQuery = db.Items
                        .Where(item => (item.Id == sourceItemId || item.Id == targetItemId) && item.CreatedByUserId == userId);
                    if (_userRoleProvider.IsAdminOrSuperAdmin) itemsQuery = itemsQuery.IgnoreQueryFilters();
                    var items = await itemsQuery
                        .Select(Database.Schema.Item.ToDomain)
                        .ToListAsync();
                    return items;
                }
            }
            catch (Exception ex)
            {
                throw new InfrastructureException($"Exception {ex.Message}");
            }
        }


        public async Task<Domain.Items.Item> GetItemByItemId(Guid itemId)
        {
            try
            {
                var query = db.Items
                    .Include(i => i.ItemCategories)
                    .ThenInclude(c => c.Category)
                    .Where(z => z.Id == itemId);
                if (_userRoleProvider.IsAdminOrSuperAdmin) query = query.IgnoreQueryFilters();
                var item = await query
                    .Select(Database.Schema.Item.ToDomain)
                    .SingleOrDefaultAsync();
                if (item is null)
                    return null;
                item.ImageUrls = item.ImageUrls.Where(url => url != item.MainImageUrl).ToList();

                if (item == null) throw new InfrastructureException($"Unable to locate item {itemId}");

                return item;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<Domain.Items.Item> UpdateItemAsync(Domain.Items.Item item)
        {
            try
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
                if (item.Equals(null))
                {
                    throw new InfrastructureException($"Item price not be null");
                }
                else
                {
                    if (!item.UpdatedByUserId.HasValue)
                        throw new InfrastructureException("No updatedByUserId provided");

                    existingDbItem.FromDomain(item);
                    existingDbItem.UpdatedAt = now;
                    existingDbItem.UpdatedByUserId = item.UpdatedByUserId.Value;

                    // Item categories
                    var dbCategories = await categoryRepository.GetCategoriesByNames(item.Categories);
                    existingDbItem.ItemCategories.RemoveAll(z => true);
                    existingDbItem.ItemCategories.AddRange(dbCategories
                        .Select(dbCat => new ItemCategory(existingDbItem.Id, dbCat.Id)));
                    if (!string.IsNullOrEmpty(item.MainImageUrl))
                    {
                        if (!item.MainImageUrl.Contains("switcheroofiles.s3.eu-north-1.amazonaws.com"))
                        {

                            string? base64 = item.MainImageUrl?.Split(',').LastOrDefault();
                            base64 = base64.Trim();
                            byte[] mainImageBytes = Convert.FromBase64String(base64);
                            existingDbItem.MainImageUrl = await UploadImageToS3Async(mainImageBytes, "image/jpeg");
                        }
                        else
                        {
                            existingDbItem.MainImageUrl = item.MainImageUrl;

                        }
                    }

                    // Item images
                    List<string> imagesBase64 = item.ImageUrls;
                    List<string> s3Urls = new List<string>();
                    foreach (string base64String in imagesBase64)
                    {
                        if (!base64String.Contains("switcheroofiles.s3.eu-north-1.amazonaws.com"))
                        {
                            // Upload ImageUrls to S3
                            string? base64 = base64String?.Split(',').LastOrDefault();
                            base64 = base64.Trim();
                            byte[] imageBytes = Convert.FromBase64String(base64);
                            string uploadedImageUrl = await UploadImageToS3Async(imageBytes, "image/jpeg");
                            s3Urls.Add(uploadedImageUrl);
                        }
                        else
                        {
                            s3Urls.Add(base64String);
                        }

                    }

                    existingDbItem.ItemImages.RemoveAll(z => true);
                    existingDbItem.ItemImages.AddRange(s3Urls
                        .Select(url => new ItemImage(url, existingDbItem.Id))
                        .ToList());

                    await db.SaveChangesAsync();

                    return await GetItemByItemId(existingDbItem.Id);
                }
            }
            catch (Exception ex)
            {
                throw new InfrastructureException($"Infrastructure Exception {ex.Message}");
            }
        }

        public static bool IsDistanceWithinRange(double sourceLatitude, double sourceLongitude, double destinationLatitude, double destinationLongitude, double desiredDistance, bool isUnitMiles = false)
        {
            const double EarthRadiusKm = 6371.0; // Earth's radius in kilometers
            const double MilesPerKm = 0.621371; // Conversion factor from kilometers to miles

            // Convert coordinates from degrees to radians
            double lat1 = DegreeToRadian(sourceLatitude);
            double lon1 = DegreeToRadian(sourceLongitude);
            double lat2 = DegreeToRadian(destinationLatitude);
            double lon2 = DegreeToRadian(destinationLongitude);

            // Calculate differences in latitude and longitude
            double dLat = lat2 - lat1;
            double dLon = lon2 - lon1;

            // Calculate the Haversine formula components
            double a = Math.Pow(Math.Sin(dLat / 2), 2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(dLon / 2), 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double distanceKm = EarthRadiusKm * c;
            double distanceMiles = distanceKm * MilesPerKm;

            // Check if the distance is within the desired range
            //Console.WriteLine($"SourceLatitude: {sourceLatitude}, SourceLongitude: {sourceLongitude}, DestinationLatitude: {destinationLatitude}, DestinationLongitude: {destinationLongitude}, DesiredDistance: {desiredDistance}, IsUnitMiles: {isUnitMiles}");
            //Console.WriteLine($"DistanceKm: {distanceKm}, DistanceMiles: {distanceMiles}");
            return isUnitMiles ? distanceMiles <= desiredDistance : distanceKm <= desiredDistance;
        }

        private static double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        public async Task<Paginated<Domain.Items.Item>> GetItems(Guid userId, Guid itemId, decimal? amount, string[]? categories, int limit, string? cursor, decimal? latitude, decimal? longitude, decimal? distance, bool? inMiles = false)
        {
            try
            {
                Console.Clear();
                // if any dismised item
                var myDismissedItems = await db.DismissedItem
                    .Where(z => z.SourceItemId.Equals(itemId))
                    .Select(z => z.TargetItemId)
                    .ToListAsync();

                // For 40% limit
                var lowerAmountLimit = Decimal.Multiply((decimal)amount, (decimal)0.60);
                var upperAmountBound = Decimal.Multiply((decimal)amount, (decimal)1.40);
                Expression<Func<Database.Schema.Item, bool>> searchPredicate =
                     x =>
                     // If there is an amount it must be within the range of the item in question
                      (amount == null || x.AskingPrice >= lowerAmountLimit && x.AskingPrice <= upperAmountBound)
                     // If there are categories, they must be on this item
                      ||
                     (categories == null || x.ItemCategories.Select(z => z.Category.Name).Any(y => categories.Contains(y)))

                     // Skip dismissed items
                     && !myDismissedItems.Contains(x.Id)

                     // Skip hidden items
                     && !x.IsHidden;

                // Order by newest created
                var filteredItems = await db.Items
                .AsNoTracking()
                .Include(z => z.ItemCategories)
                .ThenInclude(z => z.Category)
                .Where(searchPredicate)
                .Where(z => z.CreatedByUserId != userId)
                //.Where(z => z.AskingPrice >= lowerAmountLimit && z.AskingPrice <= upperAmountBound)
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
                //get created offers created by this item
                var createdOffers = db.Offers.Where(x => x.SourceItemId == itemId && x.TargetStatus == 0).Select(offer => offer.TargetItemId).ToList();

                if (createdOffers.Count != 0)
                {
                    filteredItems = filteredItems
                       .Where(item => !createdOffers.Contains(item.Id)).ToList();
                }

                //get initiated offers created by this item
                var initiatedOffers = db.Offers.Where(x => x.SourceItemId == itemId && x.TargetStatus == OfferStatus.Initiated).Select(offer => offer.TargetItemId).ToList();
                if (initiatedOffers.Count != 0)
                {
                    filteredItems = filteredItems
                       .Where(item => !initiatedOffers.Contains(item.Id)).ToList();
                }

                //get initiated offer for this item
                var matchedOffers = db.Offers.Where(x => x.TargetItemId == itemId && x.TargetStatus == OfferStatus.Initiated).Select(offer => offer.SourceItemId).ToList();
                if (matchedOffers.Count != 0)
                {
                    filteredItems = filteredItems
                       .Where(item => !matchedOffers.Contains(item.Id)).ToList();
                }


                if (filteredItems.Count == 0)
                {
                    throw new InfrastructureException($"All offers are created against this items and filteres");
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
                    .OrderBy(x => x.ItemCategories.Any(ic => categories.Contains(ic.Category.Name)) ? 0 : 1)
                    .ThenByDescending(x => x.CreatedAt)
                    .ThenByDescending(x => x.ItemCategories.Count())
                    .Select(Database.Schema.Item.ToDomain)
                    .ToListAsync();


                if (data.Count == 0)
                {
                    throw new InfrastructureException($"No data found against");
                }

                var itemOffers = await db.Offers
                 .Where(o => (o.SourceItem.CreatedByUserId == userId || o.TargetItem.CreatedByUserId == userId || o.CreatedByUserId == userId)
                         && (requiredIds.Contains(o.SourceItemId) || requiredIds.Contains(o.TargetItemId)))
                 .Select(o => new
                 {
                     o.Id,
                     o.SourceItemId,
                     SourceItemTitle = o.SourceItem.Title,
                     o.TargetItemId,
                     TargetItemTitle = o.TargetItem.Title,
                     o.Cash
                 }).ToListAsync();


                foreach (var item in data)
                {
                    // Create a new list excluding the main image URL for each item
                    item.ImageUrls = item.ImageUrls.Where(url => url != item.MainImageUrl).ToList();
                    var matchingOfferAgainstItem = itemOffers.Find(o => (o.SourceItemId == item.Id || o.TargetItemId == item.Id) && o.Cash > 0 == false);
                    var cashOfferAgainstItem = itemOffers.Find(o => (o.SourceItemId == item.Id || o.TargetItemId == item.Id) && o.Cash > 0 == true);
                    if (matchingOfferAgainstItem != null)
                    {
                        item.HasMatchingOffer = true;
                    }
                    if (cashOfferAgainstItem != null)
                    {
                        item.HasCashOffer = true;
                        item.CashOfferValue = cashOfferAgainstItem.Cash;
                    }

                }
                var newCursor = data.Count > 0 ? data.Last().Id.ToString() : "";

                _loggerManager.LogWarn($"Returning Items from database: {data.Count}");
                _loggerManager.LogWarn("-----------------------------------------------");
                return new Paginated<Domain.Items.Item>(data, newCursor ?? "", totalCount, data.Count == limit);
            }
            catch (Exception ex)
            {
                throw new InfrastructureException(ex.Message);
            }
        }

        public async Task<Paginated<Domain.Items.Item>> GetCashItems(Guid userId, int limit, string? cursor, decimal? latitude, decimal? longitude, decimal? distance, bool? inMiles = false)
        {
            try
            {
                Console.Clear();
                // if any dismised item
                var myDismissedItems = await db.DismissedItem
                    .Where(z => z.CreatedByUserId.Equals(userId) && z.SourceItemId == z.TargetItemId)
                    .Select(z => z.TargetItemId)
                    .ToListAsync();
                Console.WriteLine("Length of Dismissed Items is ", myDismissedItems?.Count());

                Expression<Func<Database.Schema.Item, bool>> searchPredicate =
                     x =>
                     // Skip dismissed items
                     !myDismissedItems.Contains(x.Id)

                     // Skip hidden items
                     && !x.IsHidden;

                //get created offers created by this user
                var createdOfferByUser = await db.Offers.Where(o => o.CreatedByUserId.Equals(userId) && o.SourceItemId == o.TargetItemId).ToListAsync();
                Console.WriteLine("Length of Created offers is :", createdOfferByUser.Count());
                // Order by newest created
                var filteredItems = await db.Items
                .AsNoTracking()
                .Include(z => z.ItemCategories)
                .ThenInclude(z => z.Category)
                .Where(searchPredicate)
                .Where(z => z.CreatedByUserId != userId)
                .Where(z => z.IsSwapOnly == true)
                .Where(x => !myDismissedItems.Contains(x.Id) && !x.IsHidden && x.CreatedByUserId != userId)
                .Where(x => !createdOfferByUser.Select(o => o.TargetItemId).Contains(x.Id))
                .OrderBy(x => x.Id)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new { x.Id, x.Latitude, x.Longitude })
                .ToListAsync();

                if (filteredItems.Count == 0)
                {
                    throw new InfrastructureException($"No Item found against this price range");
                }
                Console.WriteLine("Filtered Items length is", filteredItems.Count());
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
                    .OrderByDescending(x => x.ItemCategories.Count())
                    .Select(Database.Schema.Item.ToDomain)
                    .ToListAsync();

                if (data.Count == 0)
                {
                    throw new InfrastructureException($"No data found against");
                }
                foreach (var item in data)
                {
                    // Create a new list excluding the main image URL for each item
                    item.ImageUrls = item.ImageUrls.Where(url => url != item.MainImageUrl).ToList();
                }
                var newCursor = data.Count > 0 ? data.Last().Id.ToString() : "";

                return new Paginated<Domain.Items.Item>(data, newCursor ?? "", totalCount, data.Count == limit);
            }
            catch (Exception ex)
            {
                throw new InfrastructureException(ex.Message);
            }
        }
        public async Task<Paginated<Domain.Items.Item>> GetAllItems(Guid userId, int limit, string? cursor)
        {
            Guid? cursorGuid = cursor != null ? Guid.Parse(cursor) : (Guid?)null;
            var query = db.Items
                .Where(item => item.CreatedByUserId != userId &&
                               !db.DismissedItem.Any(di => di.CreatedByUserId == userId && di.TargetItemId == item.Id) && // Skip dismissed items
                               !item.IsHidden)
                .AsNoTracking();

            if (cursorGuid.HasValue)
            {
                query = query.Where(item => item.Id.CompareTo(cursorGuid.Value) > 0);
            }

            var totalCountQuery = await query.CountAsync();

            var paginatedItems = await query
                .OrderBy(item => item.Id)
                .Take(limit + 1)
                .Select(Database.Schema.Item.ToDomain)
                .ToListAsync();

            string? newCursor = paginatedItems.Count > limit ? paginatedItems.Last().Id.ToString() : null;
            if (newCursor != null)
            {
                paginatedItems = paginatedItems.Take(limit).ToList();
            }

            var totalCount = totalCountQuery;

            foreach (var item in paginatedItems)
            {
                item.ImageUrls = item.ImageUrls.Where(url => url != item.MainImageUrl).ToList();
            }

            return new Paginated<Domain.Items.Item>(paginatedItems, newCursor ?? "", totalCount, paginatedItems.Count == limit);
        }
        public async Task<Paginated<Domain.Items.Item>> GetAllItemsByUserForAdmin(Guid userId, int limit, string? cursor)
        {
            Guid? cursorGuid = cursor != null ? Guid.Parse(cursor) : (Guid?)null;
            var query = db.Items
                .IgnoreQueryFilters()
                .Where(item => item.CreatedByUserId == userId)
                .AsNoTracking();

            if (cursorGuid.HasValue)
            {
                query = query.Where(item => item.Id.CompareTo(cursorGuid.Value) > 0);
            }

            var totalCountQuery = await query.CountAsync();

            var paginatedItems = await query
                .OrderBy(item => item.Id)
                .Take(limit + 1)
                .Select(Database.Schema.Item.ToDomain)
                .ToListAsync();

            string? newCursor = paginatedItems.Count > limit ? paginatedItems.Last().Id.ToString() : null;
            if (newCursor != null)
            {
                paginatedItems = paginatedItems.Take(limit).ToList();
            }

            var totalCount = totalCountQuery;

            foreach (var item in paginatedItems)
            {
                item.ImageUrls = item.ImageUrls.Where(url => url != item.MainImageUrl).ToList();
            }

            return new Paginated<Domain.Items.Item>(paginatedItems, newCursor ?? "", totalCount, paginatedItems.Count == limit);
        }
        public async Task<Paginated<Domain.Items.Item>> GetAllItems(int limit, string? cursor)
        {

            Guid? cursorGuid = cursor != null ? Guid.Parse(cursor) : (Guid?)null;
            var query = db.Items.IgnoreQueryFilters().AsNoTracking();
            if (cursorGuid.HasValue)
            {
                query = query.Where(item => item.Id.CompareTo(cursorGuid.Value) > 0);
            }
            var totalCountQuery = await query.CountAsync();
            var paginatedItems = await query
                .OrderBy(item => item.Id)
                .Take(limit + 1)
                .Select(Database.Schema.Item.ToDomain)
                .ToListAsync();

            string? newCursor = paginatedItems.Count > limit ? paginatedItems.Last().Id.ToString() : null;
            if (newCursor != null)
            {
                paginatedItems = paginatedItems.Take(limit).ToList();
            }

            var totalCount = totalCountQuery;

            foreach (var item in paginatedItems)
            {
                item.ImageUrls = item.ImageUrls.Where(url => url != item.MainImageUrl).ToList();
            }

            return new Paginated<Domain.Items.Item>(paginatedItems, newCursor ?? "", totalCount, paginatedItems.Count == limit);
        }


        public async Task<string> UpdateItemLocation(Guid itemId, decimal? latitude, decimal? longitude)
        {
            var item = await db.Items
                .Where(item => item.Id == itemId)
                .FirstOrDefaultAsync();

            if (item == null)
            {
                return "No items found for the specified user.";
            }

            item.Latitude = latitude;
            item.Longitude = longitude;

            db.Entry(item).State = EntityState.Modified;

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

        public async Task<string> UpdateAllItemsLocation(Guid userId, decimal? latitude, decimal? longitude)
        {
            var items = await db.Items
                .Where(item => item.CreatedByUserId == userId)
                .ToListAsync();

            if (items.Count == 0) { throw new InfrastructureException("No item found"); }
            foreach (var item in items)
            {
                item.Latitude = latitude;
                item.Longitude = longitude;

                db.Entry(item).State = EntityState.Modified;
            }
            try
            {
                await db.SaveChangesAsync();
                return "Item locations updated successfully.";
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }
        }

        public async Task<bool> DeleteItemAsync(Guid itemId, Guid deletedByUserId)
        {
            var strategy = db.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await db.Database.BeginTransactionAsync();
                try
                {
                    var items = await db.Items
                        .Where(u => u.Id == itemId)
                        .ToListAsync();

                    var offersAgainstItem = await db.Offers
                        .Where(u => u.SourceItemId.Equals(itemId) || u.TargetItemId.Equals(itemId))
                        .ToListAsync();
                    if (offersAgainstItem.Count > 0)
                    {
                        offersAgainstItem.ForEach(u => { u.IsDeleted = true; u.DeletedByUserId = deletedByUserId; u.DeletedAt = DateTimeOffset.UtcNow; });
                        var offerIds = offersAgainstItem.Select(offer => offer.Id).ToList();
                        var messagesToUpdate = await db.Messages
                            .Where(message => offerIds.Contains(message.OfferId))
                            .ToListAsync();
                        messagesToUpdate.ForEach(u => { u.IsDeleted = true; u.DeletedByUserId = deletedByUserId; u.DeletedAt = DateTimeOffset.UtcNow; });
                    }
                    items.ForEach(u => { u.IsDeleted = true; u.DeletedByUserId = deletedByUserId; u.DeletedAt = DateTimeOffset.UtcNow; });
                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });

            return true;
        }
        public async Task<bool> DeleteItemPermanentlyAsync(Guid itemId)
        {
            var strategy = db.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await db.Database.BeginTransactionAsync();
                try
                {
                    // Fetch the offers related to the item
                    var offersAgainstItem = await db.Offers
                        .IgnoreQueryFilters()
                        .Where(u => u.SourceItemId.Equals(itemId) || u.TargetItemId.Equals(itemId))
                        .ToListAsync();

                    if (offersAgainstItem.Count > 0)
                    {
                        var offerIds = offersAgainstItem.Select(offer => offer.Id).ToList();

                        // Fetch and delete related messages
                        var messagesToDelete = await db.Messages
                            .IgnoreQueryFilters()
                            .Where(message => offerIds.Contains(message.OfferId))
                            .ToListAsync();
                        db.Messages.RemoveRange(messagesToDelete);

                        // Delete the offers
                        db.Offers.RemoveRange(offersAgainstItem);
                    }

                    // Fetch and delete the item
                    var items = await db.Items
                        .IgnoreQueryFilters()
                        .Where(u => u.Id == itemId)
                        .ToListAsync();
                    db.Items.RemoveRange(items);

                    // Save all changes
                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });

            return true;
        }


        public async Task<bool> RestoreItemAsync(Guid itemId)
        {
            var strategy = db.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await db.Database.BeginTransactionAsync();
                try
                {
                    // Restore the item
                    var items = await db.Items
                        .IgnoreQueryFilters()
                        .Where(i => i.Id == itemId && i.IsDeleted)
                        .ToListAsync();
                    if (!items.Any())
                    {
                        return;
                    }
                    items.ForEach(item =>
                    {
                        item.IsDeleted = false;
                        item.DeletedByUserId = null;
                        item.DeletedAt = null;
                    });

                    // Restore offers related to the item
                    var offersAgainstItem = await db.Offers
                        .IgnoreQueryFilters()
                        .Where(offer => (offer.SourceItemId.Equals(itemId) || offer.TargetItemId.Equals(itemId)) && offer.IsDeleted)
                        .ToListAsync();
                    offersAgainstItem.ForEach(offer =>
                    {
                        offer.IsDeleted = false;
                        offer.DeletedByUserId = null;
                        offer.DeletedAt = null;
                    });
                    var offerIds = offersAgainstItem.Select(offer => offer.Id).ToList();

                    // Restore messages related to the restored offers
                    var messagesToUpdate = await db.Messages
                        .IgnoreQueryFilters()
                        .Where(message => offerIds.Contains(message.OfferId) && message.IsDeleted)
                        .ToListAsync();
                    messagesToUpdate.ForEach(message =>
                    {
                        message.IsDeleted = false;
                        message.DeletedByUserId = null;
                        message.DeletedAt = null;
                    });
                    var userIds = items.Select(i => i.CreatedByUserId).ToList();
                    var userFcmTokens = await db.Users.IgnoreQueryFilters().Where(u => userIds.Contains(u.Id)).Select(u => u.FCMToken).ToListAsync();
                    if (userFcmTokens.Count > 0)
                    {
                        var app = FirebaseApp.DefaultInstance;
                        var messaging = FirebaseMessaging.GetMessaging(app);

                        var message = new FirebaseAdmin.Messaging.MulticastMessage()
                        {
                            Tokens = userFcmTokens,
                            Notification = new Notification
                            {
                                Title = "Item Restored",
                                Body = "One of your Item has been restored"
                            }
                        };
                        var response = await messaging.SendMulticastAsync(message);
                    }

                    // Commit all changes
                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new InfrastructureException(ex.Message);
                }
            });

            return true;
        }

        private async Task<string> UploadImageToS3Async(byte[] imageBytes, string contentType)
        {
            string fileName = Guid.NewGuid().ToString();


            using (var s3Client = new AmazonS3Client("AKIA6EM2LZWU3ULXZ32E", "skgJAOA7bXo6aWe74nuP1UZuCbyO4UVB7t4zMei9", Amazon.RegionEndpoint.EUNorth1))
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = "switcheroofiles",
                    Key = $"{fileName}.jpg",
                    InputStream = new MemoryStream(imageBytes),
                    ContentType = contentType,
                };

                PutObjectResponse response = await s3Client.PutObjectAsync(putRequest);

                // Get the URL of the uploaded image
                return $"https://switcheroofiles.s3.eu-north-1.amazonaws.com/{fileName}.jpg"; ;
            }
        }
    }
}
