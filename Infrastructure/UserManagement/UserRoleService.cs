using Infrastructure.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.UserManagement
{
    public class UserRoleService
    {
        private readonly IDbContextConfigurator _configurator;
        private readonly UserManager<Infrastructure.Database.Schema.User> _userManager;
        private readonly Domain.Users.IUserRoleProvider _userRoleProvider;

        public UserRoleService(IDbContextConfigurator configurator, UserManager<Infrastructure.Database.Schema.User> userManager, Domain.Users.IUserRoleProvider userRoleProvider)
        {
            _configurator = configurator;
            _userManager = userManager;
            _userRoleProvider = userRoleProvider;
        }

        public async Task<List<string>> GetUserRolesAsync(Guid userId)
        {
            using (var context = new SwitcherooContext(_configurator, _userRoleProvider))
            {
                var userManager = new UserManager<Infrastructure.Database.Schema.User>(
                    new UserStore<Infrastructure.Database.Schema.User, IdentityRole<Guid>, SwitcherooContext, Guid>(context),
                    null, null, null, null, null, null, null, null
                );

                var user = await userManager.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    throw new ArgumentException("User not found");
                }

                var roles = await userManager.GetRolesAsync(user);
                return roles.ToList();
            }
        }
    }
}
