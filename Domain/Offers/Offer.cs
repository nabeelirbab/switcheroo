using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Offers
{
    public class Offer
    {
        public Offer(Guid? id, Guid sourceItemId,
            Guid targetItemId, int? cash,
            Guid? createdByUserId, Guid? updatedByUserId, DateTime createdAt, int sourceStatus,int? targeteStatus)
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

        public static Offer CreateNewOffer(Guid sourceItemId, Guid targetItemId,int? cash, Guid createdByUserId, int sourceStatus, int? targeteStatus)
        {
            return new Offer(null, sourceItemId, targetItemId, cash, createdByUserId, createdByUserId, DateTime.Now, sourceStatus, targeteStatus);
        }
    }
}
