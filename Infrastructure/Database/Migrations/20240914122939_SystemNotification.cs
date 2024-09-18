using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    public partial class SystemNotification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("6424e547-f5fb-4783-a423-31e0046d6a50"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("65641e17-1381-4f4f-8738-5a6a8aad6867"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("a72bad54-deac-45fe-bbfb-f0d4de0c9706"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("c6504d5b-858e-4af7-afbf-c4e3218c596e"));

            migrationBuilder.CreateTable(
                name: "SystemNotification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Data = table.Column<string>(type: "text", nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReadAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemNotification", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("22bab7f6-28d5-46dc-91fd-96edca4dc5dc"), "Furniture" },
                    { new Guid("3038a173-8886-415f-be16-b82ffb2a414f"), "Electronics" },
                    { new Guid("560f8b31-f88c-42b2-82f5-0c8d5bf8e63d"), "White Goods" },
                    { new Guid("fd5cf32c-6c82-4232-b3ed-8b1f2b8f214f"), "Clothing" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemNotification");

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("22bab7f6-28d5-46dc-91fd-96edca4dc5dc"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("3038a173-8886-415f-be16-b82ffb2a414f"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("560f8b31-f88c-42b2-82f5-0c8d5bf8e63d"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("fd5cf32c-6c82-4232-b3ed-8b1f2b8f214f"));

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("6424e547-f5fb-4783-a423-31e0046d6a50"), "Clothing" },
                    { new Guid("65641e17-1381-4f4f-8738-5a6a8aad6867"), "Furniture" },
                    { new Guid("a72bad54-deac-45fe-bbfb-f0d4de0c9706"), "White Goods" },
                    { new Guid("c6504d5b-858e-4af7-afbf-c4e3218c596e"), "Electronics" }
                });
        }
    }
}
