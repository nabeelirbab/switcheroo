using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Database.Schema
{
    public class Message : Audit
    {
        public Message(Guid offerId, string messageText, DateTime? messageReadAt)
        {
            OfferId = offerId;
            MessageText = messageText;
            MessageReadAt = messageReadAt;
        }

        [Required]
        public Guid Id { get; set; }

        [Required]
        public Offer Offer { get; private set; } = null!;

        [Required]
        public Guid OfferId { get; private set; }
        public int? Cash { get; set; }

        public Guid? UserId { get; set; }

        [Required]
        public string MessageText { get; private set; }

        public DateTime? MessageReadAt { get; set; }

        public bool? IsRead { get; set; }




        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public Guid? DeletedByUserId { get; set; }

        [ForeignKey("DeletedByUserId")]
        public User? DeletedByUser { get; set; }


    }
}