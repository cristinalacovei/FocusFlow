using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FocusFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSpotifyRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SpotifyRefreshToken",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpotifyRefreshToken",
                table: "AspNetUsers");
        }
    }
}
