using Domain.Categories;
using Domain.Users;
using HotChocolate;
using System;
using System.Threading.Tasks;

namespace API.GraphQL.Categories.Model
{
    public class Categories
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public async Task<decimal> GetAveragePrice([Service] ICategoryRepository categoryRepository)
        {
            return await categoryRepository.GetAveragePrice(Id);
        }
    }

}
