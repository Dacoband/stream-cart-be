using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductPhysicalAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "dimensions",
                table: "products");

            migrationBuilder.AddColumn<decimal>(
                name: "height",
                table: "products",
                type: "numeric(10,2)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "length",
                table: "products",
                type: "numeric(10,2)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "width",
                table: "products",
                type: "numeric(10,2)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "height",
                table: "products");

            migrationBuilder.DropColumn(
                name: "length",
                table: "products");

            migrationBuilder.DropColumn(
                name: "width",
                table: "products");

            migrationBuilder.AddColumn<string>(
                name: "dimensions",
                table: "products",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
