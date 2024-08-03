using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    public partial class FeedbackStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("092650bd-6cc1-4a3c-855b-d553024a1f61"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("67942b9c-db0d-48ca-968b-646dc52caa12"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("79e7215b-90f6-414e-8d85-cad8a5e54fc5"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("7dcb12a0-98b7-4df3-a3ac-1f3d6d15d2fe"));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Feedback",
                type: "integer",
                nullable: false,
                defaultValue: 0);

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

        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Feedback");

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("092650bd-6cc1-4a3c-855b-d553024a1f61"), "White Goods" },
                    { new Guid("67942b9c-db0d-48ca-968b-646dc52caa12"), "Furniture" },
                    { new Guid("79e7215b-90f6-414e-8d85-cad8a5e54fc5"), "Clothing" },
                    { new Guid("7dcb12a0-98b7-4df3-a3ac-1f3d6d15d2fe"), "Electronics" }
                });
        }
    }
}
