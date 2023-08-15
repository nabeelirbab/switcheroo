using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Users
{
    public class User
    {
        public User(Guid? id, string username, string firstName, string lastName, string email,
            string? mobile, string? gender, DateTime? dateOfBirth, int? distance,
            string? blurb, string? avatarUrl, bool isMatchNotificationsEnabled,
            bool isChatNotificationsEnabled)
        {
            Id = id;
            Username = username;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Mobile = mobile;
            Gender = gender;
            DateOfBirth = dateOfBirth;
            Distance = distance;
            Blurb = blurb;
            AvatarUrl = avatarUrl;
            IsMatchNotificationsEnabled = isMatchNotificationsEnabled;
            IsChatNotificationsEnabled = isChatNotificationsEnabled;
        }

        public Guid? Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        public string? Mobile { get; set; }

        public string? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public int? Distance { get; set; }

        public string? Blurb { get; set; }

        public string? AvatarUrl { get; set; }

        public bool IsMatchNotificationsEnabled { get; set; }

        public bool IsChatNotificationsEnabled { get; set; }

        public static User CreateNewUser(string firstName, string lastName, string email)
        {
            return new User(
                null, email, firstName, lastName, email, 
                null, null, null, null, 
                null, null, true, true);
        }

        public static string GenerateSixDigitVerificationCode()
        {
            var rand = new Random();
            return rand.Next(0, 999999).ToString("D6");
        }
    }
}
