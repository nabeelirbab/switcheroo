using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Categories;
using HotChocolate;

namespace API.GraphQL
{
    public partial class Query
    {
        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<IEnumerable<Categories.Model.Categories>> GetCategories(
            [Service] ICategoryRepository categoryRepository)
        {
            var categories = await categoryRepository.GetAllCategories();

            return categories
                .Select(z => new Categories.Model.Categories { Id = z.Id, Name = z.Name })
                .OrderBy(c => c.Name.Equals("Other", StringComparison.OrdinalIgnoreCase) ? 1 : 0)
                .ThenBy(c => c.Name);

        }
    }
}