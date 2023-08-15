using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Categories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllCategories();

        Task<IEnumerable<Category>> GetCategoriesByNames(IEnumerable<string> categoryNames);
    }
}
