﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMediaApp.Migrations
{
    /// <inheritdoc />
    public partial class CererePrietenie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Follows",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Follows");
        }
    }
}
