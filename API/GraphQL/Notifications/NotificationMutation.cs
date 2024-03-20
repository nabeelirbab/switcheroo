using API.HtmlTemplates;
using Domain.Users;
using HotChocolate;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;
using System;
using Domain.Notifications;

namespace API.GraphQL
{
    public partial class Mutation
    {
        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin" })]
        public async Task<Notifications.Model.CustomNotification> CreateCusotmNotification(
            [Service] ICustomNotificationRepository customNotificationRepository,
            Domain.Notifications.CustomNotification notification,
            Notifications.Model.CustomNotificationFilters filters
        )
        {
            try
            {
                var newDomainNotification = await customNotificationRepository.CreateNotificationAsync(Domain.Notifications.CustomNotification.CreateNewNotification(
                    notification.Title,
                    notification.Description,
                    Guid.NewGuid()
                ), Domain.Notifications.CustomNotificationFilters.CreateNewNotification(null, filters.GenderFilter, filters.ItemFilter));
                return Notifications.Model.CustomNotification.FromDomain(newDomainNotification);
            }
            catch (Exception ex)
            {
                throw new InfrastructureException($"API Exception {ex}");
            }
        }
    }
}
