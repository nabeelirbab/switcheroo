using Domain.Items;
using Domain.Users;
using HotChocolate;
using HotChocolate.Types.Relay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.GraphQL.Complaints.Models
{
    public class Complaint
    {
        public Complaint(Guid? id, string title, string description, Guid? createdByUserId, Guid? targetUserId, Guid? targetItemId)
        {
            Id = id;
            Title = title;
            Description = description;
            CreatedByUserId = createdByUserId;
            TargetUserId = targetUserId;
            TargetItemId = targetItemId;
        }
        public Complaint(Guid? id, string title, string description, Guid? createdByUserId, Guid updatedByUserId, Guid? targetUserId, Guid? targetItemId) 
        {
            Id = id;
            Title = title;
            Description = description;
            CreatedByUserId = createdByUserId;
            UpdatedByUserId = updatedByUserId;
            TargetUserId = targetUserId;
            TargetItemId = targetItemId;
        }
        public Guid? Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid? CreatedByUserId { get; private set; }
        public Guid UpdatedByUserId { get; private set; }
        public Guid? TargetUserId { get; set; }
        public Guid? TargetItemId { get; set; }

        [GraphQLNonNullType]
        public async Task<List<Users.Models.User>> GetCreatedUser(
            [Service] IUserRepository userRepository
        )
        {
            return (await userRepository.GetUserById(CreatedByUserId))
                .Select(Users.Models.User.FromDomain)
                .ToList();
        }

        public async Task<List<Users.Models.User>> GetTargetUser(
            [Service] IUserRepository userRepository
        )
        {
            return (await userRepository.GetUserById(TargetUserId))
                .Select(Users.Models.User.FromDomain)
                .ToList();
        }

        [GraphQLNonNullType]
        public async Task<List<Items.Models.Item>> GetTargetItem(
            [Service] IItemRepository itemRepository
        )
        {
            return (await itemRepository.GetTargetItemById(TargetItemId))
                .Select(Items.Models.Item.FromDomain)
                .ToList();
        }

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
                domcomplaint.UpdatedByUserId.Value,
                domcomplaint.TargetUserId,
                domcomplaint.TargetItemId
                );
        }

        public static List<Complaint> FromDomains(List<Domain.Complaints.Complaint> domComplaints)
        {
            if (domComplaints == null || domComplaints.Count == 0)
            {
                // Handle the case where there are no messages if needed.
                return new List<Complaint>();
            }

            return domComplaints.Select(newComplaint => new Complaint(
                newComplaint.Id,
                newComplaint.Title,
                newComplaint.Description,
                newComplaint.CreatedByUserId,
                newComplaint.TargetUserId,
                newComplaint.TargetItemId))
                .ToList();
        }
    }
}
