using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.ContactUs
{
    public class ContactUs
    {
        public ContactUs(Guid? id, string title, string description, string name, string email, Guid? createdByUserId, Guid? updatedByUserId) 
        {
            Id = id;
            Title = title;
            Description = description;
            Name = name;
            Email= email;
            CreatedByUserId = createdByUserId;
            UpdatedByUserId = updatedByUserId;
        }

        [Required]
        public Guid? Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }

        public Guid? CreatedByUserId { get; set; }
        public Guid? UpdatedByUserId { get; set; }

        public static ContactUs CreateNewContactUs(
            string title,
            string description,
            string name,
            string email,
            Guid createdByUserId
        )
        {
            return new ContactUs(
                null,
                title,
                description,
                name,
                email,
                createdByUserId,
                createdByUserId
            );
        }

        public static ContactUs CreateUpdateContactUs(
            Guid id,
            string title,
            string description,
            string name,
            string email,
            Guid updatedByUserId
        )
        {
            return new ContactUs(
                id,
                title,
                description,
                name,
                email,
                updatedByUserId,
                updatedByUserId
            );
        }
    }
}
