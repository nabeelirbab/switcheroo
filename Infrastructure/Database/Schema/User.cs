using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Database.Schema
{
    public class User : IdentityUser<Guid>, IWhen
    {

        public User(string userName, string firstName, string lastName, 
            string? mobile, string? gender, DateTime? dateOfBirth, int? distance, decimal? latitude, decimal? longitude, 
            string? fCMToken, string? blurb, string? avatarUrl, string email, bool isMatchNotificationsEnabled, bool isChatNotificationsEnabled)
        {
            UserName = userName;
            FirstName = firstName;
            LastName = lastName;
            Mobile = mobile;
            Gender = gender;
            DateOfBirth = dateOfBirth;
            Distance = distance;
            Latitude = latitude;
            Longitude = longitude;
            FCMToken = fCMToken;
            Blurb = blurb;
            AvatarUrl = avatarUrl;
            Email = email;
            IsMatchNotificationsEnabled = isMatchNotificationsEnabled;
            IsChatNotificationsEnabled = isChatNotificationsEnabled;
        }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string? Mobile { get; set; }

        public string? Gender { get; set; }

        [Column(TypeName="Date")]
        public DateTime? DateOfBirth { get; set; }

        public int? Distance { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public string? FCMToken { get; set; }

        public string? Blurb { get; set; }

        public string? AvatarUrl { get; set; }

        public bool IsMatchNotificationsEnabled { get; set; }

        public bool IsChatNotificationsEnabled { get; set; }

        [Required]
        public DateTimeOffset CreatedAt { get; set; }

        [Required]
        public DateTimeOffset UpdatedAt { get; set; }

        public DateTimeOffset? ArchivedAt { get; set; }

        public static Expression<Func<User, Domain.Users.User>> ToDomain =>
            user => new Domain.Users.User(
                user.Id,
                user.UserName,
                user.FirstName,
                user.LastName,                
                user.Email,
                user.Mobile,
                user.Gender,
                user.DateOfBirth,
                user.Distance,
                user.Latitude,
                user.Longitude,
                user.FCMToken,
                user.Blurb,
                user.AvatarUrl,
                user.IsMatchNotificationsEnabled,
                user.IsChatNotificationsEnabled
            );
    }
}
