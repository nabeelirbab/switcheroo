using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Domain.Users;
using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Infrastructure.UserManagement
{
    public class UserRepository : IUserRepository
    {
        private readonly SwitcherooContext db;
        private readonly ILogger<UserRepository> _logger;
        private readonly Domain.Users.IUserRoleProvider _userRoleProvider;

        public UserRepository(SwitcherooContext db, ILogger<UserRepository> logger, IUserRoleProvider userRoleProvider)
        {
            this.db = db;
            _logger = logger;
            _logger.LogDebug("Nlog is integrated to User repository");
            _userRoleProvider = userRoleProvider;
        }

        public async Task<User> GetByEmail(string email)
        {
            var query = db.Users
                .AsNoTracking()
                .Where(user => user.Email == email);
            if (_userRoleProvider.IsAdminOrSuperAdmin) query = query.IgnoreQueryFilters();
            var user = await query
                .Select(Database.Schema.User.ToDomain)
                .SingleOrDefaultAsync();

            if (user == null) throw new InfrastructureException($"Couldn't find user with email {email}");

            return user;
        }
        public async Task<Paginated<User>> GetAllUsers(int limit, string? cursor)
        {
            var usersIdsSorted = await db.Users.AsNoTracking().IgnoreQueryFilters().OrderByDescending(u => u.CreatedAt).Select(x => x.Id).ToListAsync();

            IEnumerable<Guid> requiredIds;
            if (cursor != null)
            {
                requiredIds = usersIdsSorted
                    .SkipWhile(x => x.ToString() != cursor)
                    .Skip(1)
                    .Take(limit);
            }
            else
            {
                requiredIds = usersIdsSorted.Take(limit);
            }

            var data = await db.Users
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(x => requiredIds.Contains(x.Id))
                .OrderByDescending(x => x.CreatedAt)
                .Select(Database.Schema.User.ToDomain)
                .ToListAsync();
            var allItems = await db.Items.AsNoTracking().IgnoreQueryFilters().Where(i => requiredIds.Contains(i.CreatedByUserId)).ToListAsync();
            var allItemIds = allItems.Select(i => i.Id).ToList();
            var allOffers = await db.Offers.IgnoreQueryFilters()
                    .Where(offer =>
                        allItemIds.Contains(offer.SourceItemId) || allItemIds.Contains(offer.TargetItemId))
                    .Where(offer =>
                        offer.SourceStatus == Database.Schema.OfferStatus.Initiated &&
                        offer.TargetStatus == Database.Schema.OfferStatus.Initiated).ToListAsync();

            foreach (var user in data)
            {
                user.ItemCount = allItems.Where(i => i.CreatedByUserId == user.Id).Count();

                var userItems = allItems.Where(i => i.CreatedByUserId == user.Id).ToList();

                var sourceItemIds = allItems.Where(i => i.CreatedByUserId == user.Id).Select(i => i.Id).ToList();
                user.MatchedItemCount = allOffers
                    .Where(offer =>
                        sourceItemIds.Contains(offer.SourceItemId) || sourceItemIds.Contains(offer.TargetItemId))
                    .Where(offer =>
                        offer.SourceStatus == Database.Schema.OfferStatus.Initiated &&
                        offer.TargetStatus == Database.Schema.OfferStatus.Initiated)
                    .Count();
            }

            var newCursor = data.Count > 0 ? data.Last().Id.ToString() : "";
            var totalCount = usersIdsSorted.Count;

            return new Paginated<User>(data, newCursor, totalCount, data.Count == limit);
        }


        public async Task<User> GetById(Guid? id)
        {
            var query = db.Users
                .AsNoTracking()
                .Where(user => user.Id == id.Value);
            if (_userRoleProvider.IsAdminOrSuperAdmin) query = query.IgnoreQueryFilters();
            var user = await query
                .Select(Database.Schema.User.ToDomain)
                .SingleOrDefaultAsync();

            if (user == null) throw new InfrastructureException($"Couldn't find user {id}");

            return user;
        }

        public async Task<List<User>> GetUserByUserId(List<Guid> userIds)
        {

            var query = db.Users
                .AsNoTracking()
                .Where(user => userIds.Contains(user.Id));
            if (_userRoleProvider.IsAdminOrSuperAdmin) query = query.IgnoreQueryFilters();
            var users = await query
                .Select(Database.Schema.User.ToDomain)
                .ToListAsync();

            return users;
        }
        public async Task<string> GetTargetUserForMessage(Guid? userId, Guid offerId, bool getFCMToken = false)
        {
            var query = db.Offers
                .Where(o => o.Id == offerId)
                .Select(o => new
                {
                    SourceUser = o.CreatedByUserId,
                    SourceUserFcm = getFCMToken ? o.CreatedByUser.FCMToken : null,
                    TargetUser = o.TargetItem.CreatedByUserId,
                    TargetUserFcm = getFCMToken ? o.TargetItem.CreatedByUser.FCMToken : null
                });
            if (_userRoleProvider.IsAdminOrSuperAdmin) query = query.IgnoreQueryFilters();

            var offerUsers = await query.FirstOrDefaultAsync();

            if (offerUsers == null)
            {
                return null; // Or handle as appropriate
            }

            if (userId == offerUsers.SourceUser)
            {
                return getFCMToken ? offerUsers.TargetUserFcm : offerUsers.TargetUser.ToString();
            }
            else
            {
                return getFCMToken ? offerUsers.SourceUserFcm : offerUsers.SourceUser.ToString();
            }
        }


        public async Task<List<User>> GetTargetUser(Guid? userId, Guid offerId)
        {
            try
            {

                var query = db.Offers.Where(o => o.Id.Equals(offerId)).Select(o => new
                {
                    SourceItemId = o.SourceItemId,
                    TargetItemId = o.TargetItemId,
                    OfferCreatedBy = o.CreatedByUserId,
                    Cash = o.Cash,
                });
                if (_userRoleProvider.IsAdminOrSuperAdmin) query = query.IgnoreQueryFilters();
                var itemIds = await query.FirstOrDefaultAsync();


                var sourceItemId = itemIds.SourceItemId;
                var targetItemId = itemIds.TargetItemId;
                var Cash = itemIds.Cash != null && itemIds.Cash > 0 ? true : false;
                List<Domain.Items.Item> items;
                if (Cash)
                {
                    var itemsQuery = db.Items
                    .Where(item => (item.Id == sourceItemId || item.Id == targetItemId))
                    .Select(Database.Schema.Item.ToDomain);
                    if (_userRoleProvider.IsAdminOrSuperAdmin) itemsQuery = itemsQuery.IgnoreQueryFilters();
                    items = await itemsQuery.ToListAsync();
                }
                else
                {
                    var itemsQuery = db.Items
                        .Where(item => (item.Id == sourceItemId || item.Id == targetItemId) && item.CreatedByUserId != userId)
                        .Select(Database.Schema.Item.ToDomain);
                    if (_userRoleProvider.IsAdminOrSuperAdmin) itemsQuery = itemsQuery.IgnoreQueryFilters();
                    items = await itemsQuery.ToListAsync();
                }

                var creatorIds = items.Select(item => item.CreatedByUserId).Distinct().ToList();

                var users = await db.Users
                    .AsNoTracking()
                    .Where(user => creatorIds.Contains(user.Id))
                    .Select(Database.Schema.User.ToDomain)
                    .ToListAsync();

                return users;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException($"Exception {ex.Message}");
            }
        }

        public async Task<List<User>> GetUserById(Guid? userId)
        {
            try
            {
                var query = db.Users
                    .AsNoTracking()
                    .Where(user => user.Id == userId)
                    .Select(Database.Schema.User.ToDomain);
                if (_userRoleProvider.IsAdminOrSuperAdmin) query = query.IgnoreQueryFilters();
                var users = await query.ToListAsync();

                return users;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException($"Exception {ex.Message}");
            }
        }

        public async Task<List<KeyValue>> GetUsersGenderCount()
        {
            try
            {
                var query = db.Users
                                      .Where(user => user.Gender != null);
                if (_userRoleProvider.IsAdminOrSuperAdmin) query = query.IgnoreQueryFilters();
                var keyValueList = await query.GroupBy(user => user.Gender)
                                      .Select(group => new KeyValue(group.Key, group.Count()))
                                      .ToListAsync();
                return keyValueList;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException($"Exception {ex.Message}");
            }
        }

        public async Task<User> UpdateUserDateOfBirth(Guid id, DateTime? dateOfBirth)
        {
            var dbUser = await db.Users.SingleOrDefaultAsync(x => x.Id == id);

            if (dbUser == null) throw new InfrastructureException($"Couldn't find user with id {id}");

            dbUser.DateOfBirth = dateOfBirth;

            db.Users.Update(dbUser);
            await db.SaveChangesAsync();

            return await GetById(id);
        }

        public async Task<User> UpdateUserDistance(Guid id, int? distance)
        {
            var dbUser = await db.Users.SingleOrDefaultAsync(x => x.Id == id);

            if (dbUser == null) throw new InfrastructureException($"Couldn't find user with id {id}");

            dbUser.Distance = distance;

            db.Users.Update(dbUser);
            await db.SaveChangesAsync();

            return await GetById(id);
        }

        public async Task<User> UpdateUserLocation(Guid id, decimal? latitude, decimal? longitude)
        {
            var dbUser = await db.Users.SingleOrDefaultAsync(x => x.Id == id);

            if (dbUser == null) throw new InfrastructureException($"Couldn't find user with id {id}");

            dbUser.Latitude = latitude;
            dbUser.Longitude = longitude;

            db.Users.Update(dbUser);
            await db.SaveChangesAsync();

            return await GetById(id);
        }

        public async Task<User> UpdateUserFCMToken(Guid id, string? fcmToken)
        {
            var dbUser = await db.Users.SingleOrDefaultAsync(x => x.Id == id);

            if (dbUser == null) throw new InfrastructureException($"Couldn't find user with id {id}");

            if (string.IsNullOrEmpty(fcmToken))
            {
                throw new InfrastructureException($"token not found in payload");
            }
            else
            {
                dbUser.FCMToken = fcmToken;
                db.Users.Update(dbUser);
                await db.SaveChangesAsync();
            }

            return await GetById(id);
        }


        public async Task<User> UpdateUserEmail(Guid id, string email)
        {
            var dbUser = await db.Users.SingleOrDefaultAsync(x => x.Id == id);

            if (dbUser == null) throw new InfrastructureException($"Couldn't find user with id {id}");
            if (await db.Users.AnyAsync(z => z.Email == email)) throw new InfrastructureException($"Email already in use {email}");

            dbUser.Email = email;

            db.Users.Update(dbUser);
            await db.SaveChangesAsync();

            return await GetById(id);
        }

        public async Task<User> UpdateUserGender(Guid id, string? gender)
        {
            var dbUser = await db.Users.SingleOrDefaultAsync(x => x.Id == id);

            if (dbUser == null) throw new InfrastructureException($"Couldn't find user with id {id}");

            dbUser.Gender = gender;

            db.Users.Update(dbUser);
            await db.SaveChangesAsync();

            return await GetById(id);
        }

        public async Task<User> UpdateUserMobile(Guid id, string? mobile)
        {
            var dbUser = await db.Users.SingleOrDefaultAsync(x => x.Id == id);

            if (dbUser == null) throw new InfrastructureException($"Couldn't find user with id {id}");

            dbUser.Mobile = mobile;

            db.Users.Update(dbUser);
            await db.SaveChangesAsync();

            return await GetById(id);
        }

        public async Task<User> UpdateUserName(Guid id, string firstName, string lastName)
        {
            var dbUser = await db.Users.SingleOrDefaultAsync(x => x.Id == id);

            if (dbUser == null) throw new InfrastructureException($"Couldn't find user with id {id}");

            dbUser.FirstName = firstName;
            dbUser.LastName = lastName;

            db.Users.Update(dbUser);
            await db.SaveChangesAsync();

            return await GetById(id);
        }

        public async Task<User> UpdateUserProfileDetails(Guid id, string? blurb, string? avatarUrl)
        {
            var dbUser = await db.Users.SingleOrDefaultAsync(x => x.Id == id);

            if (dbUser == null) throw new InfrastructureException($"Couldn't find user with id {id}");

            dbUser.Blurb = blurb;
            dbUser.AvatarUrl = avatarUrl;
            dbUser.UpdatedAt = DateTime.UtcNow;

            db.Users.Update(dbUser);
            await db.SaveChangesAsync();

            return await GetById(id);
        }

        public async Task<bool> DeleteUser(List<Guid> userIds, Guid deletedByUserId)
        {
            if (userIds == null || !userIds.Any())
            {
                throw new ArgumentNullException(nameof(userIds), "User IDs cannot be null or empty.");
            }

            var strategy = db.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await db.Database.BeginTransactionAsync();
                try
                {
                    // Soft delete Users
                    var usersToUpdate = await db.Users
                        .Where(u => userIds.Contains(u.Id))
                        .ToListAsync();

                    if (!usersToUpdate.Any())
                    {
                        throw new KeyNotFoundException("No users found with the provided IDs.");
                    }
                    usersToUpdate.ForEach(u => { u.IsDeleted = true; u.DeletedByUserId = deletedByUserId; u.DeletedAt = DateTimeOffset.UtcNow; });

                    // Soft delete Items
                    var itemsToUpdate = await db.Items
                        .Where(item => userIds.Contains(item.CreatedByUserId))
                        .ToListAsync();
                    itemsToUpdate.ForEach(u => { u.IsDeleted = true; u.DeletedByUserId = deletedByUserId; u.DeletedAt = DateTimeOffset.UtcNow; });

                    // Get item IDs for later queries
                    var itemIds = itemsToUpdate.Select(item => item.Id).ToList();

                    // Soft delete Offers
                    var offersToUpdate = await db.Offers
                        .Where(offer => userIds.Contains(offer.CreatedByUserId) || itemIds.Contains(offer.TargetItemId))
                        .ToListAsync();
                    offersToUpdate.ForEach(u => { u.IsDeleted = true; u.DeletedByUserId = deletedByUserId; u.DeletedAt = DateTimeOffset.UtcNow; });

                    // Soft delete Messages linked to the Offers
                    var offerIds = offersToUpdate.Select(offer => offer.Id).ToList();
                    var messagesToUpdate = await db.Messages
                        .Where(message => offerIds.Contains(message.OfferId))
                        .ToListAsync();
                    messagesToUpdate.ForEach(u => { u.IsDeleted = true; u.DeletedByUserId = deletedByUserId; u.DeletedAt = DateTimeOffset.UtcNow; });

                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An error occurred while deleting users with IDs {string.Join(", ", userIds)}");
                    await transaction.RollbackAsync();
                    throw;
                }
            });

            return true;
        }
        public async Task<bool> RestoreUser(List<Guid> userIds)
        {
            if (userIds == null || !userIds.Any())
            {
                throw new ArgumentNullException(nameof(userIds), "User IDs cannot be null or empty.");
            }

            var strategy = db.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await db.Database.BeginTransactionAsync();
                try
                {
                    // Restore Users
                    var usersToUpdate = await db.Users
                        .IgnoreQueryFilters()
                        .Where(u => userIds.Contains(u.Id) && u.IsDeleted)
                        .ToListAsync();
                    if (!usersToUpdate.Any())
                    {
                        throw new KeyNotFoundException("No deleted users found with the provided IDs.");
                    }
                    usersToUpdate.ForEach(u => { u.IsDeleted = false; u.DeletedByUserId = null; u.DeletedAt = null; });
                    await db.SaveChangesAsync();
                    // Restore Items
                    var itemsToUpdate = await db.Items
                        .IgnoreQueryFilters()
                        .Where(item => userIds.Contains(item.CreatedByUserId) && item.IsDeleted)
                        .ToListAsync();
                    itemsToUpdate.ForEach(item => { item.IsDeleted = false; item.DeletedByUserId = null; item.DeletedAt = null; });
                    await db.SaveChangesAsync();
                    // Get item IDs for later queries
                    var itemIds = itemsToUpdate.Select(item => item.Id).ToList();

                    // Restore Offers
                    var deletedOffersRelatedToUser = await db.Offers
                        .Include(o => o.SourceItem).ThenInclude(i => i.CreatedByUser)
                        .Include(o => o.TargetItem).ThenInclude(i => i.CreatedByUser)
                        .IgnoreQueryFilters()
                        .Where(offer => userIds.Contains(offer.CreatedByUserId) || itemIds.Contains(offer.TargetItemId))
                        .ToListAsync();
                    // Find offers that were initiated by other users so that we can check if there is deleted enityt in heirarchy 
                    var offersToUpdate = new List<Database.Schema.Offer>();
                    foreach (var deletedOffer in deletedOffersRelatedToUser)
                    {
                        if (userIds.Contains(deletedOffer.CreatedByUserId))
                        {
                            if (deletedOffer.TargetItem.IsDeleted) continue;
                        }
                        else
                        {
                            if (deletedOffer.SourceItem.IsDeleted) continue;
                        }
                        offersToUpdate.Add(deletedOffer);
                    }
                    if (offersToUpdate.Count > 0)
                    {
                        offersToUpdate.ForEach(offer => { offer.IsDeleted = false; offer.DeletedByUserId = null; offer.DeletedAt = null; });
                        await db.SaveChangesAsync();

                        // Restore Messages linked to the Offers
                        var offerIds = offersToUpdate.Select(offer => offer.Id).ToList();
                        var messagesToUpdate = await db.Messages
                            .IgnoreQueryFilters()
                            .Where(message => offerIds.Contains(message.OfferId) && message.IsDeleted)
                            .ToListAsync();
                        messagesToUpdate.ForEach(message => { message.IsDeleted = false; message.DeletedByUserId = null; message.DeletedAt = null; });
                        await db.SaveChangesAsync();
                    }
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An error occurred while restoring users with IDs {string.Join(", ", userIds)}");
                    await transaction.RollbackAsync();
                    throw;
                }
            });

            return true;
        }
        public async Task<bool> NotifyMe(Guid? id, bool NewMatchingNotification, bool NewCashOfferNotification, bool CashOfferAcceptedNotification)
        {
            try
            {
                if (id == null) return false;

                var userFCMToken = db.Users
                .Where(x => x.Id == id)
                .Select(x => x.FCMToken).FirstOrDefault();
                if (string.IsNullOrEmpty(userFCMToken)) throw new InfrastructureException($"No FCM Token exists for this user");

                var app = FirebaseApp.DefaultInstance;
                var messaging = FirebaseMessaging.GetMessaging(app);
                string response = "";
                if (NewMatchingNotification)
                {
                    var offer = db.Offers.Include(o => o.SourceItem).Include(o => o.TargetItem).Where(x => (x.CreatedByUserId == id || x.TargetItem.CreatedByUserId == id) && x.SourceItemId != x.TargetItemId).FirstOrDefault();
                    if (offer == null) throw new InfrastructureException($"No Matching Offer is created by this user.....");
                    var message = new Message()
                    {
                        Token = userFCMToken,
                        Notification = new Notification
                        {
                            Title = "Product Matched",
                            Body = "One of your product is matched"
                            // Other notification parameters can be added here
                        },
                        Data = new Dictionary<string, string>
                            {
                                {"NavigateTo", "NewMatchingOffer"},
                                {"IsMatch", "true"},
                                {"SourceItemId", offer.SourceItemId.ToString()},
                                {"SourceItemImage", offer.SourceItem.MainImageUrl},
                                {"TargetItemId", offer.TargetItemId.ToString()},
                                {"TargetItemImage", offer.TargetItem.MainImageUrl}
                            }
                    };

                    response = await messaging.SendAsync(message);
                }
                else if (NewCashOfferNotification)
                {
                    var offer = db.Offers.Include(o => o.SourceItem).Include(o => o.TargetItem).Where(x => x.TargetItem.CreatedByUserId == id && x.Cash > 0).FirstOrDefault();
                    if (offer == null) throw new InfrastructureException($"No Cash Offer received by this user.....");
                    var message = new FirebaseAdmin.Messaging.Message()
                    {
                        Token = userFCMToken,
                        Notification = new Notification
                        {
                            Title = "New Cash Offer",
                            Body = "You have a new cash offer",

                            // Other notification parameters can be added here
                        },
                        Data = new Dictionary<string, string>
                        {
                            {"NavigateTo", "NewCashOffer"},
                            {"TargetItemId", offer.TargetItemId.ToString()}
                        }
                    };

                    response = await messaging.SendAsync(message);
                }
                else if (CashOfferAcceptedNotification)
                {
                    var offer = db.Offers.Include(o => o.SourceItem).Include(o => o.TargetItem).Where(x => x.CreatedByUserId == id && x.Cash > 0 && x.SourceStatus == x.TargetStatus).FirstOrDefault();
                    if (offer == null) throw new InfrastructureException($"No Cash Offer received by this user.....");
                    var newDummyMessage = new Domain.Offers.Message(
                         Guid.NewGuid(),
                             offer.CreatedByUserId,
                             offer.Id,
                             offer.Cash,
                             offer.CreatedByUserId,
                             "",
                             null,
                             offer.CreatedAt,
                             false
                         );
                    string newDummyMessageJson = System.Text.Json.JsonSerializer.Serialize(newDummyMessage);
                    var message = new FirebaseAdmin.Messaging.Message()
                    {
                        Token = userFCMToken,
                        Notification = new Notification
                        {
                            Title = "Offer Accpeted",
                            Body = "One of your offer is accepted"
                            // Other notification parameters can be added here
                        },
                        Data = new Dictionary<string, string>
                        {
                            {"NavigateTo", "OfferAccepted"},
                            {"TargetItemId", offer.TargetItemId.ToString()},
                            {"ChatListingStrigifiedObject", newDummyMessageJson}
                        }
                    };

                    response = await messaging.SendAsync(message);
                }
                if (response != null) return true;
                return false;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException($"No request sent to you {ex.Message}");
            }
        }
        public async Task<bool> CheckIfUserByEmail(string email)
        {
            return await db.Users
                           .IgnoreQueryFilters()
                           .AsNoTracking()
                           .AnyAsync(user => user.Email == email);
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var credential = GoogleCredential.FromFile("radvix-push-notification-firebase-adminsdk-k4y1u-2e9407127d.json")
                .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");

            var token = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
            return token;
        }


        //public async Task SendFireBaseNotification(string title, string message, List<string> deviceTokens, string eventType, string eventId)
        //{
        //    try
        //    {
        //        var accessToken = await GetAccessTokenAsync();
        //        var url = "https://fcm.googleapis.com/v1/projects/radvix-push-notification/messages:send";

        //        foreach (var token in deviceTokens)
        //        {
        //            var payload = new
        //            {
        //                message = new
        //                {
        //                    notification = new
        //                    {
        //                        title = title,
        //                        body = message
        //                    },
        //                    data = new Dictionary<string, string>
        //                {
        //                    { "eventType", eventType },
        //                    { "eventId", eventId }
        //                },
        //                    token = token
        //                }
        //            };

        //            var jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
        //            var request = new HttpRequestMessage(HttpMethod.Post, url)
        //            {
        //                Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
        //            };
        //            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        //            HttpClient httpClient = new HttpClient();
        //            var response = await httpClient.SendAsync(request);

        //            if (response.IsSuccessStatusCode)
        //            {
        //                Console.WriteLine($"Notification sent successfully to token: {token}");
        //            }
        //            else
        //            {
        //                var error = await response.Content.ReadAsStringAsync();
        //                Console.WriteLine($"Error sending notification to token: {token}. Error: {error}");
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine($"General error: {e.Message}");
        //    }
        //}

        public async Task SendFireBaseNotification(string title, string message, List<string> deviceTokens, string eventType, string eventId)
        {
            try
            {
                var accessToken = await GetAccessTokenAsync();
                var url = "https://fcm.googleapis.com/v1/projects/radvix-push-notification/messages:send";

                var tasks = new List<Task>();

                foreach (var token in deviceTokens)
                {
                    var payload = new
                    {
                        message = new
                        {
                            notification = new
                            {
                                title = title,
                                body = message
                            },
                            data = new Dictionary<string, string>
                        {
                            { "eventType", eventType },
                            { "eventId", eventId }
                        },
                            token = token
                        }
                    };

                    var jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
                    var request = new HttpRequestMessage(HttpMethod.Post, url)
                    {
                        Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
                    };
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    HttpClient httpClient = new HttpClient();
                    tasks.Add(httpClient.SendAsync(request).ContinueWith(responseTask =>
                    {
                        var response = responseTask.Result;
                        if (response.IsSuccessStatusCode)
                        {
                            Console.WriteLine($"Notification sent successfully to token: {token}");
                        }
                        else
                        {
                            var error = response.Content.ReadAsStringAsync().Result;
                            Console.WriteLine($"Error sending notification to token: {token}. Error: {error}");
                        }
                    }));
                }

                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                Console.WriteLine($"General error: {e.Message}");
            }
        }

        public async Task<bool> IsUserActive(string email)
        {
            var user = await db.Users.IgnoreQueryFilters().Where(u => u.Email == email).FirstOrDefaultAsync();
            if (user == null) return true;
            if (user.IsDeleted) return false;
            return true;

        }
    }
}

