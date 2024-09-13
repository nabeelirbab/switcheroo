using API.GraphQL.ContactUs.Model;
using API.GraphQL.Model;
using Domain.Complaints;
using Domain.Notifications;
using GraphQL;
using HotChocolate;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.GraphQL
{
    public partial class Query
    {
        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin" })]
        public async Task<List<AppVersion>> GetAppVersions(
            [Service] Domain.Version.IAppVersionRepository repository)
        {
            var entities = await repository.GetAll();
            return AppVersion.FromDomains(entities);
        }
        public async Task<AppVersion> GetLatestAppVersion(
            [Service] Domain.Version.IAppVersionRepository repository)
        {
            var entities = await repository.GetLatest();
            return AppVersion.FromDomain(entities);
        }
    }
}
