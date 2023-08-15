// using System;
// using System.ComponentModel.DataAnnotations;

// namespace Domain.Audit
// {
//     public abstract class AuditWhen
//     {
//         // protected AuditWhen() {}
//         // protected AuditWhen(DateTimeOffset createdAt, DateTimeOffset updatedAt, DateTimeOffset? archivedAt = null)
//         // {
//         //     CreatedAt = createdAt;
//         //     UpdatedAt = updatedAt;
//         //     ArchivedAt = archivedAt;
//         // }

//         [Required]
//         public DateTimeOffset CreatedAt { get; private set; }

//         [Required]
//         public DateTimeOffset UpdatedAt { get; private set; }

//         public DateTimeOffset? ArchivedAt { get; private set; }
//     }
// }
