using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSlotToFlashSale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Flash-Sales_ProductID",
                table: "Flash-Sales");

            migrationBuilder.AlterColumn<bool>(
                name: "NotificationSent",
                table: "Flash-Sales",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<int>(
                name: "Slot",
                table: "Flash-Sales",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_FlashSales_Product_Variant_Time",
                table: "Flash-Sales",
                columns: new[] { "ProductID", "VariantID", "StartTime", "EndTime" });

            migrationBuilder.CreateIndex(
                name: "IX_FlashSales_Slot",
                table: "Flash-Sales",
                column: "Slot");

            migrationBuilder.CreateIndex(
                name: "IX_FlashSales_Slot_StartTime_EndTime",
                table: "Flash-Sales",
                columns: new[] { "Slot", "StartTime", "EndTime" });

            migrationBuilder.CreateIndex(
                name: "IX_FlashSales_TimeRange",
                table: "Flash-Sales",
                columns: new[] { "StartTime", "EndTime" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_FlashSales_Slot",
                table: "Flash-Sales",
                sql: "\"Slot\" >= 1 AND \"Slot\" <= 8");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FlashSales_Product_Variant_Time",
                table: "Flash-Sales");

            migrationBuilder.DropIndex(
                name: "IX_FlashSales_Slot",
                table: "Flash-Sales");

            migrationBuilder.DropIndex(
                name: "IX_FlashSales_Slot_StartTime_EndTime",
                table: "Flash-Sales");

            migrationBuilder.DropIndex(
                name: "IX_FlashSales_TimeRange",
                table: "Flash-Sales");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FlashSales_Slot",
                table: "Flash-Sales");

            migrationBuilder.DropColumn(
                name: "Slot",
                table: "Flash-Sales");

            migrationBuilder.AlterColumn<bool>(
                name: "NotificationSent",
                table: "Flash-Sales",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Flash-Sales_ProductID",
                table: "Flash-Sales",
                column: "ProductID");
        }
    }
}
