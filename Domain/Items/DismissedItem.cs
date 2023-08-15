using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Items
{
    public class DismissedItem
    {
        public DismissedItem(Guid? id, Guid? sourceItemId, Guid targetItemId, Guid? createdByUserId, Guid? updatedByUserId)
        {
            Id = id;
            SourceItemId = sourceItemId;
            TargetItemId = targetItemId;
            CreatedByUserId = createdByUserId;
            UpdatedByUserId = updatedByUserId;
        }

        [Required]
        public Guid? Id { get; private set; }

        public Guid? SourceItemId { get; private set; }

        [Required]
        public Guid TargetItemId { get; private set; }

        public Guid? CreatedByUserId { get; private set; }

        public Guid? UpdatedByUserId { get; private set; }

        public static DismissedItem CreateDismissItemForItem(Guid sourceItemId, Guid targetItemId, Guid createdByUserId)
        {
            return new DismissedItem(null, sourceItemId, targetItemId, createdByUserId, createdByUserId);
        }

        public static DismissedItem CreateDismissItem(Guid targetItemId, Guid createdByUserId)
        {
            return new DismissedItem(null, null, targetItemId, createdByUserId, createdByUserId);
        }
    }
}
