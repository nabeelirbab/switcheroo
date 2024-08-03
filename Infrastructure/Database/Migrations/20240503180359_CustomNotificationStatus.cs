using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    public partial class CustomNotificationStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("384649bd-08ae-46d0-906d-e25850986594"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("83c7a8db-ac16-4a8b-ae3f-08b49c6bd624"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("94a2a3e3-203a-48f7-8a54-d723232151ad"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("96e21924-e819-4d21-8016-bdd99646e1b3"));

            migrationBuilder.CreateTable(
                name: "CustomNotificationStatus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomNotificationStatus", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("7fc851b4-d917-4088-908b-3586ad77a959"), "Clothing" },
                    { new Guid("8c002103-308d-4ebb-b5ff-52a07faaed0c"), "White Goods" },
                    { new Guid("95129bfe-02e2-4f4e-8497-b52488a0bbe9"), "Electronics" },
                    { new Guid("a3e8978e-a943-4eca-83c2-6fc93b180f77"), "Furniture" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomNotificationStatus");

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("7fc851b4-d917-4088-908b-3586ad77a959"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("8c002103-308d-4ebb-b5ff-52a07faaed0c"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("95129bfe-02e2-4f4e-8497-b52488a0bbe9"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("a3e8978e-a943-4eca-83c2-6fc93b180f77"));

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("384649bd-08ae-46d0-906d-e25850986594"), "Electronics" },
                    { new Guid("83c7a8db-ac16-4a8b-ae3f-08b49c6bd624"), "White Goods" },
                    { new Guid("94a2a3e3-203a-48f7-8a54-d723232151ad"), "Clothing" },
                    { new Guid("96e21924-e819-4d21-8016-bdd99646e1b3"), "Furniture" }
                });
        }
    }
}
