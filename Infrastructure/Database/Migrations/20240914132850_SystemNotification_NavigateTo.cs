using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    public partial class SystemNotification_NavigateTo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddColumn<string>(
                name: "NavigateTo",
                table: "SystemNotification",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("aa72401a-ef28-497e-9aa7-caadd2a468a0"), "Furniture" },
                    { new Guid("bc2cb9ec-f77b-4d1a-8af4-4e964c0f3edd"), "White Goods" },
                    { new Guid("d6b8f176-f115-4f52-8f7a-854cacfa8540"), "Electronics" },
                    { new Guid("d960610a-65a6-451c-a041-fb97ba6adef1"), "Clothing" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("aa72401a-ef28-497e-9aa7-caadd2a468a0"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("bc2cb9ec-f77b-4d1a-8af4-4e964c0f3edd"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("d6b8f176-f115-4f52-8f7a-854cacfa8540"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("d960610a-65a6-451c-a041-fb97ba6adef1"));

            migrationBuilder.DropColumn(
                name: "NavigateTo",
                table: "SystemNotification");

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
    }
}
