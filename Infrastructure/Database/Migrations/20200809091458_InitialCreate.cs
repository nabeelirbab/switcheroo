using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Infrastructure.Database.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserName = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    PasswordHash = table.Column<string>(nullable: true),
                    SecurityStamp = table.Column<string>(nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    LockoutEnabled = table.Column<bool>(nullable: false),
                    AccessFailedCount = table.Column<int>(nullable: false),
                    FirstName = table.Column<string>(nullable: false),
                    LastName = table.Column<string>(nullable: false),
                    Mobile = table.Column<string>(nullable: true),
                    Gender = table.Column<string>(nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "Date", nullable: true),
                    Distance = table.Column<int>(nullable: true),
                    Blurb = table.Column<string>(nullable: true),
                    AvatarUrl = table.Column<string>(nullable: true),
                    IsMatchNotificationsEnabled = table.Column<bool>(nullable: false),
                    IsChatNotificationsEnabled = table.Column<bool>(nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                    ArchivedAt = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<Guid>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                    ArchivedAt = table.Column<DateTimeOffset>(nullable: true),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Categories_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                    ArchivedAt = table.Column<DateTimeOffset>(nullable: true),
                    Title = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    AskingPrice = table.Column<decimal>(nullable: false),
                    FlexibilityRange = table.Column<int>(nullable: true),
                    IsHidden = table.Column<bool>(nullable: false),
                    IsSwapOnly = table.Column<bool>(nullable: false),
                    Latitude = table.Column<decimal>(nullable: true),
                    Longitude = table.Column<decimal>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Items_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserClaims_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(nullable: false),
                    ProviderKey = table.Column<string>(nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_UserLogins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(nullable: false),
                    RoleId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(nullable: false),
                    LoginProvider = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_UserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserVerificationCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                    ArchivedAt = table.Column<DateTimeOffset>(nullable: true),
                    Email = table.Column<string>(nullable: false),
                    SixDigitCode = table.Column<string>(nullable: false),
                    EmailConfirmationToken = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVerificationCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserVerificationCodes_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserVerificationCodes_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DismissedItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                    ArchivedAt = table.Column<DateTimeOffset>(nullable: true),
                    SourceItemId = table.Column<Guid>(nullable: false),
                    TargetItemId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DismissedItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DismissedItem_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DismissedItem_Items_SourceItemId",
                        column: x => x.SourceItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DismissedItem_Items_TargetItemId",
                        column: x => x.TargetItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DismissedItem_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ItemId = table.Column<Guid>(nullable: false),
                    CategoryId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemCategories_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Url = table.Column<string>(nullable: false),
                    ItemId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemImages_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Offers",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                    ArchivedAt = table.Column<DateTimeOffset>(nullable: true),
                    SourceItemId = table.Column<Guid>(nullable: false),
                    TargetItemId = table.Column<Guid>(nullable: false),
                    Cash = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Offers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Offers_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Offers_Items_SourceItemId",
                        column: x => x.SourceItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Offers_Items_TargetItemId",
                        column: x => x.TargetItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Offers_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                    ArchivedAt = table.Column<DateTimeOffset>(nullable: true),
                    OfferId = table.Column<Guid>(nullable: false),
                    MessageText = table.Column<string>(nullable: false),
                    MessageReadAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Messages_Offers_OfferId",
                        column: x => x.OfferId,
                        principalTable: "Offers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Messages_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("c63a092d-e08e-4962-8d82-e0c10212831b"), "78c06b84-13a4-428e-aa66-a3a3fa173c9a", "Administrator", "administrator" },
                    { new Guid("c63a092d-e08e-4962-8d82-e0c10212833b"), "848a3c1d-6339-4fbf-b4e1-b4616c8b67d3", "Test", "test" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AccessFailedCount", "ArchivedAt", "AvatarUrl", "Blurb", "ConcurrencyStamp", "CreatedAt", "DateOfBirth", "Distance", "Email", "EmailConfirmed", "FirstName", "Gender", "IsChatNotificationsEnabled", "IsMatchNotificationsEnabled", "LastName", "LockoutEnabled", "LockoutEnd", "Mobile", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UpdatedAt", "UserName" },
                values: new object[,]
                {
                    { new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e575"), 0, null, "https://picsum.photos/300/300", "I love things. Have too many of them though. Keen to swap yo!", "a01912d7-98ad-43a5-a469-1169685f577a", new DateTimeOffset(new DateTime(2020, 8, 9, 9, 14, 57, 927, DateTimeKind.Unspecified).AddTicks(8770), new TimeSpan(0, 0, 0, 0, 0)), null, null, "admin@switcherooapp.com.au", true, "Admin", null, true, true, "Admin", false, null, null, "ADMIN@SWITCHEROOAPP.COM.AU", "ADMIN@SWITCHEROOAPP.COM.AU", "AQAAAAEAACcQAAAAEAHNLkDE0Ln04kdcaqZVhfUsIHEP6yrvW5KXevuhO8MWTZ2QYHFHGmPG7z/tgenWsA==", null, false, "", false, new DateTimeOffset(new DateTime(2020, 8, 9, 9, 14, 57, 927, DateTimeKind.Unspecified).AddTicks(8770), new TimeSpan(0, 0, 0, 0, 0)), "admin@switcherooapp.com.au" },
                    { new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e576"), 0, null, "https://picsum.photos/300/300", "Swap swap swapperoooooo yew yew yew!", "076eb480-1bed-459d-be4a-5e3b2c7af137", new DateTimeOffset(new DateTime(2020, 8, 9, 9, 14, 57, 927, DateTimeKind.Unspecified).AddTicks(8770), new TimeSpan(0, 0, 0, 0, 0)), null, null, "test@switcherooapp.com.au", true, "Test", null, true, true, "User", false, null, null, "TEST@SWITCHEROOAPP.COM.AU", "TEST@SWITCHEROOAPP.COM.AU", "AQAAAAEAACcQAAAAEBi/nraAEFbaP8lcaerwSXhKCcEex67sGMduQPvnrMAaYnN7eEvZ7LslXIsi/YnVfw==", null, false, "", false, new DateTimeOffset(new DateTime(2020, 8, 9, 9, 14, 57, 927, DateTimeKind.Unspecified).AddTicks(8770), new TimeSpan(0, 0, 0, 0, 0)), "test@switcherooapp.com.au" }
                });

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

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e575"), new Guid("c63a092d-e08e-4962-8d82-e0c10212831b") },
                    { new Guid("a18be9c0-aa65-4af8-bd17-00bd9344e576"), new Guid("c63a092d-e08e-4962-8d82-e0c10212831b") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CreatedByUserId",
                table: "Categories",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_UpdatedByUserId",
                table: "Categories",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DismissedItem_CreatedByUserId",
                table: "DismissedItem",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DismissedItem_TargetItemId",
                table: "DismissedItem",
                column: "TargetItemId");

            migrationBuilder.CreateIndex(
                name: "IX_DismissedItem_UpdatedByUserId",
                table: "DismissedItem",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DismissedItem_SourceItemId_TargetItemId",
                table: "DismissedItem",
                columns: new[] { "SourceItemId", "TargetItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemCategories_ItemId",
                table: "ItemCategories",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemCategories_CategoryId_ItemId",
                table: "ItemCategories",
                columns: new[] { "CategoryId", "ItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemImages_ItemId",
                table: "ItemImages",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_CreatedByUserId",
                table: "Items",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_UpdatedByUserId",
                table: "Items",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_CreatedByUserId",
                table: "Messages",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_OfferId",
                table: "Messages",
                column: "OfferId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_UpdatedByUserId",
                table: "Messages",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Offers_CreatedByUserId",
                table: "Offers",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Offers_TargetItemId",
                table: "Offers",
                column: "TargetItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Offers_UpdatedByUserId",
                table: "Offers",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Offers_SourceItemId_TargetItemId",
                table: "Offers",
                columns: new[] { "SourceItemId", "TargetItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaims_RoleId",
                table: "RoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Roles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserClaims_UserId",
                table: "UserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogins_UserId",
                table: "UserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Users",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserVerificationCodes_CreatedByUserId",
                table: "UserVerificationCodes",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserVerificationCodes_EmailConfirmationToken",
                table: "UserVerificationCodes",
                column: "EmailConfirmationToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserVerificationCodes_UpdatedByUserId",
                table: "UserVerificationCodes",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserVerificationCodes_Email_SixDigitCode",
                table: "UserVerificationCodes",
                columns: new[] { "Email", "SixDigitCode" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DismissedItem");

            migrationBuilder.DropTable(
                name: "ItemCategories");

            migrationBuilder.DropTable(
                name: "ItemImages");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "RoleClaims");

            migrationBuilder.DropTable(
                name: "UserClaims");

            migrationBuilder.DropTable(
                name: "UserLogins");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "UserTokens");

            migrationBuilder.DropTable(
                name: "UserVerificationCodes");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Offers");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
