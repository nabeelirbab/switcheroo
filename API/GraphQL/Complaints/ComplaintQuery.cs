using Domain.Items;
using HotChocolate;
using System.Threading.Tasks;
using System;
using GraphQL;
using Domain.Complaints;
using System.Collections.Generic;
using System.Linq;

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
    }
}
