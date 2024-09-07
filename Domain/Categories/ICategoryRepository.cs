using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Categories
{
    public interface ICategoryRepository
    {
        Task<bool> CreateCategories(List<string> categories);

        Task<IEnumerable<Category>> GetAllCategories();

        Task<IEnumerable<Category>> GetCategoriesByNames(IEnumerable<string> categoryNames);

        Task<decimal> GetAveragePrice(Guid caregoryId);
    }
}
