using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    public partial class RemoveIndexesFromDismissedItems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DismissedItem_SourceItemId_TargetItemId",
                table: "DismissedItem");

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("32157d3a-e333-48ea-ab53-902a55b88046"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("410278a9-c4b6-4011-8119-3bfcc01a0794"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("5e6d9862-a372-4d8d-be74-bd2da92d68bb"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("c93f9f89-77f8-4ffb-b262-d44d426a4c55"));

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("139150e4-c73f-4f79-819b-9e0a76c7084c"), "Furniture" },
                    { new Guid("5e3b6ed2-be80-4d1c-9a26-aa9a7d0a0847"), "Clothing" },
                    { new Guid("e4cebf0d-dd5a-402b-a743-f74517f94dbb"), "Electronics" },
                    { new Guid("ecfeaadd-55a1-4d13-8ec8-185c5852502f"), "White Goods" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DismissedItem_SourceItemId",
                table: "DismissedItem",
                column: "SourceItemId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DismissedItem_SourceItemId",
                table: "DismissedItem");

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("139150e4-c73f-4f79-819b-9e0a76c7084c"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("5e3b6ed2-be80-4d1c-9a26-aa9a7d0a0847"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("e4cebf0d-dd5a-402b-a743-f74517f94dbb"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("ecfeaadd-55a1-4d13-8ec8-185c5852502f"));

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("32157d3a-e333-48ea-ab53-902a55b88046"), "Furniture" },
                    { new Guid("410278a9-c4b6-4011-8119-3bfcc01a0794"), "Clothing" },
                    { new Guid("5e6d9862-a372-4d8d-be74-bd2da92d68bb"), "Electronics" },
                    { new Guid("c93f9f89-77f8-4ffb-b262-d44d426a4c55"), "White Goods" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DismissedItem_SourceItemId_TargetItemId",
                table: "DismissedItem",
                columns: new[] { "SourceItemId", "TargetItemId" },
                unique: true);
        }
    }
}
