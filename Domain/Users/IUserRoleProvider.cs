using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Users
{
    public interface IUserRoleProvider
    {
        bool IsAdminOrSuperAdmin { get; }
    }
}
