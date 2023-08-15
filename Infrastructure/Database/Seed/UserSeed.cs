using System;
using Infrastructure.Database.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Seed
{
    public static class UserSeed
    {
        public static Guid AdminId = Guid.Parse("a18be9c0-aa65-4af8-bd17-00bd9344e575");
        public static Guid TestUserId = Guid.Parse("a18be9c0-aa65-4af8-bd17-00bd9344e576");

        public static void SeedUsers(this ModelBuilder modelBuilder)
        {
            // These need to be hardcoded so that if you run the migration twice
            // you won't get duplicate rows.
            var roleId = Guid.Parse("c63a092d-e08e-4962-8d82-e0c10212831b");
            var adminEmail = "admin@switcherooapp.com.au";
            var now = DateTime.UtcNow;
            var adminPassword = "ABCD1234!";

            modelBuilder.Entity<IdentityRole<Guid>>().HasData(new IdentityRole<Guid>
            {
                Id = roleId,
                Name = "Administrator",
                NormalizedName = "administrator"
            });
            
            var textUserRoleId = Guid.Parse("c63a092d-e08e-4962-8d82-e0c10212833b");
            var textUserEmail = "test@switcherooapp.com.au";
            var textUserPassword = "ABCD1234!";

            modelBuilder.Entity<IdentityRole<Guid>>().HasData(new IdentityRole<Guid>
            {
                Id = textUserRoleId,
                Name = "Test",
                NormalizedName = "test"
            });

            var hasher = new PasswordHasher<User>();

            modelBuilder.Entity<User>().HasData(new User(adminEmail, "Admin", "Admin", null, null, null, null, "I love things. Have too many of them though. Keen to swap yo!", "https://picsum.photos/300/300", adminEmail, true, true)
            {
                Id = AdminId,
                NormalizedUserName = adminEmail.ToUpper(),
                Email = adminEmail,
                NormalizedEmail = adminEmail.ToUpper(),
                EmailConfirmed = true,
                PasswordHash = hasher.HashPassword(null, adminPassword),
                SecurityStamp = string.Empty,
                CreatedAt = now,
                UpdatedAt = now
            });

            modelBuilder.Entity<IdentityUserRole<Guid>>().HasData(new IdentityUserRole<Guid>
            {
                RoleId = roleId,
                UserId = AdminId
            });
            
            modelBuilder.Entity<User>().HasData(new User(textUserEmail, "Test", "User", null, null, null, null, "Swap swap swapperoooooo yew yew yew!", "https://picsum.photos/300/300", textUserEmail, true, true)
            {
                Id = TestUserId,
                NormalizedUserName = textUserEmail.ToUpper(),
                Email = textUserEmail,
                NormalizedEmail = textUserEmail.ToUpper(),
                EmailConfirmed = true,
                PasswordHash = hasher.HashPassword(null, textUserPassword),
                SecurityStamp = string.Empty,
                CreatedAt = now,
                UpdatedAt = now
            });

            modelBuilder.Entity<IdentityUserRole<Guid>>().HasData(new IdentityUserRole<Guid>
            {
                RoleId = roleId,
                UserId = TestUserId
            });
        }
    }
}