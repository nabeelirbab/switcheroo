using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    public partial class CustomNotificationFilters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("633c43f5-4d2c-49ed-9dc4-bd0cb6dd0e1c"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("85c8daec-efae-4f5e-8728-2a569ebd9708"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("8d8a0ac0-4984-4fa7-8776-7f22bc858a7e"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("8f5aa47f-c794-47bd-9986-b0943c88e199"));

            migrationBuilder.CreateTable(
                name: "CustomNotificationFilters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomNotificationId = table.Column<Guid>(type: "uuid", maxLength: 500, nullable: false),
                    GenderFilter = table.Column<string>(type: "text", nullable: true),
                    ItemFilter = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomNotificationFilters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomNotificationFilters_CustomNotification_CustomNotifica~",
                        column: x => x.CustomNotificationId,
                        principalTable: "CustomNotification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_CustomNotificationFilters_CustomNotificationId",
                table: "CustomNotificationFilters",
                column: "CustomNotificationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomNotificationFilters");

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

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("633c43f5-4d2c-49ed-9dc4-bd0cb6dd0e1c"), "Furniture" },
                    { new Guid("85c8daec-efae-4f5e-8728-2a569ebd9708"), "Clothing" },
                    { new Guid("8d8a0ac0-4984-4fa7-8776-7f22bc858a7e"), "Electronics" },
                    { new Guid("8f5aa47f-c794-47bd-9986-b0943c88e199"), "White Goods" }
                });
        }
    }
}
