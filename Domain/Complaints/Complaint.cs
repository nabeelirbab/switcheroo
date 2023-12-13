using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Complaints
{
    public class Complaint
    {
        public Complaint(Guid? id, string title, string description, bool? isSolved, Guid? createdByUserId, Guid? updatedByUserId)
        {
            Id = id;
            Title = title;
            Description = description;
            IsSolved = isSolved;
            CreatedByUserId = createdByUserId;
            UpdatedByUserId = updatedByUserId;
        }

        [Required]
        public Guid? Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        public bool? IsSolved { get; set; }
        public Guid? CreatedByUserId { get; private set; }
        public Guid? UpdatedByUserId { get; private set; }

        public static Complaint CreateNewComplaint(
            string title,
            string description,
            Guid createdByUserId
        )
        {
            return new Complaint(
                null,
                title,
                description,
                false,
                createdByUserId,
                createdByUserId
            );
        }

        public static Complaint CreateUpdateComplaint(
            Guid id,
            string title,
            string description,
            bool isSolved,
            Guid updatedByUserId
        )
        {
            return new Complaint(
                id,
                title,
                description,
                isSolved,
                updatedByUserId,
                updatedByUserId
            );
        }
    }
}
