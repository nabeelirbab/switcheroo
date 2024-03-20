using HotChocolate;
using System.Threading.Tasks;
using Domain.Complaints;
using System.Collections.Generic;

namespace API.GraphQL
{
    public partial class Query
    {

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin" })]
        public async Task<List<Complaints.Models.Complaint>> GetComplaints(
            [Service] IComplaintRepository complaintRepository)
        {
            var complaints = await complaintRepository.GetComplaints();
            return Complaints.Models.Complaint.FromDomains(complaints);
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin" })]
        public async Task<List<Complaints.Models.Complaint>> GetRestrictedItems(
            [Service] IComplaintRepository complaintRepository)
        {
            var restrictedItems = await complaintRepository.GetRestrictedItems();
            return Complaints.Models.Complaint.FromDomains(restrictedItems);

        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin" })]
        public async Task<List<Complaints.Models.Complaint>> GetRestrictedUsers(
            [Service] IComplaintRepository complaintRepository)
        {
            var restrictedItems = await complaintRepository.GetRestrictedUsers();
            return Complaints.Models.Complaint.FromDomains(restrictedItems);

        }
    }
}
