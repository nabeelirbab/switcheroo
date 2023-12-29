using Domain.Complaints;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ContactUs
{
    public interface IContactUsRepository
    {
        Task<ContactUs> CreateContactUsAsync(ContactUs complaint);
        Task<ContactUs> GetContactUsById(Guid contacttId);
        Task<List<ContactUs>> GetContactUs();
    }
}
