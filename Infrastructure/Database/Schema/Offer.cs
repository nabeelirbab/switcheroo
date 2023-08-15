using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Database.Schema
{
    public class Offer : Audit
    {
        public Offer(Guid sourceItemId, Guid targetItemId)
        {
            SourceItemId = sourceItemId;
            TargetItemId = targetItemId;
        }
        
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Item SourceItem { get; private set; } = null!;

        [Required]
        public Guid SourceItemId { get; private set; }

        [Required]
        public Item TargetItem { get; set; } = null!;

        [Required]
        public Guid TargetItemId { get; set; }

        [Required]
        public OfferStatus SourceStatus {get; set;}

        [Required]
        public OfferStatus TargetStatus {get; set;}

        public int? Cash { get; private set; }

        public List<Message> Messages { get; private set; } = new List<Message>();
    }
}
