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
    public class SystemNotificationRepositoy : ISystemNotificationRepository
    {
        private readonly SwitcherooContext db;
        private readonly IUserRepository userRepository;
        private readonly IDbContextConfigurator _configurator;
        public SystemNotificationRepositoy(SwitcherooContext db, IUserRepository userRepository, IDbContextConfigurator configurator)
        {
            this.db = db;
            this.userRepository = userRepository;
            _configurator = configurator;
        }

        public async Task<Domain.Notifications.SystemNotification> CreateAsync(Domain.Notifications.SystemNotification notification, bool sendNotification = true, Dictionary<string, string> notificationData = null)
        {
            try
            {

                using (var context = new SwitcherooContext(_configurator))
                {
                    var now = DateTime.UtcNow;

                    var newDbNotificaiton = new Database.Schema.SystemNotification(
                        notification.Title,
                        notification.Message,
                        notification.Type,
                        notification.UserId,
                        notification.Data,
                        false,
                        notification.NavigateTo
                    )
                    {
                        CreatedAt = now
                    };
                    await context.SystemNotification.AddAsync(newDbNotificaiton);
                    await context.SaveChangesAsync();
                    //await SendFirebaseNotification(notification);
                    await Task.Delay(1000);
                    var savedNotification = await GetById(newDbNotificaiton.Id, context);
                    if (sendNotification)
                    {
                        Task.Run(() => SendFirebaseNotification(savedNotification, notificationData));
                    }
                    return savedNotification;
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception {ex.InnerException}");
                throw new InfrastructureException($"Exception:{ex.Message}");
            }
        }
        private async Task<bool> SendFirebaseNotification(SystemNotification notification, Dictionary<string, string> notificationData = null)
        {
            try
            {

                var user = await db.Users.Where(u => u.Id == notification.UserId).FirstOrDefaultAsync();
                if (user == null) return false;
                if (string.IsNullOrEmpty(user.FCMToken)) return false;
                var app = FirebaseApp.DefaultInstance;
                var messaging = FirebaseMessaging.GetMessaging(app);
                var data = new Dictionary<string, string>
                                    {
                                        { "NotificationId",notification.Id.ToString() },
                                        { "NavigateTo", notification.NavigateTo},
                                        { "Data", notification.Data ?? ""}
        };
                if (notificationData != null)
                {
                    foreach (var kvp in notificationData)
                    {
                        data[kvp.Key] = kvp.Value;
                    }
                }
                var message = new FirebaseAdmin.Messaging.Message()
                {
                    Token = user.FCMToken,
                    Notification = new Notification
                    {
                        Title = notification.Title,
                        Body = notification.Message
                        // Other notification parameters can be added here
                    },
                    Data = data
                };
                string response = await messaging.SendAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public async Task<bool> MarkAsRead(Guid id)
        {
            var notification = await db.SystemNotification.Where(n => n.Id == id).FirstOrDefaultAsync();
            if (notification == null) throw new InfrastructureException("Invalid notification id!");
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return true;
        }
        public async Task<Domain.Notifications.SystemNotification> GetById(Guid id)
        {
            return await db.SystemNotification
                .Where(n => n.Id == id)
                .Select(Database.Schema.SystemNotification.ToDomain)
                .FirstOrDefaultAsync();
        }
        private async Task<Domain.Notifications.SystemNotification> GetById(Guid id, SwitcherooContext context)
        {
            return await context.SystemNotification
                .Where(n => n.Id == id)
                .Select(Database.Schema.SystemNotification.ToDomain)
                .FirstOrDefaultAsync();
        }
        public async Task<List<Domain.Notifications.SystemNotification>> GetAll()
        {
            return await db.SystemNotification
                .OrderBy(x => x.IsRead)
                .ThenByDescending(x => x.CreatedAt)
                .Select(Database.Schema.SystemNotification.ToDomain)
                .ToListAsync();
        }
        public async Task<List<Domain.Notifications.SystemNotification>> GetUnread()
        {
            return await db.SystemNotification
                .Where(n => n.IsRead == false)
                .OrderBy(x => x.IsRead)
                .ThenByDescending(x => x.CreatedAt)
                .Select(Database.Schema.SystemNotification.ToDomain)
                .ToListAsync();
        }
        public async Task<List<Domain.Notifications.SystemNotification>> GetByUserId(Guid userId)
        {
            return await db.SystemNotification
                .Where(n => n.UserId == userId)
                .OrderBy(x => x.IsRead)
                .ThenByDescending(x => x.CreatedAt)
                .Select(Database.Schema.SystemNotification.ToDomain)
                .ToListAsync();
        }
        public async Task<List<Domain.Notifications.SystemNotification>> GetUnreadByUserId(Guid userId)
        {
            return await db.SystemNotification
                .Where(n => n.IsRead == false)
                .Where(n => n.UserId == userId)
                .OrderBy(x => x.IsRead)
                .ThenByDescending(x => x.CreatedAt)
                .Select(Database.Schema.SystemNotification.ToDomain)
                .ToListAsync();
        }
        public async Task<int> GetUnreadByUserCount(Guid userId)
        {
            return await db.SystemNotification
                .Where(n => n.IsRead == false)
                .Where(n => n.UserId == userId)
                .CountAsync();
        }
        public async Task<int> GetReadByUserCount(Guid userId)
        {
            return await db.SystemNotification
                .Where(n => n.IsRead == true)
                .Where(n => n.UserId == userId)
                .CountAsync();
        }
    }
}
