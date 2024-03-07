using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    public partial class RemoveUniqueIndexInOffer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Offers_SourceItemId_TargetItemId",
                table: "Offers");

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
                    { new Guid("49825113-90a9-498b-b2ca-51463a28eed4"), "Electronics" },
                    { new Guid("49827294-0a92-47dc-8cf1-effa5341f766"), "White Goods" },
                    { new Guid("4a434ee2-256b-4113-9677-fb0f695ef634"), "Clothing" },
                    { new Guid("c7271f09-7c1a-4398-9683-945a3169c993"), "Furniture" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Offers_SourceItemId_TargetItemId",
                table: "Offers",
                columns: new[] { "SourceItemId", "TargetItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_DismissedItem_SourceItemId_TargetItemId",
                table: "DismissedItem",
                columns: new[] { "SourceItemId", "TargetItemId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Offers_SourceItemId_TargetItemId",
                table: "Offers");

            migrationBuilder.DropIndex(
                name: "IX_DismissedItem_SourceItemId_TargetItemId",
                table: "DismissedItem");

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
                name: "IX_Offers_SourceItemId_TargetItemId",
                table: "Offers",
                columns: new[] { "SourceItemId", "TargetItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DismissedItem_SourceItemId",
                table: "DismissedItem",
                column: "SourceItemId");
        }
    }
}
