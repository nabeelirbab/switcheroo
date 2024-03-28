using Domain.Categories;
using Domain.Users;
using HotChocolate;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.GraphQL
{
    public partial class Mutation
    {
        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<bool> CreateCategories(
           [Service] ICategoryRepository categoryRepository,
           List<string> name
       )
        {
            await categoryRepository.CreateCategories(name);
            return true;
        }
    }
}
