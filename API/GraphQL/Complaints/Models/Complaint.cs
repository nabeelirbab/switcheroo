using System;

namespace API.GraphQL.Complaints.Models
{
    public class Complaint
    {
        public Complaint(Guid? id, string title, string description, Guid createdByUserId, Guid updatedByUserId) 
        {
            Id = id;
            Title = title;
            Description = description;
            CreatedByUserId = createdByUserId;
            UpdatedByUserId = updatedByUserId;
        }
        public Guid? Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid CreatedByUserId { get; private set; }
        public Guid UpdatedByUserId { get; private set; }

        public static Complaint FromDomain(Domain.Complaints.Complaint domcomplaint)
        {
            if (!domcomplaint.Id.HasValue) throw new ApiException("Mapping error. Id missing");
            if (!domcomplaint.CreatedByUserId.HasValue) throw new ApiException("Mapping error. CreatedByUserId missing");
            if (!domcomplaint.UpdatedByUserId.HasValue) throw new ApiException("Mapping error. UpdatedByUserId missing");

            return new Complaint(
                domcomplaint.Id.Value,
                domcomplaint.Title,
                domcomplaint.Description,
                domcomplaint.CreatedByUserId.Value,
                domcomplaint.UpdatedByUserId.Value
                );
        }
    }
}
