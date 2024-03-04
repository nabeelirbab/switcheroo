using System;
using Infrastructure.Database.Schema;
using Infrastructure.Database.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database
{
    public class SwitcherooContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        private readonly IDbContextConfigurator configurator;

        public SwitcherooContext(IDbContextConfigurator configurator)
        {
            this.configurator = configurator;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            configurator.Configure(optionsBuilder);
        }

        public DbSet<UserVerificationCode> UserVerificationCodes { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;

        public DbSet<Complaint> Complaints { get; set; } = null!;

        public DbSet<Schema.ContactUs> ContactUs { get; set; } = null!;
        public DbSet<Item> Items { get; set; } = null!;
        public DbSet<ItemCategory> ItemCategories { get; set; } = null!;
        public DbSet<ItemImage> ItemImages { get; set; } = null!;
        public DbSet<Offer> Offers { get; set; } = null!;
        public DbSet<DismissedItem> DismissedItem { get; set; } = null!;
        public DbSet<Message> Messages { get; set; } = null!;
        public DbSet<Location> Location { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.UseIdentityColumns();

            // Enum Registrations
            modelBuilder.HasPostgresEnum<OfferStatus>();

            // Global Query Filters https://docs.microsoft.com/en-us/ef/core/querying/filters
            // This will ensure that any query will exclude entities that are archived.
            // to explicitly get archivedAt, you can disable it by: .IgnoreQueryFilters()
            modelBuilder.Entity<Item>().HasQueryFilter(z => !z.ArchivedAt.HasValue);
            modelBuilder.Entity<User>().HasQueryFilter(z => !z.ArchivedAt.HasValue);
            modelBuilder.Entity<Offer>().HasQueryFilter(z => !z.ArchivedAt.HasValue);
            modelBuilder.Entity<Message>().HasQueryFilter(z => !z.ArchivedAt.HasValue);

            // UserVerificationCode Indexes
            modelBuilder.Entity<UserVerificationCode>()
                .HasIndex(x => new { x.Email, x.SixDigitCode })
                .IsUnique();
            modelBuilder.Entity<UserVerificationCode>()
                .HasIndex(x => new { x.EmailConfirmationToken })
                .IsUnique();

            // Category Indexes
            modelBuilder.Entity<Category>()
                .HasIndex(x => new { x.Name })
                .IsUnique();

            // ItemCategory Indexes
            modelBuilder.Entity<ItemCategory>()
                .HasIndex(x => new { x.CategoryId, x.ItemId })
                .IsUnique();

            // Offer Indexes
            modelBuilder.Entity<Offer>()
                .HasIndex(x => new { x.SourceItemId, x.TargetItemId })
                .IsUnique();

            // DismissedItem Indexes
            //modelBuilder.Entity<DismissedItem>()
            //    .HasIndex(x => new { x.SourceItemId, x.TargetItemId })
            //    .IsUnique();

            // Many to many relationships
            modelBuilder.Entity<ItemCategory>()
                .HasOne(x => x.Item)
                .WithMany(x => x.ItemCategories)
                .HasForeignKey(x => x.ItemId);

            modelBuilder.Entity<ItemCategory>()
                .HasOne(x => x.Category)
                .WithMany(x => x.ItemCategories)
                .HasForeignKey(x => x.CategoryId);

            // Some table name clean up
            modelBuilder.Entity<User>(entity => { entity.ToTable("Users"); });
            modelBuilder.Entity<IdentityRole<Guid>>(entity => { entity.ToTable("Roles"); });
            modelBuilder.Entity<IdentityUserRole<Guid>>(entity => { entity.ToTable("UserRoles"); });
            modelBuilder.Entity<IdentityUserClaim<Guid>>(entity => { entity.ToTable("UserClaims"); });
            modelBuilder.Entity<IdentityUserLogin<Guid>>(entity => { entity.ToTable("UserLogins"); });
            modelBuilder.Entity<IdentityUserToken<Guid>>(entity => { entity.ToTable("UserTokens"); });
            modelBuilder.Entity<IdentityRoleClaim<Guid>>(entity => { entity.ToTable("RoleClaims"); });

            // Indexing
            modelBuilder.Entity<Item>().HasIndex(p => p.CreatedByUserId);
            modelBuilder.Entity<Item>().HasIndex(p => p.IsHidden);
            modelBuilder.Entity<DismissedItem>().HasIndex(p => p.CreatedByUserId);
            modelBuilder.Entity<DismissedItem>().HasIndex(p => new { p.CreatedByUserId, p.TargetItemId });

            var categories = modelBuilder.SeedCategories();
        }
    }
}
