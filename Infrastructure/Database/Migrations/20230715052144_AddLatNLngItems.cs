using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Infrastructure.Database.Migrations
{
    public partial class AddLatNLngItems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.AddColumn<decimal>("Latitude", "Items", nullable: true);
            // migrationBuilder.AddColumn<decimal>("Longitude", "Items", nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropColumn("Latitude", "Items");
            // migrationBuilder.DropColumn("Longitude", "Items");
        }
    }
}
