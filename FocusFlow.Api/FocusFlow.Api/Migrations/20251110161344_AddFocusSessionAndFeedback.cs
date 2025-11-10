using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FocusFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFocusSessionAndFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FocusSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IntendedDurationMinutes = table.Column<int>(type: "int", nullable: false),
                    Mood = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AppUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ActivityId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FocusSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FocusSessions_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FocusSessions_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SessionFeedbacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductivityRating = table.Column<int>(type: "int", nullable: false),
                    MusicFeedback = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MusicGenreUsed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FocusSessionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionFeedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionFeedbacks_FocusSessions_FocusSessionId",
                        column: x => x.FocusSessionId,
                        principalTable: "FocusSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FocusSessions_ActivityId",
                table: "FocusSessions",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_FocusSessions_AppUserId",
                table: "FocusSessions",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionFeedbacks_FocusSessionId",
                table: "SessionFeedbacks",
                column: "FocusSessionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionFeedbacks");

            migrationBuilder.DropTable(
                name: "FocusSessions");
        }
    }
}
