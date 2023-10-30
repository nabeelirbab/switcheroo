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
        Task<Offer> GetOfferById(Guid userId, Guid offerId);
        Task<Offer> MarkMessagesAsRead(Guid userId, Guid offerId);
        Task<Offer> CreateOffer(Offer offer);
        Task<bool> DeleteOffer(Guid Id);
        Task<IEnumerable<Offer>> GetAllOffers(Guid value);
        Task<IEnumerable<Offer>> GetAllOffersByItemId(Guid userId);
    }
}
