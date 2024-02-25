using Domain.Items;
using HotChocolate;
using System.Threading.Tasks;
using System;
using GraphQL;
using Domain.Complaints;
using System.Collections.Generic;
using System.Linq;
using Domain.Users;
using Microsoft.AspNetCore.Http;
using Amazon.Lambda.AspNetCoreServer;

namespace API.GraphQL
{
    public partial class Query
    {

        [Authorize]

        public async Task<List<Complaints.Models.Complaint>> GetComplaints(
            [Service] IComplaintRepository complaintRepository)
        {
            var complaints = await complaintRepository.GetComplaints();


            return Complaints.Models.Complaint.FromDomains(complaints);
        }

        public async Task<List<Complaints.Models.Complaint>> GetRestrictedItems(
            [Service] IComplaintRepository complaintRepository)
        {
            var restrictedItems = await complaintRepository.GetRestrictedItems();

            return Complaints.Models.Complaint.FromDomains(restrictedItems);

        }

        public async Task<List<Complaints.Models.Complaint>> GetRestrictedUsers(
            [Service] IComplaintRepository complaintRepository)
        {
            var restrictedItems = await complaintRepository.GetRestrictedUsers();

            return Complaints.Models.Complaint.FromDomains(restrictedItems);

        }
    }
}
