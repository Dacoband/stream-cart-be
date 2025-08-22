using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LivestreamService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCartLive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LivestreamCarts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LivestreamId = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LivestreamCarts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LivestreamCarts_Livestreams_LivestreamId",
                        column: x => x.LivestreamId,
                        principalTable: "Livestreams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LivestreamCartItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LivestreamCartId = table.Column<Guid>(type: "uuid", nullable: false),
                    LivestreamId = table.Column<Guid>(type: "uuid", nullable: false),
                    LivestreamProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VariantId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ProductName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ShopId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShopName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    LivestreamPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    OriginalPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Stock = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    PrimaryImage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ProductStatus = table.Column<bool>(type: "boolean", nullable: false),
                    Attributes = table.Column<Dictionary<string, string>>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LivestreamCartItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LivestreamCartItems_LivestreamCarts_LivestreamCartId",
                        column: x => x.LivestreamCartId,
                        principalTable: "LivestreamCarts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LivestreamCartItems_LivestreamProducts_LivestreamProductId",
                        column: x => x.LivestreamProductId,
                        principalTable: "LivestreamProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LivestreamCartItems_LivestreamCartId",
                table: "LivestreamCartItems",
                column: "LivestreamCartId");

            migrationBuilder.CreateIndex(
                name: "IX_LivestreamCartItems_LivestreamCartId_LivestreamProductId_Va~",
                table: "LivestreamCartItems",
                columns: new[] { "LivestreamCartId", "LivestreamProductId", "VariantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LivestreamCartItems_LivestreamProductId",
                table: "LivestreamCartItems",
                column: "LivestreamProductId");

            migrationBuilder.CreateIndex(
                name: "IX_LivestreamCarts_ExpiresAt",
                table: "LivestreamCarts",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_LivestreamCarts_LivestreamId_ViewerId",
                table: "LivestreamCarts",
                columns: new[] { "LivestreamId", "ViewerId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LivestreamCartItems");

            migrationBuilder.DropTable(
                name: "LivestreamCarts");
        }
    }
}
