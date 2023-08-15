using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Database
{
    public class PostgresDbConfigurator : IDbContextConfigurator
    {
        private readonly PostgresOptions postgresOptions;

        public PostgresDbConfigurator(IOptions<PostgresOptions> pgOptions)
        {
            postgresOptions = pgOptions.Value;
        }

        public DbContextOptionsBuilder Configure(DbContextOptionsBuilder options)
        {
            // TODO configure some logging if isDevelopment

            var stringBuilder = new StringBuilder();

            stringBuilder.Append($"Host={postgresOptions.POSTGRES_HOST};");
            stringBuilder.Append($"Port={postgresOptions.POSTGRES_PORT};");
            stringBuilder.Append($"Database={postgresOptions.POSTGRES_DATABASE};");
            stringBuilder.Append($"User ID={postgresOptions.POSTGRES_USER};");
            stringBuilder.Append($"Password={postgresOptions.POSTGRES_PASSWORD};");

            if (!string.IsNullOrWhiteSpace(postgresOptions.POSTGRES_TIMEOUT)) stringBuilder.Append($"Timeout={postgresOptions.POSTGRES_TIMEOUT};");
            if (!string.IsNullOrWhiteSpace(postgresOptions.POSTGRES_COMMANDTIMEOUT)) stringBuilder.Append($"Command Timeout={postgresOptions.POSTGRES_COMMANDTIMEOUT};");
            if (!string.IsNullOrWhiteSpace(postgresOptions.POSTGRES_MINPOOLSIZE)) stringBuilder.Append($"Minimum Pool Size={postgresOptions.POSTGRES_MINPOOLSIZE};");
            if (!string.IsNullOrWhiteSpace(postgresOptions.POSTGRES_MAXPOOLSIZE)) stringBuilder.Append($"Maximum Pool Size={postgresOptions.POSTGRES_MAXPOOLSIZE};");
            if (!string.IsNullOrWhiteSpace(postgresOptions.POSTGRES_MAXAUTOPREPARE)) stringBuilder.Append($"Max Auto Prepare={postgresOptions.POSTGRES_MAXAUTOPREPARE};");
            if (!string.IsNullOrWhiteSpace(postgresOptions.POSTGRES_AUTOPREPAREMINUSAGES)) stringBuilder.Append($"Auto Prepare Min Usages={postgresOptions.POSTGRES_AUTOPREPAREMINUSAGES};");
            if (!string.IsNullOrWhiteSpace(postgresOptions.POSTGRES_KEEPALIVE)) stringBuilder.Append($"Keepalive={postgresOptions.POSTGRES_KEEPALIVE}");

            return options.UseNpgsql(stringBuilder.ToString(), o => o.EnableRetryOnFailure());
        }
    }
}
