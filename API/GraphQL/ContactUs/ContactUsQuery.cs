using API.GraphQL.ContactUs.Model;
using Domain.Complaints;
using Domain.ContactUs;
using GraphQL;
using HotChocolate;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.GraphQL
{
    public partial class Query
    {

        [Authorize]

        public async Task<List<ContactUs.Model.ContactUs >> GetContactUs(
            [Service] IContactUsRepository contactUsRepository)
        {
            var contactUs = await contactUsRepository.GetContactUs();


            return ContactUs.Model.ContactUs.FromDomains(contactUs);
        }
    }
}
