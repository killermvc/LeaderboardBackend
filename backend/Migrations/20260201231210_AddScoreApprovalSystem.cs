using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leaderboard.Migrations
{
    /// <inheritdoc />
    public partial class AddScoreApprovalSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Scores",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "Scores",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewedById",
                table: "Scores",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Scores",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "GameModerators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameModerators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameModerators_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameModerators_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Scores_ReviewedById",
                table: "Scores",
                column: "ReviewedById");

            migrationBuilder.CreateIndex(
                name: "IX_GameModerators_GameId_UserId",
                table: "GameModerators",
                columns: new[] { "GameId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameModerators_UserId",
                table: "GameModerators",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Scores_Users_ReviewedById",
                table: "Scores",
                column: "ReviewedById",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Scores_Users_ReviewedById",
                table: "Scores");

            migrationBuilder.DropTable(
                name: "GameModerators");

            migrationBuilder.DropIndex(
                name: "IX_Scores_ReviewedById",
                table: "Scores");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Scores");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "Scores");

            migrationBuilder.DropColumn(
                name: "ReviewedById",
                table: "Scores");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Scores");
        }
    }
}
