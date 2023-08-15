using System;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Database.Schema
{
    public class UserVerificationCode : Audit
    {
        public UserVerificationCode(string email, string sixDigitCode, string emailConfirmationToken)
        {
            Email = email;
            SixDigitCode = sixDigitCode;
            EmailConfirmationToken = emailConfirmationToken;
        }

        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string SixDigitCode { get; set; }

        [Required]
        public string EmailConfirmationToken { get; set; }
    }
}
