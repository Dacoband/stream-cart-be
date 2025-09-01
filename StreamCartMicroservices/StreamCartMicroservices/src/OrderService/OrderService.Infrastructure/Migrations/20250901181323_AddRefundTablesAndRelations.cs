using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRefundTablesAndRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "refund_requests",
                columns: table => new
                {
                    refund_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tracking_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    requested_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requested_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    processed_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    refund_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    shipping_fee = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refund_requests", x => x.refund_id);
                    table.ForeignKey(
                        name: "fk_refund_requests_orders",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refund_details",
                columns: table => new
                {
                    refund_detail_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_item = table.Column<Guid>(type: "uuid", nullable: false),
                    refund_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    image_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refund_details", x => x.refund_detail_id);
                    table.ForeignKey(
                        name: "fk_refund_details_order_items",
                        column: x => x.order_item,
                        principalTable: "order_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_refund_details_refund_requests",
                        column: x => x.refund_request_id,
                        principalTable: "refund_requests",
                        principalColumn: "refund_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_refund_details_order_item_id",
                table: "refund_details",
                column: "order_item");

            migrationBuilder.CreateIndex(
                name: "ix_refund_details_refund_request_id",
                table: "refund_details",
                column: "refund_request_id");

            migrationBuilder.CreateIndex(
                name: "ix_refund_requests_order_id",
                table: "refund_requests",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_refund_requests_requested_at",
                table: "refund_requests",
                column: "requested_at");

            migrationBuilder.CreateIndex(
                name: "ix_refund_requests_requested_by_user_id",
                table: "refund_requests",
                column: "requested_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_refund_requests_status",
                table: "refund_requests",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "refund_details");

            migrationBuilder.DropTable(
                name: "refund_requests");
        }
    }
}
