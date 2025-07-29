using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shop_vouchers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "shop_vouchers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    available_quantity = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    max_value = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    min_order_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    used_quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    value = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shop_vouchers", x => x.id);
                    table.ForeignKey(
                        name: "fk_shop_vouchers_shops",
                        column: x => x.shop_id,
                        principalTable: "shops",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_shop_vouchers_active_lookup",
                table: "shop_vouchers",
                columns: new[] { "shop_id", "is_active", "start_date", "end_date" });

            migrationBuilder.CreateIndex(
                name: "ix_shop_vouchers_code",
                table: "shop_vouchers",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_shop_vouchers_end_date",
                table: "shop_vouchers",
                column: "end_date");

            migrationBuilder.CreateIndex(
                name: "ix_shop_vouchers_is_active",
                table: "shop_vouchers",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_shop_vouchers_shop_id",
                table: "shop_vouchers",
                column: "shop_id");

            migrationBuilder.CreateIndex(
                name: "ix_shop_vouchers_start_date",
                table: "shop_vouchers",
                column: "start_date");

            migrationBuilder.CreateIndex(
                name: "ix_shop_vouchers_type",
                table: "shop_vouchers",
                column: "type");
        }
    }
}
