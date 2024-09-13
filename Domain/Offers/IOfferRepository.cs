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
        Task<Offer> GetOfferById(Guid offerId);
        Task<Offer> GetOfferByOfferId(Guid offerId);
        Task<Offer> MarkMessagesAsRead(Guid userId, Guid offerId);
        Task<Offer> CreateOffer(Offer offer);
        Task<bool> DeleteOffer(Guid Id, Guid userId);
        Task<bool> RestoreOffer(Guid offerId);
        Task<bool> AcceptOffer(Guid offerId);
        Task<bool> UnmatchOffer(Guid offerId);
        Task<IEnumerable<Offer>> GetAllOffers(Guid value);
        Task<IEnumerable<Offer>> GetAllOffersByItemId(Guid userId);

        Task<Paginated<Offer>> GetAllMatchedOffers(int limit, string? cursor);
        Task<Paginated<Offer>> GetAllConfirmedOffers(int limit, string? cursor);
        Task<Paginated<Offer>> GetAllOffersConfirmedByOneParty(int limit, string? cursor);
        Task<Paginated<Offer>> GetAllPendingMatchingOffers(int limit, string? cursor);
        Task<Paginated<Offer>> GetAllAcceptedCashOffers(int limit, string? cursor);
        Task<Paginated<Offer>> GetAllPendingCashOffers(int limit, string? cursor);
        Task<Tuple<int, int>> GetTodayAndYesturdaySwipesInfo(Guid userId);
        Task<int> GetSwipesInfo(Guid userId);
        Task<int> GetYesturdaySwipesInfo(Guid userId);
        Task<List<Offer>> GetMatchedOffers();

        Task<Offer> ConfirmOffer(Guid offerId,Guid userId);


    }
}
