using Domain.Offers;
using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Notifications
{
    public enum NotificationType
    {
        Info,        // Informational notification
        Warning,     // Warning notification
        Alert,       // Alert/critical notification
        Success,     // Success notification
        Error,       // Error or failure notification
        System,      // System-related notification
        Promotional  // Promotional or marketing notification
    }
    public static class MobileNavigateTo
    {
        public const string OfferCompletionConfirmationPage = "ConfirmationOfOfferCompletion";
        public const string OfferCompletionCancellationPage = "OfferCompletionCancellation";
        public const string OfferAcceptedPage = "OfferAccepted";
        public const string NewCashOfferPage = "NewCashOffer";
        public const string NewMatchingOfferPage = "NewMatchingOffer";
    }
    public class SystemNotification
    {
        public SystemNotification(Guid? id, string title, string message, NotificationType type, Guid userId, string? data, bool isRead, string navigateTo)
        {
            Id = id;
            Title = title;
            Message = message;
            Type = type;
            UserId = userId;
            Data = data;
            IsRead = isRead;
            NavigateTo = navigateTo;
        }

        [Required]
        public Guid? Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public Guid UserId { get; set; }
        public string? Data { get; set; }
        public bool IsRead { get; set; }
        public string NavigateTo { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }


        public static SystemNotification CreateNewNotification(
            string title,
            string message,
            NotificationType type,
            Guid userId,
            string data,
            string navigateTo
        )
        {
            return new SystemNotification(
                null,
                title,
                message,
                type,
                userId,
                data,
                false,
                navigateTo
            )
            {
                CreatedAt = DateTime.Now
            };
        }

        public static SystemNotification OfferConfirmationNotification(string sourceItem, string targetItem, bool isCash, int? cash, string username, bool confirmedByOtherParty, Guid userId, string? data)
        {
            string message = "";
            if (!isCash)
            {
                message = confirmedByOtherParty
                    ? $"Offer for '{sourceItem}' against '{targetItem}' has been confirmed by {username}. Prepare to swap your items."
                    : $"Offer for '{sourceItem}' against '{targetItem}' has been confirmed by {username}. Please confirm on your end to proceed with the swap.";
            }
            else
            {
                message = confirmedByOtherParty
                    ? $"Offer of ${cash} for '{targetItem}' has been confirmed by {username}. Prepare to pick your item."
                    : $"Offer of ${cash} for '{targetItem}' has been confirmed by {username}. Please confirm on your end to proceed with the pickup.";
            }

            return new SystemNotification(null,
                    "Offer Completion Confirmation",
                    message,
                    NotificationType.Info,
                    userId,
                    data,
                    false,
                    MobileNavigateTo.OfferCompletionConfirmationPage)
            {
                CreatedAt = DateTime.Now
            };
        }

        public static SystemNotification OfferConfirmationCancellationNotification(string sourceItem, string targetItem, bool isCash, int? cash, string username, Guid userId, string? data)
        {
            string message = "";
            if (!isCash) message = $"Offer for '{sourceItem}' against '{targetItem}', which was previously confirmed by {username}, has now been cancelled. Immediate action is required on your part to prevent the offer from falling through.";
            else message = $"Offer of ${cash} for '{targetItem}', which was previously confirmed by {username}, has now been cancelled. Immediate action is required on your part to prevent the offer from falling through.";
            return new SystemNotification(null,
                    "Offer Completion Cancelled",
                    message,
                    NotificationType.Info,
                    userId,
                    data,
                    false,
                    MobileNavigateTo.OfferCompletionCancellationPage)
            {
                CreatedAt = DateTime.Now
            };
        }

        public static SystemNotification OfferAcceptedNotification(string targetItem, int? cash, string username, Guid userId, string? data)
        {
            string message = $"Your Offer of ${cash} for '{targetItem}', has been aceepted by {username}, please get ready to swap the item.";
            return new SystemNotification(null,
                    "Offer Accepted",
                    message,
                    NotificationType.Info,
                    userId,
                    data,
                    false,
                    MobileNavigateTo.OfferAcceptedPage)
            {
                CreatedAt = DateTime.Now
            };
        }

        public static SystemNotification NewCashOfferNotification(string targetItem, int? cash, Guid userId, string? data)
        {
            string message = $"Your have a new cash cash offer  of ${cash} for '{targetItem}', please accept to swap the item.";
            return new SystemNotification(null,
                    "New Cash Offer",
                    message,
                    NotificationType.Info,
                    userId,
                    data,
                    false,
                    MobileNavigateTo.NewCashOfferPage)
            {
                CreatedAt = DateTime.Now
            };
        }
        public static SystemNotification ItemMatchedNotification(Guid userId, string? data)
        {
            string message = $"One of your product is matched.";
            return new SystemNotification(null,
                    "Product Matched",
                    message,
                    NotificationType.Info,
                    userId,
                    data,
                    false,
                    MobileNavigateTo.NewMatchingOfferPage)
            {
                CreatedAt = DateTime.Now
            };
        }

    }
}
