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

        public async Task<List<Notifications.Model.CustomNotification>> GetNotifications(
            [Service] ICustomNotificationRepository customNotificationRepository)
        {
            var notifications = await customNotificationRepository.GeNotifications();


            return Notifications.Model.CustomNotification.FromDomains(notifications);
        }
    }
}
