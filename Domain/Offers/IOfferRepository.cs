using Domain.Categories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Offers
{
    public interface IOfferRepository
    {
        Task<IEnumerable<Offer>> GetCreatedOffers(Guid userId);
        Task<IEnumerable<Offer>> GetReceivedOffers(Guid userId);
        Task<int> GetNotificationCount(Guid userId);
        Task<bool> MarkNotificationRead(Guid userId);
        Task<Offer> GetOfferById(Guid userId, Guid offerId);
        Task<Offer> MarkMessagesAsRead(Guid userId, Guid offerId);
        Task<Offer> CreateOffer(Offer offer);
        Task<Offer> CreateCashOffer(Offer offer);
        Task<bool> DeleteOffer(Guid Id, Guid userId);
        Task<bool> AcceptOffer(Guid offerId);
        Task<bool> UnmatchOffer(Guid offerId);
        Task<IEnumerable<Offer>> GetAllOffers(Guid value);
        Task<IEnumerable<Offer>> GetAllOffersByItemId(Guid userId);
    }
}
