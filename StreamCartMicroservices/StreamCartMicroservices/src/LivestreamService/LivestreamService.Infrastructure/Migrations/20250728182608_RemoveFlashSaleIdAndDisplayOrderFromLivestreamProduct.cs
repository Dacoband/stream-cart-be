using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LivestreamService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFlashSaleIdAndDisplayOrderFromLivestreamProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "LivestreamProducts");

            migrationBuilder.DropColumn(
                name: "FlashSaleId",
                table: "LivestreamProducts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "LivestreamProducts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "FlashSaleId",
                table: "LivestreamProducts",
                type: "uuid",
                nullable: true);
        }
    }
}
