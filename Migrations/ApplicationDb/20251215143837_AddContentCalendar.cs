using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLOGAURA.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class AddContentCalendar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContentCalendar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: true),
                    PostId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PlannedPublishDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TargetAudience = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EditorUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentCalendar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentCalendar_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentCalendar_ContentType",
                table: "ContentCalendar",
                column: "ContentType");

            migrationBuilder.CreateIndex(
                name: "IX_ContentCalendar_EditorUserId",
                table: "ContentCalendar",
                column: "EditorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentCalendar_PlannedPublishDate",
                table: "ContentCalendar",
                column: "PlannedPublishDate");

            migrationBuilder.CreateIndex(
                name: "IX_ContentCalendar_PostId",
                table: "ContentCalendar",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentCalendar_Status",
                table: "ContentCalendar",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentCalendar");
        }
    }
}
