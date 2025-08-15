using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LivestreamService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialLivestreamSchema1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LivestreamHostId",
                table: "Livestreams",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalPrice",
                table: "LivestreamProducts",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LivestreamHostId",
                table: "Livestreams");

            migrationBuilder.DropColumn(
                name: "OriginalPrice",
                table: "LivestreamProducts");
        }
    }
}
