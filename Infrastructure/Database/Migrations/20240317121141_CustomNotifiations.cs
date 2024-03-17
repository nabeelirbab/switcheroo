using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    public partial class CustomNotifiations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("49825113-90a9-498b-b2ca-51463a28eed4"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("49827294-0a92-47dc-8cf1-effa5341f766"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4a434ee2-256b-4113-9677-fb0f695ef634"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("c7271f09-7c1a-4398-9683-945a3169c993"));

            migrationBuilder.CreateTable(
                name: "CustomNotification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ArchivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomNotification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomNotification_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomNotification_Users_UpdatedByUserId",
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
                    { new Guid("633c43f5-4d2c-49ed-9dc4-bd0cb6dd0e1c"), "Furniture" },
                    { new Guid("85c8daec-efae-4f5e-8728-2a569ebd9708"), "Clothing" },
                    { new Guid("8d8a0ac0-4984-4fa7-8776-7f22bc858a7e"), "Electronics" },
                    { new Guid("8f5aa47f-c794-47bd-9986-b0943c88e199"), "White Goods" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomNotification_CreatedByUserId",
                table: "CustomNotification",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomNotification_UpdatedByUserId",
                table: "CustomNotification",
                column: "UpdatedByUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomNotification");

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

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("49825113-90a9-498b-b2ca-51463a28eed4"), "Electronics" },
                    { new Guid("49827294-0a92-47dc-8cf1-effa5341f766"), "White Goods" },
                    { new Guid("4a434ee2-256b-4113-9677-fb0f695ef634"), "Clothing" },
                    { new Guid("c7271f09-7c1a-4398-9683-945a3169c993"), "Furniture" }
                });
        }
    }
}
