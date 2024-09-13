using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    public partial class AppVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("416bd625-72eb-4f3b-8ae2-3e68e28b46cd"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("96e71f32-b099-41f3-80c8-a293e9f02eae"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("a8e944ff-7c0e-4177-902d-1550397af767"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("eaf7f5b5-f21d-4ef9-ac80-c7724e0baac4"));

            migrationBuilder.CreateTable(
                name: "AppVersion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AndroidVersion = table.Column<string>(type: "text", nullable: false),
                    IOSVersion = table.Column<string>(type: "text", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ArchivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppVersion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppVersion_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppVersion_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_AppVersion_CreatedByUserId",
                table: "AppVersion",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppVersion_UpdatedByUserId",
                table: "AppVersion",
                column: "UpdatedByUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppVersion");

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

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("416bd625-72eb-4f3b-8ae2-3e68e28b46cd"), "Clothing" },
                    { new Guid("96e71f32-b099-41f3-80c8-a293e9f02eae"), "Furniture" },
                    { new Guid("a8e944ff-7c0e-4177-902d-1550397af767"), "Electronics" },
                    { new Guid("eaf7f5b5-f21d-4ef9-ac80-c7724e0baac4"), "White Goods" }
                });
        }
    }
}
