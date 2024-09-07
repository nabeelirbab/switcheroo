using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    public partial class OfferConfirmationStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("35905850-5431-4043-8a8a-0e712abf7512"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("3abe4745-c436-4356-9419-ba8fc23f497b"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("47c46d5d-6294-4921-a3d3-405adbca8ede"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("6524445a-2c87-483e-aff7-406c03bf0f6c"));

            migrationBuilder.AddColumn<bool>(
                name: "ConfirmedBySourceUser",
                table: "Offers",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ConfirmedByTargetUser",
                table: "Offers",
                type: "boolean",
                nullable: true);

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

        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "ConfirmedBySourceUser",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "ConfirmedByTargetUser",
                table: "Offers");

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("35905850-5431-4043-8a8a-0e712abf7512"), "Electronics" },
                    { new Guid("3abe4745-c436-4356-9419-ba8fc23f497b"), "Clothing" },
                    { new Guid("47c46d5d-6294-4921-a3d3-405adbca8ede"), "Furniture" },
                    { new Guid("6524445a-2c87-483e-aff7-406c03bf0f6c"), "White Goods" }
                });
        }
    }
}
