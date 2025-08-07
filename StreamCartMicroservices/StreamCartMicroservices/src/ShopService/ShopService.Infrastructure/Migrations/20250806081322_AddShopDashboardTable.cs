using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using ShopService.Domain.Entities;

#nullable disable

namespace ShopService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShopDashboardTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "shop_dashboards",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    to_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    period_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    total_livestream = table.Column<int>(type: "integer", nullable: false),
                    total_livestream_duration = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    total_livestream_viewers = table.Column<int>(type: "integer", nullable: false),
                    total_revenue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    order_in_livestream = table.Column<int>(type: "integer", nullable: false),
                    total_order = table.Column<int>(type: "integer", nullable: false),
                    complete_order_count = table.Column<int>(type: "integer", nullable: false),
                    refund_order_count = table.Column<int>(type: "integer", nullable: false),
                    processing_order_count = table.Column<int>(type: "integer", nullable: false),
                    canceled_order_count = table.Column<int>(type: "integer", nullable: false),
                    top_order_products = table.Column<List<TopProductInfo>>(type: "jsonb", nullable: false),
                    top_ai_recommended_products = table.Column<List<TopProductInfo>>(type: "jsonb", nullable: false),
                    repeat_customer_count = table.Column<int>(type: "integer", nullable: false),
                    new_customer_count = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shop_dashboards", x => x.id);
                    table.ForeignKey(
                        name: "fk_shop_dashboards_shops",
                        column: x => x.shop_id,
                        principalTable: "shops",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_shop_dashboards_period_lookup",
                table: "shop_dashboards",
                columns: new[] { "shop_id", "from_time", "to_time", "period_type" });

            migrationBuilder.CreateIndex(
                name: "ix_shop_dashboards_shop_id",
                table: "shop_dashboards",
                column: "shop_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shop_dashboards");
        }
    }
}
