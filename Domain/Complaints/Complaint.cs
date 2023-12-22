using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Complaints
{
    public class Complaint
    {
        public Complaint(Guid? id, string title, string description, bool isSolved, Guid? createdByUserId, Guid? updatedByUserId, Guid? targetUserId, Guid? targetItemId)
        {
            Id = id;
            Title = title;
            Description = description;
            IsSolved = isSolved;
            CreatedByUserId = createdByUserId;
            UpdatedByUserId = updatedByUserId;
            TargetUserId = targetUserId;
            TargetItemId = targetItemId;
        }

        public Complaint(Guid? id, string title, string description, bool isSolved, Guid? createdByUserId, Guid? targetUserId, Guid? targetItemId)
        {
            Id = id;
            Title = title;
            Description = description;
            IsSolved = isSolved;
            CreatedByUserId = createdByUserId;
            TargetUserId = targetUserId;
            TargetItemId = targetItemId;
        }

        [Required]
        public Guid? Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        public bool IsSolved { get; set; }
        public Guid? CreatedByUserId { get; set; }
        public Guid? UpdatedByUserId { get; set; }
        public Guid? TargetUserId { get; set; }
        public Guid? TargetItemId { get; set; }

        public static Complaint CreateNewComplaint(
            string title,
            string description,
            Guid createdByUserId,
            Guid? targetUserId,
            Guid? targetItemId
        )
        {
            return new Complaint(
                null,
                title,
                description,
                false,
                createdByUserId,
                createdByUserId,
                targetUserId,
                targetItemId
            );
        }

        public static Complaint CreateUpdateComplaint(
            Guid id,
            string title,
            string description,
            bool isSolved,
            Guid updatedByUserId,
            Guid? targetUserId,
            Guid? targetItemId
        )
        {
            return new Complaint(
                id,
                title,
                description,
                isSolved,
                updatedByUserId,
                updatedByUserId,
                targetUserId,
                targetItemId
            );
        }
    }
}
