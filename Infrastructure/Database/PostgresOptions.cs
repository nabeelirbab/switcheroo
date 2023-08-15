using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Database
{
    // http://www.npgsql.org/doc/connection-string-parameters.html
    public class PostgresOptions
    {
        [Required]
        public string POSTGRES_HOST { get; set; } = null!;
        [Required]
        public string POSTGRES_PORT { get; set; } = null!;
        [Required]
        public string POSTGRES_DATABASE { get; set; } = null!;
        [Required]
        public string POSTGRES_USER { get; set; } = null!;
        [Required]
        public string POSTGRES_PASSWORD { get; set; } = null!;

        public string? POSTGRES_TIMEOUT { get; set; }
        public string? POSTGRES_COMMANDTIMEOUT { get; set; }
        public string? POSTGRES_MINPOOLSIZE { get; set; }
        public string? POSTGRES_MAXPOOLSIZE { get; set; }
        public string? POSTGRES_MAXAUTOPREPARE { get; set; }
        public string? POSTGRES_AUTOPREPAREMINUSAGES { get; set; }
        public string? POSTGRES_KEEPALIVE { get; set; }
    }
}
