using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLOGAURA.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class AddReels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    EventPostId = table.Column<int>(type: "int", nullable: true),
                    Caption = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    VideoPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ThumbnailPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reels_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Reels_Posts_EventPostId",
                        column: x => x.EventPostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ReelComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReelId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReelComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReelComments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReelComments_Reels_ReelId",
                        column: x => x.ReelId,
                        principalTable: "Reels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReelLikes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReelId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReelLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReelLikes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReelLikes_Reels_ReelId",
                        column: x => x.ReelId,
                        principalTable: "Reels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReelComments_ReelId",
                table: "ReelComments",
                column: "ReelId");

            migrationBuilder.CreateIndex(
                name: "IX_ReelComments_UserId",
                table: "ReelComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReelLikes_ReelId",
                table: "ReelLikes",
                column: "ReelId");

            migrationBuilder.CreateIndex(
                name: "IX_ReelLikes_UserId",
                table: "ReelLikes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reels_EventPostId",
                table: "Reels",
                column: "EventPostId");

            migrationBuilder.CreateIndex(
                name: "IX_Reels_UserId",
                table: "Reels",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReelComments");

            migrationBuilder.DropTable(
                name: "ReelLikes");

            migrationBuilder.DropTable(
                name: "Reels");
        }
    }
}
