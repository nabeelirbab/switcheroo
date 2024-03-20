using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace API.GraphQL.Notifications.Model
{
    public class CustomNotificationFilters
    {
        public CustomNotificationFilters(string? genderFilter, string? itemFilter)
        {
            GenderFilter = genderFilter;
            ItemFilter = itemFilter;
        }

        public string? GenderFilter { get; set; }
        public string? ItemFilter { get; set; }

        public static CustomNotificationFilters FromDomain(Domain.Notifications.CustomNotificationFilters domainFilters)
        {
            if (!domainFilters.Id.HasValue) throw new ApiException("Mapping error. Id missing");
            if (!domainFilters.CustomNotificationId.HasValue) throw new ApiException("Mapping error. Custom Notification Id missing");

            return new CustomNotificationFilters(
                domainFilters.GenderFilter,
                domainFilters.ItemFilter
                );
        }
    }
}
