using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Categories;
using HotChocolate;

namespace API.GraphQL
{
    public partial class Query
    {
        public async Task<IEnumerable<string>> GetCategories([Service]ICategoryRepository categoryRepository)
            => (await categoryRepository.GetAllCategories())
                .Select(z => z.Name);
    }
}
