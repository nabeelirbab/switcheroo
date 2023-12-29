using System.ComponentModel.DataAnnotations;
using System;

namespace API.GraphQL.ContactUs.Model
{
    public class ContactUs
    {
        public ContactUs(Guid? id, string title, string description, string name, string email, Guid? createdByUserId, Guid? updatedByUserId) 
        {
            Id = id;
            Title = title;
            Description = description;
            Name = name;
            Email = email;
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

        public static ContactUs FromDomain(Domain.ContactUs.ContactUs domcontactus)
        {
            if (!domcontactus.Id.HasValue) throw new ApiException("Mapping error. Id missing");
            if (!domcontactus.CreatedByUserId.HasValue) throw new ApiException("Mapping error. CreatedByUserId missing");
            if (!domcontactus.UpdatedByUserId.HasValue) throw new ApiException("Mapping error. UpdatedByUserId missing");

            return new ContactUs(
                domcontactus.Id.Value,
                domcontactus.Title,
                domcontactus.Description,
                domcontactus.Name,
                domcontactus.Email,
                domcontactus.CreatedByUserId.Value,
                domcontactus.UpdatedByUserId.Value
                );
        }
    }
}
