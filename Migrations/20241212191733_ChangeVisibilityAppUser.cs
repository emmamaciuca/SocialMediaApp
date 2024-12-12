using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMediaApp.Migrations
{
    /// <inheritdoc />
    public partial class ChangeVisibilityAppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "visibility",
                table: "AspNetUsers",
                newName: "Visibility");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Visibility",
                table: "AspNetUsers",
                newName: "visibility");
        }
    }
}
