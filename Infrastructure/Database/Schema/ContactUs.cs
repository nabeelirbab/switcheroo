using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace Infrastructure.Database.Schema
{
    public class ContactUs : Audit
    {
        public ContactUs(string title, string description, string name, string email) 
        {
            Title = title;
            Description = description;
            Name = name;
            Email = email;
        }

        [Required]
        public Guid Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }

        public void FromDomain(Domain.ContactUs.ContactUs domaincontactus)
        {
            Title = domaincontactus.Title;
            Description = domaincontactus.Description;
        }

        public static Expression<Func<ContactUs, Domain.ContactUs.ContactUs>> ToDomain =>
            complaint => new Domain.ContactUs.ContactUs(
                complaint.Id,
                complaint.Title,
                complaint.Description,
                complaint.Name,
                complaint.Email,
                complaint.CreatedByUserId,
                complaint.UpdatedByUserId
            );

    }
}
