using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFieldReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Reviews");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrls",
                table: "Reviews",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrls",
                table: "Reviews");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Reviews",
                type: "text",
                nullable: true);
        }
    }
}
