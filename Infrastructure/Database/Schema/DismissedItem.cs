using System;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Database.Schema
{
    public class DismissedItem : Audit
    {
        public DismissedItem(Guid? sourceItemId, Guid targetItemId)
        {
            TargetItemId = targetItemId;
            SourceItemId = sourceItemId;
        }

        [Required]
        public Guid Id { get; set; }

        public Item? SourceItem { get; private set; }

        [Required]
        public Guid? SourceItemId { get; private set; }

        [Required]
        public Item TargetItem { get; private set; } = null!;

        [Required]
        public Guid TargetItemId { get; private set; }
    }
}
