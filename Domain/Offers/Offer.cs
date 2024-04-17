using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Offers
{
    public class Offer
    {
        public Offer(Guid? id, Guid sourceItemId,
            Guid targetItemId, int? cash,
            Guid? createdByUserId, Guid? updatedByUserId,
            DateTime createdAt, int sourceStatus, int? targeteStatus, bool? isRead)
        {
            Id = id;
            SourceItemId = sourceItemId;
            TargetItemId = targetItemId;
            Cash = cash;
            CreatedByUserId = createdByUserId;
            UpdatedByUserId = updatedByUserId;
            CreatedAt = createdAt;
            SourceStatus = sourceStatus;
            TargeteStatus = targeteStatus;
            IsRead = isRead;
        }

        [Required]
        public Guid? Id { get; private set; }

        [Required]
        public Guid SourceItemId { get; set; }

        [Required]
        public Guid TargetItemId { get; set; }

        public int? Cash { get; private set; }

        public Guid? CreatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; }

        public Guid? UpdatedByUserId { get; private set; }
        public int SourceStatus { get; set; }
        public int? TargeteStatus { get; set; }

        public bool? IsRead { get; set; }

        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public Guid? DeletedByUserId { get; set; }

        public static Offer CreateNewOffer(Guid sourceItemId, Guid targetItemId, int? cash, Guid createdByUserId, int sourceStatus, int? targeteStatus, bool? isRead)
        {
            return new Offer(null, sourceItemId, targetItemId, cash, createdByUserId, createdByUserId, DateTime.Now, sourceStatus, targeteStatus, isRead);
        }
    }
}
