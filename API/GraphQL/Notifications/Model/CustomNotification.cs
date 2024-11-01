﻿using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using Infrastructure.Notifications;
using Domain.Notifications;

namespace API.GraphQL.Notifications.Model
{
    public class CustomNotification
    {
        public CustomNotification(Guid? id, string title, string description, Guid? createdByUserId, Guid? updatedByUserId)
        {
            Id = id;
            Title = title;
            Description = description;
            CreatedByUserId = createdByUserId;
            UpdatedByUserId = updatedByUserId;
        }

        [Required]
        public Guid? Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Guid? CreatedByUserId { get; set; }
        public Guid? UpdatedByUserId { get; set; }

        public async Task<List<CustomNotificationStatus>> DeliveryStatus([Service] ICustomNotificationRepository customNotifications)
        {
            var statuses = await customNotifications.GetDeliveryStatus(Id.Value);
            if (statuses != null)
            {
                return CustomNotificationStatus.FromDomains(statuses);

            }
            else return new List<CustomNotificationStatus>();
        }

        public static CustomNotification FromDomain(Domain.Notifications.CustomNotification domainNotification)
        {
            if (!domainNotification.Id.HasValue) throw new ApiException("Mapping error. Id missing");

            return new CustomNotification(
                domainNotification.Id.Value,
                domainNotification.Title,
                domainNotification.Description,
                domainNotification.CreatedByUserId.Value,
                domainNotification.UpdatedByUserId.Value
                )
            {
                CreatedAt = domainNotification.CreatedAt,
            };
        }
        public static List<CustomNotification> FromDomains(List<Domain.Notifications.CustomNotification> domainNotifications)
        {
            if (domainNotifications == null || domainNotifications.Count == 0)
            {
                // Handle the case where there are no messages if needed.
                return new List<CustomNotification>();
            }
            return domainNotifications.Select(newNotification => new CustomNotification(
                newNotification.Id,
                newNotification.Title,
                newNotification.Description,
                newNotification.CreatedByUserId,
                newNotification.UpdatedByUserId)
            {
                CreatedAt = newNotification.CreatedAt
            })
                .ToList();
        }


    }
}
