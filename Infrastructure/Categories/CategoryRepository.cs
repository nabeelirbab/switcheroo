using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Categories;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Categories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly SwitcherooContext db;

        public CategoryRepository(SwitcherooContext db)
        {
            this.db = db;
        }

        public async Task<bool> CreateCategories(List<string> name)
        {
            foreach (var category in name)
            {
                var newCategory = new Database.Schema.Category(
                category
                )
                {
                    Name = category,
                };
                db.Categories.Add(newCategory);
            }

            await db.SaveChangesAsync();

            var caregories = await db.Categories.ToListAsync();
            if ( caregories.Count > 0 ) { return true; }
            else { return false; }
            
        }

        public async Task<IEnumerable<Category>> GetAllCategories()
        {
            return await db.Categories
                .Select(cat => new Category(cat.Id, cat.Name))
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetCategoriesByNames(IEnumerable<string> categoryNames)
        {
            return await db.Categories
                .Where(cat => categoryNames.Contains(cat.Name))
                .Select(cat => new Category(cat.Id, cat.Name))
                .ToListAsync();
        }
    }
}
