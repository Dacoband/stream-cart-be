using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialProductSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    base_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    discount_price = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    stock_quantity = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    weight = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    dimensions = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    has_variant = table.Column<bool>(type: "boolean", nullable: false),
                    quantity_sold = table.Column<int>(type: "integer", nullable: false),
                    shop_id = table.Column<Guid>(type: "uuid", nullable: true),
                    livestream_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_products_category_id",
                table: "products",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_is_active",
                table: "products",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_products_livestream_id",
                table: "products",
                column: "livestream_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_quantity_sold",
                table: "products",
                column: "quantity_sold");

            migrationBuilder.CreateIndex(
                name: "ix_products_shop_id",
                table: "products",
                column: "shop_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_sku",
                table: "products",
                column: "sku");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "products");
        }
    }
}
