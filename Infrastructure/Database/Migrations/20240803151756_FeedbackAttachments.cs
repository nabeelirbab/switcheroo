using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    public partial class FeedbackAttachments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("2900d3fd-b0c6-409c-a2bb-f76411900f69"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("62085349-e04f-480b-ba15-7fb0e55164d8"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("7ab3c845-57cc-4f5e-b05d-260591797f46"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("e0651c23-21d2-4f9c-9133-e64b7a9d5bb9"));

            migrationBuilder.AddColumn<List<string>>(
                name: "Attachments",
                table: "Feedback",
                type: "text[]",
                nullable: true);

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

        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "Attachments",
                table: "Feedback");

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("2900d3fd-b0c6-409c-a2bb-f76411900f69"), "Clothing" },
                    { new Guid("62085349-e04f-480b-ba15-7fb0e55164d8"), "White Goods" },
                    { new Guid("7ab3c845-57cc-4f5e-b05d-260591797f46"), "Electronics" },
                    { new Guid("e0651c23-21d2-4f9c-9133-e64b7a9d5bb9"), "Furniture" }
                });
        }
    }
}
