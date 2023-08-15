using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Infrastructure.Database.Migrations
{
    public partial class SimplifiedCategory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Users_CreatedByUserId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Users_UpdatedByUserId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_CreatedByUserId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_UpdatedByUserId",
                table: "Categories");

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("04e1cc13-aef8-4297-a8ed-2b6c55c7e277"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("11e6234c-8a6c-494d-9f7d-75e453162101"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("47783eaf-8236-48bd-9df6-df2c540d4464"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("a7c76613-85d8-4707-bf58-c420cb29043d"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c63a092d-e08e-4962-8d82-e0c10212833b"));

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumns: new[] { "UserId", "RoleId" },
                keyValues: new object[] { new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e575"), new Guid("c63a092d-e08e-4962-8d82-e0c10212831b") });

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumns: new[] { "UserId", "RoleId" },
                keyValues: new object[] { new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e576"), new Guid("c63a092d-e08e-4962-8d82-e0c10212831b") });

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c63a092d-e08e-4962-8d82-e0c10212831b"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e575"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e576"));

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Categories");

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("c6089450-3005-479d-a834-551588eed14b"), "Electronics" },
                    { new Guid("2179793d-65bf-4d1e-8f66-cbf9d18e45f9"), "White Goods" },
                    { new Guid("b82c4942-8be8-4ffa-bb22-74624ea032d9"), "Clothing" },
                    { new Guid("b0498a70-5e46-4cdc-8021-c30b590d752c"), "Furniture" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("2179793d-65bf-4d1e-8f66-cbf9d18e45f9"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("b0498a70-5e46-4cdc-8021-c30b590d752c"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("b82c4942-8be8-4ffa-bb22-74624ea032d9"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("c6089450-3005-479d-a834-551588eed14b"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ArchivedAt",
                table: "Categories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Categories",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Categories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Categories",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "Categories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("c63a092d-e08e-4962-8d82-e0c10212831b"), "74b63bb6-a865-472f-8015-191fb57a3839", "Administrator", "administrator" },
                    { new Guid("c63a092d-e08e-4962-8d82-e0c10212833b"), "21eb6acf-ce8f-42f8-93c7-6ff390b3d9dc", "Test", "test" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AccessFailedCount", "ArchivedAt", "AvatarUrl", "Blurb", "ConcurrencyStamp", "CreatedAt", "DateOfBirth", "Distance", "Email", "EmailConfirmed", "FirstName", "Gender", "IsChatNotificationsEnabled", "IsMatchNotificationsEnabled", "LastName", "LockoutEnabled", "LockoutEnd", "Mobile", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UpdatedAt", "UserName" },
                values: new object[,]
                {
                    { new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e575"), 0, null, "https://picsum.photos/300/300", "I love things. Have too many of them though. Keen to swap yo!", "617293a5-357c-44ee-83c9-8f320ad84066", new DateTimeOffset(new DateTime(2020, 8, 15, 6, 11, 56, 74, DateTimeKind.Unspecified).AddTicks(2304), new TimeSpan(0, 0, 0, 0, 0)), null, null, "admin@switcherooapp.com.au", true, "Admin", null, true, true, "Admin", false, null, null, "ADMIN@SWITCHEROOAPP.COM.AU", "ADMIN@SWITCHEROOAPP.COM.AU", "AQAAAAEAACcQAAAAEL+KC2Ikv9s4Q7W92dgtTAZPbm1tDiJeR1/5+aOLeXFBHdKuDeoLgRzdSz+MqoaRyg==", null, false, "", false, new DateTimeOffset(new DateTime(2020, 8, 15, 6, 11, 56, 74, DateTimeKind.Unspecified).AddTicks(2304), new TimeSpan(0, 0, 0, 0, 0)), "admin@switcherooapp.com.au" },
                    { new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e576"), 0, null, "https://picsum.photos/300/300", "Swap swap swapperoooooo yew yew yew!", "788054a0-623a-4cf7-9556-dffc425ba978", new DateTimeOffset(new DateTime(2020, 8, 15, 6, 11, 56, 74, DateTimeKind.Unspecified).AddTicks(2304), new TimeSpan(0, 0, 0, 0, 0)), null, null, "test@switcherooapp.com.au", true, "Test", null, true, true, "User", false, null, null, "TEST@SWITCHEROOAPP.COM.AU", "TEST@SWITCHEROOAPP.COM.AU", "AQAAAAEAACcQAAAAEIKcuCoW7hQjGLQvX18rYAxwgcNf6Dg9+K1hDWjW+6qDGBuuLekOQWi9qoPYJauvsw==", null, false, "", false, new DateTimeOffset(new DateTime(2020, 8, 15, 6, 11, 56, 74, DateTimeKind.Unspecified).AddTicks(2304), new TimeSpan(0, 0, 0, 0, 0)), "test@switcherooapp.com.au" }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "ArchivedAt", "CreatedAt", "CreatedByUserId", "Name", "UpdatedAt", "UpdatedByUserId" },
                values: new object[,]
                {
                    { new Guid("47783eaf-8236-48bd-9df6-df2c540d4464"), null, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e575"), "Electronics", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e575") },
                    { new Guid("04e1cc13-aef8-4297-a8ed-2b6c55c7e277"), null, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e575"), "White Goods", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e575") },
                    { new Guid("11e6234c-8a6c-494d-9f7d-75e453162101"), null, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e575"), "Clothing", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e575") },
                    { new Guid("a7c76613-85d8-4707-bf58-c420cb29043d"), null, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e575"), "Furniture", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e575") }
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e575"), new Guid("c63a092d-e08e-4962-8d82-e0c10212831b") },
                    { new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e576"), new Guid("c63a092d-e08e-4962-8d82-e0c10212831b") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CreatedByUserId",
                table: "Categories",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_UpdatedByUserId",
                table: "Categories",
                column: "UpdatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Users_CreatedByUserId",
                table: "Categories",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Users_UpdatedByUserId",
                table: "Categories",
                column: "UpdatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
