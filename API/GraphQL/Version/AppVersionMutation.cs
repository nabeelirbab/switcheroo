using HotChocolate;
using Infrastructure;
using System.Threading.Tasks;
using System;
using API.GraphQL.CommonServices;
using API.GraphQL.Model;

namespace API.GraphQL
{
    public partial class Mutation
    {
        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin" })]
        public async Task<AppVersion> CreateAppVersion(
            [Service] UserContextService userContextService,
            [Service] Domain.Version.IAppVersionRepository repository,
            Domain.Version.AppVersion version
        )
        {
            try
            {
                var requestUserId = userContextService.GetCurrentUserId();
                var newDomainEntity = await repository.CreateAsync(Domain.Version.AppVersion.CreateNewAppVersion(
                    version.AndroidVersion,
                    version.IOSVersion,
                    Guid.NewGuid()
                ), requestUserId);
                return AppVersion.FromDomain(newDomainEntity);
            }
            catch (Exception ex)
            {
                throw new InfrastructureException($"API Exception {ex}");
            }
        }
    }
}
