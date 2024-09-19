using API.HtmlTemplates;
using Domain.Users;
using HotChocolate;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;
using System;
using Domain.Notifications;
using API.GraphQL.CommonServices;

namespace API.GraphQL
{
    public partial class Mutation
    {
        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin" })]
        public async Task<Notifications.Model.CustomNotification> CreateCusotmNotification(
            [Service] UserContextService userContextService,
            [Service] ICustomNotificationRepository customNotificationRepository,
            Domain.Notifications.CustomNotification notification,
            Notifications.Model.CustomNotificationFilters filters
        )
        {
            try
            {
                var requestUserId = userContextService.GetCurrentUserId();
                var newDomainNotification = await customNotificationRepository.CreateNotificationAsync(Domain.Notifications.CustomNotification.CreateNewNotification(
                    notification.Title,
                    notification.Description,
                    Guid.NewGuid()
                ), Domain.Notifications.CustomNotificationFilters.CreateNewNotification(null, filters.GenderFilter, filters.ItemFilter), requestUserId);
                return Notifications.Model.CustomNotification.FromDomain(newDomainNotification);
            }
            catch (Exception ex)
            {
                throw new InfrastructureException($"API Exception {ex}");
            }
        }

        public async Task<bool> MarkSystemNotificationAsRead(
            [Service] ISystemNotificationRepository systemNotificationRepository,
            Guid id)
        {
            return await systemNotificationRepository.MarkAsRead(id);
        }
    }
}
