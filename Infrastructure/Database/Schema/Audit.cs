using System;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Database.Schema
{
    public class Audit : IWhen
    {
        [Required]
        public User CreatedByUser { get; set; } = null!;

        [Required]
        public Guid CreatedByUserId { get; set; }

        [Required]
        public User UpdatedByUser { get; set; } = null!;

        [Required]
        public Guid UpdatedByUserId { get; set; }

        [Required]
        public DateTimeOffset CreatedAt { get; set; }

        [Required]
        public DateTimeOffset UpdatedAt { get; set; }
        
        public DateTimeOffset? ArchivedAt { get; set; }
    }
}
