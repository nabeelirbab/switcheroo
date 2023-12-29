using Domain.ContactUs;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.ContactUs
{
    public class ContactUsRepositoy : IContactUsRepository
    {
        private readonly SwitcherooContext db;
        public async Task<Domain.ContactUs.ContactUs> CreateContactUsAsync(Domain.ContactUs.ContactUs contactUs)
        {
            try
            {
                var now = DateTime.UtcNow;
                if (!contactUs.CreatedByUserId.HasValue)
                    throw new InfrastructureException("No createdByUserId provided");

                var newDbContactUs = new Database.Schema.ContactUs(
                    contactUs.Title,
                    contactUs.Description,
                    contactUs.Name,
                    contactUs.Email
                )
                {
                    CreatedByUserId = contactUs.CreatedByUserId.Value,
                    UpdatedByUserId = contactUs.CreatedByUserId.Value,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                await db.ContactUs.AddAsync(newDbContactUs);
                await db.SaveChangesAsync();

                return await GetContactUsById(newDbContactUs.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception {ex.InnerException}");  
                throw new InfrastructureException($"Exception:{ex.Message}");
            }
        }

        public async Task<List<Domain.ContactUs.ContactUs>> GetContactUs()
        {
            return await db.ContactUs
                .Select(Database.Schema.ContactUs.ToDomain)
                .ToListAsync();
        }

        public async Task<Domain.ContactUs.ContactUs> GetContactUsById(Guid contactId)
        {
            var item = await db.ContactUs
                .Where(z => z.Id == contactId)
                .Select(Database.Schema.ContactUs.ToDomain)
                .FirstOrDefaultAsync();

            if (item == null) throw new InfrastructureException($"Unable to locate contactId");

            return item;
        }
    }
}
