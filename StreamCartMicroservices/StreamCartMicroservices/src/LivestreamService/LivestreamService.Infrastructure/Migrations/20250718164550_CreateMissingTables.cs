using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LivestreamService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateMissingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LivestreamProducts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LivestreamId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    VariantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FlashSaleId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsPin = table.Column<bool>(type: "boolean", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Stock = table.Column<int>(type: "integer", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LivestreamProducts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StreamEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LivestreamId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LivestreamProductId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Payload = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreamEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StreamViews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LivestreamId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreamViews", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LivestreamProducts_IsPin",
                table: "LivestreamProducts",
                column: "IsPin");

            migrationBuilder.CreateIndex(
                name: "IX_LivestreamProducts_LivestreamId",
                table: "LivestreamProducts",
                column: "LivestreamId");

            migrationBuilder.CreateIndex(
                name: "IX_LivestreamProducts_ProductId",
                table: "LivestreamProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StreamEvents_EventType",
                table: "StreamEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_StreamEvents_LivestreamId",
                table: "StreamEvents",
                column: "LivestreamId");

            migrationBuilder.CreateIndex(
                name: "IX_StreamEvents_UserId",
                table: "StreamEvents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StreamViews_LivestreamId",
                table: "StreamViews",
                column: "LivestreamId");

            migrationBuilder.CreateIndex(
                name: "IX_StreamViews_UserId",
                table: "StreamViews",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LivestreamProducts");

            migrationBuilder.DropTable(
                name: "StreamEvents");

            migrationBuilder.DropTable(
                name: "StreamViews");
        }
    }
}
