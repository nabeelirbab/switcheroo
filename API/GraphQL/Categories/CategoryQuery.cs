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
            => (await categoryRepository.GetAllCategories())
                .Select(z => new Categories.Model.Categories { Id = z.Id, Name = z.Name }).OrderBy(comparer => comparer.Name);
    }
}