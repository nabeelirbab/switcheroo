using API.GraphQL.CommonServices;
using API.GraphQL.ContactUs.Model;
using Domain.Complaints;
using Domain.Notifications;
using GraphQL;
using HotChocolate;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.GraphQL
{
    public partial class Query
    {
        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin" })]
        public async Task<List<Notifications.Model.CustomNotification>> GetNotifications(
            [Service] ICustomNotificationRepository customNotificationRepository)
        {
            var notifications = await customNotificationRepository.GeNotifications();
            return Notifications.Model.CustomNotification.FromDomains(notifications);
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin" })]
        public async Task<List<Notifications.Model.SystemNotification>> GetAllSystemNotifications(
            [Service] ISystemNotificationRepository notificationRepository)
        {
            var notifications = await notificationRepository.GetAll();
            return Notifications.Model.SystemNotification.FromDomains(notifications);
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin" })]
        public async Task<List<Notifications.Model.SystemNotification>> GetAllUnReadSystemNotifications(
            [Service] ISystemNotificationRepository notificationRepository)
        {
            var notifications = await notificationRepository.GetUnread();
            return Notifications.Model.SystemNotification.FromDomains(notifications);
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<List<Notifications.Model.SystemNotification>> GetUserSystemNotifications(
            [Service] UserContextService userContextService,
            [Service] ISystemNotificationRepository notificationRepository)
        {
            var requestUserId = userContextService.GetCurrentUserId();
            var notifications = await notificationRepository.GetByUserId(requestUserId);
            return Notifications.Model.SystemNotification.FromDomains(notifications);
        }
        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<List<Notifications.Model.SystemNotification>> GetUserUnReadSystemNotifications(
            [Service] UserContextService userContextService,
            [Service] ISystemNotificationRepository notificationRepository)
        {
            var requestUserId = userContextService.GetCurrentUserId();
            var notifications = await notificationRepository.GetUnreadByUserId(requestUserId);
            return Notifications.Model.SystemNotification.FromDomains(notifications);
        }
    }
}
