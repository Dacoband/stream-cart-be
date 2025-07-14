using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LivestreamService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialLivestreamSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Livestreams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShopId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduledStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActualStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualEndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<bool>(type: "boolean", nullable: false),
                    StreamKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PlaybackUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    MaxViewer = table.Column<int>(type: "integer", nullable: true),
                    ApprovalStatusContent = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovalDateContent = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsPromoted = table.Column<bool>(type: "boolean", nullable: false),
                    Tags = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    LivekitRoomId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Livestreams", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Livestreams_ScheduledStartTime",
                table: "Livestreams",
                column: "ScheduledStartTime");

            migrationBuilder.CreateIndex(
                name: "IX_Livestreams_SellerId",
                table: "Livestreams",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_Livestreams_ShopId",
                table: "Livestreams",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_Livestreams_Status",
                table: "Livestreams",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Livestreams");
        }
    }
}
