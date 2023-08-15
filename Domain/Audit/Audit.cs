// using System;
// using System.ComponentModel.DataAnnotations;
// using Domain.Users;

// namespace Domain.Audit
// {
//     public abstract class Audit : AuditWhen
//     {
//         // protected Audit(User createdByUser, User updatedByUser,
//         //     DateTimeOffset createdAt, DateTimeOffset updatedAt, DateTimeOffset? archivedAt = null) 
//         //     : base(createdAt, updatedAt, archivedAt)
//         // {
//         //     CreatedByUser = createdByUser;
//         //     UpdatedByUser = updatedByUser;
//         // }

//         [Required]
//         public User CreatedByUser { get; private set; } = null!;

//         [Required]
//         public Guid CreatedByUserId { get; private set; }

//         [Required]
//         public User UpdatedByUser { get; private set; } = null!;

//         [Required]
//         public Guid UpdatedByUserId { get; private set; }
//     }
// }
