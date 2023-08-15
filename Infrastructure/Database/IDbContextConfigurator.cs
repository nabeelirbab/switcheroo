using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database
{
    public interface IDbContextConfigurator
    {
        DbContextOptionsBuilder Configure(DbContextOptionsBuilder options);
    }
}
