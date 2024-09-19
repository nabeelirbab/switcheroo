using Domain.Notifications;
using Domain.Users;
using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Notifications
{
    public class CustomNotificationRepositoy : ICustomNotificationRepository
    {
        private readonly SwitcherooContext db;
        private readonly IUserRepository userRepository;
        private readonly ISystemNotificationRepository _systemNotificationRepository;
        public CustomNotificationRepositoy(SwitcherooContext db, IUserRepository userRepository, ISystemNotificationRepository systemNotificationRepository)
        {
            this.db = db;
            this.userRepository = userRepository;
            this._systemNotificationRepository = systemNotificationRepository;
        }

        public async Task<Domain.Notifications.CustomNotification> CreateNotificationAsync(Domain.Notifications.CustomNotification notification, Domain.Notifications.CustomNotificationFilters filters, Guid userId)
        {
            try
            {
                var now = DateTime.UtcNow;

                var newDbNotificaiton = new Database.Schema.CustomNotification(
                    notification.Title,
                    notification.Description
                )
                {
                    CreatedByUserId = userId,
                    UpdatedByUserId = userId,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                await db.CustomNotification.AddAsync(newDbNotificaiton);
                await db.SaveChangesAsync();
                var newDbNotificationFilters = new Database.Schema.CustomNotificationFilters(newDbNotificaiton.Id, filters.GenderFilter ?? "None", filters.ItemFilter ?? "None");
                await db.CustomNotificationFilters.AddAsync(newDbNotificationFilters);
                await db.SaveChangesAsync();


                IQueryable<Database.Schema.User> usersQuery = db.Users.Where(u => !string.IsNullOrWhiteSpace(u.FCMToken));
                if (filters.GenderFilter != null)
                {
                    usersQuery = usersQuery.Where(u => u.Gender == filters.GenderFilter);
                }
                if (filters.ItemFilter != null)
                {
                    switch (filters.ItemFilter)
                    {
                        case "No Items":
                            usersQuery = usersQuery.Where(u => !db.Items.Any(i => i.CreatedByUserId == u.Id));
                            break;
                        case "At Least One Item":
                            usersQuery = usersQuery.Where(u => db.Items.Any(i => i.CreatedByUserId == u.Id));
                            break;
                    }
                }
                List<string> userFcmTokens = await usersQuery.Select(u => u.FCMToken).ToListAsync();
                var filteredUsersList = await db.Users.Where(u => userFcmTokens.Contains(u.FCMToken)).ToListAsync();
                Task.Run(() => InsertCustomNotificationInSystemNotifications(newDbNotificaiton, filteredUsersList));
                if (userFcmTokens.Any())
                {
                    var app = FirebaseApp.DefaultInstance;
                    var messaging = FirebaseMessaging.GetMessaging(app);

                    var message = new FirebaseAdmin.Messaging.MulticastMessage()
                    {
                        Tokens = userFcmTokens,
                        Notification = new Notification
                        {
                            Title = notification.Title,
                            Body = notification.Description
                            // Other notification parameters can be added here
                        }
                    };
                    var response = await messaging.SendMulticastAsync(message);
                    for (int i = 0; i < response.Responses.Count; i++)
                    {
                        var sendResponse = response.Responses[i];
                        var userToken = userFcmTokens[i];

                        var status = new Database.Schema.CustomNotificationStatus(newDbNotificaiton.Id,
                            filteredUsersList.Where(u => u.FCMToken == userToken).FirstOrDefault().Id,
                            sendResponse.IsSuccess);
                        db.CustomNotificationStatus.Add(status);
                    }
                }
                await db.SaveChangesAsync();
                return await GetNotificationById(newDbNotificaiton.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception {ex.InnerException}");
                throw new InfrastructureException($"Exception:{ex.Message}");
            }
        }

        private async Task<bool> InsertCustomNotificationInSystemNotifications(Database.Schema.CustomNotification customNotification, List<Database.Schema.User> users)
        {
            foreach (var user in users)
            {
                var systemNotification = SystemNotification.CreateNewNotification(customNotification.Title, customNotification.Description, NotificationType.Promotional, user.Id, "", "");
                await _systemNotificationRepository.CreateAsync(systemNotification, false);
            }
            return true;
        }
        public async Task<List<Domain.Notifications.CustomNotification>> GeNotifications()
        {
            return await db.CustomNotification
                .Select(Database.Schema.CustomNotification.ToDomain)
                .ToListAsync();
        }

        public async Task<Domain.Notifications.CustomNotification> GetNotificationById(Guid notificationId)
        {
            var item = await db.CustomNotification
                .Where(z => z.Id == notificationId)
                .Select(Database.Schema.CustomNotification.ToDomain)
                .FirstOrDefaultAsync();

            if (item == null) throw new InfrastructureException($"Unable to locate notificaitonId");

            return item;
        }

        public async Task<List<Domain.Notifications.CustomNotificationStatus>> GetDeliveryStatus(Guid notificationId)
        {
            var statuses = await db.CustomNotificationStatus.Where(s => s.NotificationId == notificationId).ToListAsync();
            if (statuses == null || statuses.Count == 0)
            {
                return null;
            }
            var userIds = statuses.Select(s => s.UserId).ToList();
            var usersList = await db.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();
            foreach (var status in statuses)
            {
                status.UserEmail = usersList.Where(u => u.Id == status.UserId).Select(u => u.Email).FirstOrDefault();
                status.UserName = usersList.Where(u => u.Id == status.UserId).Select(u => u.FirstName).FirstOrDefault();
            }
            return Database.Schema.CustomNotificationStatus.ToDomains(statuses);
        }
    }
}
