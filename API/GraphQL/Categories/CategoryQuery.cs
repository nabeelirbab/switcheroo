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
        public async Task<IEnumerable<Categories>> GetCategories(
            [Service]ICategoryRepository categoryRepository)
            => (await categoryRepository.GetAllCategories())
                .Select(z => new Categories { Id = z.Id, Name = z.Name });
    }
}
public class Categories
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}
