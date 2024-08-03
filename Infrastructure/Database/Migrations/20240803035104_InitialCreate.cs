using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateTable(
                name: "Feedback",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ArchivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedback", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Feedback_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Feedback_Users_UpdatedByUserId",
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
                    { new Guid("092650bd-6cc1-4a3c-855b-d553024a1f61"), "White Goods" },
                    { new Guid("67942b9c-db0d-48ca-968b-646dc52caa12"), "Furniture" },
                    { new Guid("79e7215b-90f6-414e-8d85-cad8a5e54fc5"), "Clothing" },
                    { new Guid("7dcb12a0-98b7-4df3-a3ac-1f3d6d15d2fe"), "Electronics" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_CreatedByUserId",
                table: "Feedback",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_UpdatedByUserId",
                table: "Feedback",
                column: "UpdatedByUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Feedback");

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
    }
}
