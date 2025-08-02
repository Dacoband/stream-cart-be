using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updatevariantdimension : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "height",
                table: "ProductVariants",
                type: "numeric(10,2)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "length",
                table: "ProductVariants",
                type: "numeric(10,2)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "weight",
                table: "ProductVariants",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "width",
                table: "ProductVariants",
                type: "numeric(10,2)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "height",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "length",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "weight",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "width",
                table: "ProductVariants");
        }
    }
}
