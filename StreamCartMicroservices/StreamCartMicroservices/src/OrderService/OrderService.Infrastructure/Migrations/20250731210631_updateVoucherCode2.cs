using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateVoucherCode2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "voucher_code",
                table: "orders",
                type: "text",
                nullable: true, // Cho phép null
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "voucher_code",
                table: "orders",
                type: "text",
                nullable: false, // Trở lại không cho phép null nếu rollback
                defaultValue: "", // Tránh lỗi rollback
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
