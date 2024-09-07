using System.Linq;
using System.Data.Common;
using Domain.Users;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace Infrastructure.Database
{
    public class DbQueryInterceptor : DbCommandInterceptor
    {
        private readonly IUserRoleProvider _userRoleProvider;
        private readonly IServiceProvider _serviceProvider;

        public DbQueryInterceptor(IUserRoleProvider userRoleProvider, IServiceProvider serviceProvider)
        {
            _userRoleProvider = userRoleProvider;
            _serviceProvider = serviceProvider;
        }

        //public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command,
        //    CommandEventData eventData,
        //    InterceptionResult<DbDataReader> result)
        //{
        //    if (_userRoleProvider.IsAdminOrSuperAdmin)
        //    {
        //        // Modify the query to ignore filters if the user is an admin
        //        command.CommandText = command.CommandText.Replace("WHERE [IsDeleted] = 0", ""); // Example for IsDeleted filters
        //    }

        //    return base.ReaderExecuting(command, eventData, result);
        //}

        //public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command,
        //    CommandEventData eventData,
        //    InterceptionResult<DbDataReader> result,
        //    CancellationToken cancellationToken = default)
        //{
        //    if (_userRoleProvider.IsAdminOrSuperAdmin)
        //    {
        //        // Modify the query to ignore filters if the user is an admin
        //        command.CommandText = command.CommandText.Replace("WHERE [IsDeleted] = 0", ""); // Example for IsDeleted filters
        //    }

        //    return await base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        //}

        public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result)
        {
            ModifyCommandIfNecessary(command);
            return base.ReaderExecuting(command, eventData, result);
        }

        public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            ModifyCommandIfNecessary(command);
            return await base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }

        private void ModifyCommandIfNecessary(DbCommand command)
        {
            if (_userRoleProvider.IsAdminOrSuperAdmin)
            {
                // Match variations of IsDeleted filtering inside WHERE clauses, keeping the "AS u" intact
                var pattern = @"(\s+AND\s+NOT\s*\(\s*u\.""IsDeleted""\s*\)\s*)|(\s+AND\s+u\.""IsDeleted""\s*=\s*false\s*)";

                // Replace the IsDeleted filter in the SQL command
                command.CommandText = Regex.Replace(command.CommandText, pattern, "", RegexOptions.IgnoreCase).Trim();

                // Clean up any dangling WHERE or AND clauses
                command.CommandText = Regex.Replace(command.CommandText, @"\s+AND\s*$", "", RegexOptions.IgnoreCase);
                command.CommandText = Regex.Replace(command.CommandText, @"\s+WHERE\s*AND", "WHERE", RegexOptions.IgnoreCase);

                // Ensure proper spacing after "AS u"
                command.CommandText = Regex.Replace(command.CommandText, @"AS uWHERE", "AS u WHERE", RegexOptions.IgnoreCase);
            }
        }
    }
}
