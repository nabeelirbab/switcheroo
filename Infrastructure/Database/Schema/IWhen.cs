using System;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Database.Schema
{
    public interface IWhen
    {
        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public DateTimeOffset? ArchivedAt { get; set; }
    }
}