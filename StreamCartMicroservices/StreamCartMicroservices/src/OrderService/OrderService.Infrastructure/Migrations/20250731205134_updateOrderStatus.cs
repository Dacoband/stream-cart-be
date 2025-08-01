using Microsoft.EntityFrameworkCore.Migrations;
using OrderService.Domain.Enums;

#nullable disable

namespace OrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateOrderStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
    ALTER TABLE orders
    ALTER COLUMN payment_status
    TYPE payment_status
    USING payment_status::payment_status;
");

            migrationBuilder.AlterColumn<OrderStatus>(
                name: "order_status",
                table: "orders",
                type: "order_status",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "payment_status",
                table: "orders",
                type: "text",
                nullable: false,
                oldClrType: typeof(PaymentStatus),
                oldType: "payment_status");

            migrationBuilder.AlterColumn<string>(
                name: "order_status",
                table: "orders",
                type: "text",
                nullable: false,
                oldClrType: typeof(OrderStatus),
                oldType: "order_status");
        }
    }
}
