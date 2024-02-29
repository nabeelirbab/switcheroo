using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    public partial class IndexesInItemsAndDismissedItems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Items_IsHidden",
                table: "Items",
                column: "IsHidden");

            migrationBuilder.CreateIndex(
                name: "IX_DismissedItem_CreatedByUserId_TargetItemId",
                table: "DismissedItem",
                columns: new[] { "CreatedByUserId", "TargetItemId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Items_IsHidden",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_DismissedItem_CreatedByUserId_TargetItemId",
                table: "DismissedItem");



        }
    }
}
