using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Domain.Users
{
    public interface IUserRepository
    {
        Task<User> GetById(Guid id);

        Task<User> GetByEmail(string email);

        Task<User> UpdateUserProfileDetails(Guid id, string? blurb, string? avatarUrl);

        Task<User> UpdateUserName(Guid id, string firstName, string lastName);

        Task<User> UpdateUserEmail(Guid id, string email);

        Task<User> UpdateUserMobile(Guid id, string? mobile);

        Task<User> UpdateUserGender(Guid id, string? gender);

        Task<User> UpdateUserDateOfBirth(Guid id, DateTime? dateOfBirth);

        Task<User> UpdateUserDistance(Guid id, int? distance);

        Task<bool> DeleteUser(Guid id);

    }
}
