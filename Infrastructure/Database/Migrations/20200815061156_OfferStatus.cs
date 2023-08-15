using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Infrastructure.Database.Migrations
{
    public partial class OfferStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("116dcda4-309c-4ef7-a5c5-e8393728fdaf"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("473cacff-94ca-411b-89e7-7e69c6c1655b"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("abdc2d51-71e5-4fe8-83a3-ae19398b8743"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("c03515a0-8077-40b2-9864-4956d9786415"));

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:offer_status", "initiated,discussing,confirmed,cancelled");

            migrationBuilder.AddColumn<int>(
                name: "SourceStatus",
                table: "Offers",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TargetStatus",
                table: "Offers",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "ArchivedAt", "CreatedAt", "CreatedByUserId", "Name", "UpdatedAt", "UpdatedByUserId" },
                values: new object[,]
                {
                    { new Guid("47783eaf-8236-48bd-9df6-df2c540d4464"), null, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e575"), "Electronics", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e575") },
                    { new Guid("04e1cc13-aef8-4297-a8ed-2b6c55c7e277"), null, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e575"), "White Goods", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e575") },
                    { new Guid("11e6234c-8a6c-494d-9f7d-75e453162101"), null, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e575"), "Clothing", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e575") },
                    { new Guid("a7c76613-85d8-4707-bf58-c420cb29043d"), null, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e575"), "Furniture", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e575") }
                });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c63a092d-e08e-4962-8d82-e0c10212831b"),
                column: "ConcurrencyStamp",
                value: "74b63bb6-a865-472f-8015-191fb57a3839");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c63a092d-e08e-4962-8d82-e0c10212833b"),
                column: "ConcurrencyStamp",
                value: "21eb6acf-ce8f-42f8-93c7-6ff390b3d9dc");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e575"),
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { "617293a5-357c-44ee-83c9-8f320ad84066", new DateTimeOffset(new DateTime(2020, 8, 15, 6, 11, 56, 74, DateTimeKind.Unspecified).AddTicks(2304), new TimeSpan(0, 0, 0, 0, 0)), "AQAAAAEAACcQAAAAEL+KC2Ikv9s4Q7W92dgtTAZPbm1tDiJeR1/5+aOLeXFBHdKuDeoLgRzdSz+MqoaRyg==", new DateTimeOffset(new DateTime(2020, 8, 15, 6, 11, 56, 74, DateTimeKind.Unspecified).AddTicks(2304), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e576"),
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { "788054a0-623a-4cf7-9556-dffc425ba978", new DateTimeOffset(new DateTime(2020, 8, 15, 6, 11, 56, 74, DateTimeKind.Unspecified).AddTicks(2304), new TimeSpan(0, 0, 0, 0, 0)), "AQAAAAEAACcQAAAAEIKcuCoW7hQjGLQvX18rYAxwgcNf6Dg9+K1hDWjW+6qDGBuuLekOQWi9qoPYJauvsw==", new DateTimeOffset(new DateTime(2020, 8, 15, 6, 11, 56, 74, DateTimeKind.Unspecified).AddTicks(2304), new TimeSpan(0, 0, 0, 0, 0)) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("04e1cc13-aef8-4297-a8ed-2b6c55c7e277"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("11e6234c-8a6c-494d-9f7d-75e453162101"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("47783eaf-8236-48bd-9df6-df2c540d4464"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("a7c76613-85d8-4707-bf58-c420cb29043d"));

            migrationBuilder.DropColumn(
                name: "SourceStatus",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "TargetStatus",
                table: "Offers");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:Enum:offer_status", "initiated,discussing,confirmed,cancelled");

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "ArchivedAt", "CreatedAt", "CreatedByUserId", "Name", "UpdatedAt", "UpdatedByUserId" },
                values: new object[,]
                {
                    { new Guid("473cacff-94ca-411b-89e7-7e69c6c1655b"), null, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e575"), "Electronics", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e575") },
                    { new Guid("116dcda4-309c-4ef7-a5c5-e8393728fdaf"), null, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e575"), "White Goods", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e575") },
                    { new Guid("abdc2d51-71e5-4fe8-83a3-ae19398b8743"), null, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e575"), "Clothing", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e575") },
                    { new Guid("c03515a0-8077-40b2-9864-4956d9786415"), null, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e575"), "Furniture", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e575") }
                });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c63a092d-e08e-4962-8d82-e0c10212831b"),
                column: "ConcurrencyStamp",
                value: "78c06b84-13a4-428e-aa66-a3a3fa173c9a");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c63a092d-e08e-4962-8d82-e0c10212833b"),
                column: "ConcurrencyStamp",
                value: "848a3c1d-6339-4fbf-b4e1-b4616c8b67d3");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e575"),
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { "a01912d7-98ad-43a5-a469-1169685f577a", new DateTimeOffset(new DateTime(2020, 8, 9, 9, 14, 57, 927, DateTimeKind.Unspecified).AddTicks(8770), new TimeSpan(0, 0, 0, 0, 0)), "AQAAAAEAACcQAAAAEAHNLkDE0Ln04kdcaqZVhfUsIHEP6yrvW5KXevuhO8MWTZ2QYHFHGmPG7z/tgenWsA==", new DateTimeOffset(new DateTime(2020, 8, 9, 9, 14, 57, 927, DateTimeKind.Unspecified).AddTicks(8770), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e576"),
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { "076eb480-1bed-459d-be4a-5e3b2c7af137", new DateTimeOffset(new DateTime(2020, 8, 9, 9, 14, 57, 927, DateTimeKind.Unspecified).AddTicks(8770), new TimeSpan(0, 0, 0, 0, 0)), "AQAAAAEAACcQAAAAEBi/nraAEFbaP8lcaerwSXhKCcEex67sGMduQPvnrMAaYnN7eEvZ7LslXIsi/YnVfw==", new DateTimeOffset(new DateTime(2020, 8, 9, 9, 14, 57, 927, DateTimeKind.Unspecified).AddTicks(8770), new TimeSpan(0, 0, 0, 0, 0)) });
        }
    }
}
