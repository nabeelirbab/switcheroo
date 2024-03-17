using Domain.Notifications;
using Domain.Services;
using Domain.Users;
using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Notifications
{
    public class CustomNotificationRepositoy : ICustomNotificationRepository
    {
        private readonly SwitcherooContext db;
        private readonly IUserRepository userRepository;
        public CustomNotificationRepositoy(SwitcherooContext db, IUserRepository userRepository)
        {
            this.db = db;
            this.userRepository = userRepository;
        }

        public async Task<Domain.Notifications.CustomNotification> CreateNotificationAsync(Domain.Notifications.CustomNotification notification)
        {
            try
            {
                var now = DateTime.UtcNow;

                var newDbNotificaiton = new Database.Schema.CustomNotification(
                    notification.Title,
                    notification.Description
                )
                {
                    CreatedByUserId = new Guid("f101ee70-8879-4f24-ac1a-eaeb36d4715c"),
                    UpdatedByUserId = new Guid("f101ee70-8879-4f24-ac1a-eaeb36d4715c"),
                    CreatedAt = now,
                    UpdatedAt = now
                };
                await db.CustomNotification.AddAsync(newDbNotificaiton);
                await db.SaveChangesAsync();

                var usersList = await db.Users.Where(u => !string.IsNullOrWhiteSpace(u.FCMToken)).Select(u => u.FCMToken).ToListAsync();
                if (usersList.Any())
                {
                    var app = FirebaseApp.DefaultInstance;
                    var messaging = FirebaseMessaging.GetMessaging(app);

                    var message = new FirebaseAdmin.Messaging.MulticastMessage()
                    {
                        Tokens = usersList,
                        Notification = new Notification
                        {
                            Title = notification.Title,
                            Body = notification.Description
                            // Other notification parameters can be added here
                        }
                    };
                    var response = await messaging.SendMulticastAsync(message);
                }
                return await GetNotificationById(newDbNotificaiton.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception {ex.InnerException}");
                throw new InfrastructureException($"Exception:{ex.Message}");
            }
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
    }
}
