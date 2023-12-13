using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Email
{
    public class SmtpOptions
    {
        [Required]
        public string SMTP_HOST { get; set; } = null!;

        [Required]
        public string SMTP_PORT { get; set; } = null!;

        [Required]
        public string SMTP_FROM_ADDRESS { get; set; } = null!;
        
        // This is used for SMPT4DEV
        public string? SMTP_UI_PORT { get; set; }

        public string SMTP_FROM_SUPPORT_ADDRESS { get; set; } = null!;

        [Required]
        public string EMAIL_API_KEY { get; set; } = null!;

        [Required]
        public string SMTP_PASSWORD { get; set; } = null!;
    }
}
