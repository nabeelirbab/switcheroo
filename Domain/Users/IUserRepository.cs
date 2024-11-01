using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Users
{
    public interface IUserRepository
    {
        Task<User> GetById(Guid? id);

        Task<List<User>> GetUserByUserId(List<Guid> userIds);

        Task<Paginated<User>> GetAllUsers(int limit, string? cursor);
        Task<bool> NotifyMe(Guid? userId, bool NewMatchingNotification, bool NewCashOfferNotification, bool CashOfferAcceptedNotification = false);
        Task SendFireBaseNotification(string title, string message, List<string> DeviceTokens, string eventType, string eventId);

        Task<List<User>> GetUserById(Guid? userId);
        Task<List<User>> GetTargetUser(Guid? userId, Guid offerId);

        Task<User> GetByEmail(string email);

        Task<List<KeyValue>> GetUsersGenderCount();

        Task<User> UpdateUserProfileDetails(Guid id, string? blurb, string? avatarUrl);

        Task<User> UpdateUserName(Guid id, string firstName, string lastName);

        Task<User> UpdateUserEmail(Guid id, string email);

        Task<User> UpdateUserMobile(Guid id, string? mobile);

        Task<User> UpdateUserGender(Guid id, string? gender);

        Task<User> UpdateUserDateOfBirth(Guid id, DateTime? dateOfBirth);

        Task<User> UpdateUserDistance(Guid id, int? distance);

        Task<User> UpdateUserLocation(Guid id, decimal? latitude, decimal? longitude);

        Task<User> UpdateUserFCMToken(Guid id, string? fcmToken);

        Task<bool> DeleteUser(List<Guid> ids, Guid deletedByUserId);
        Task<bool> DeleteUserPermanently(List<Guid> ids);
        Task<bool> RestoreUser(List<Guid> userIds);

        Task<bool> CheckIfUserByEmail(string email);

        Task<string> GetTargetUserForMessage(Guid? userId, Guid offerId, bool getFCMToken = false);

        Task<bool> IsUserActive(string email);

    }
}
