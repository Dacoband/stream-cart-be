using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePaymentStatusEnumCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tạm thời drop ràng buộc cũ
            migrationBuilder.Sql("ALTER TABLE orders ALTER COLUMN payment_status TYPE text");

            // Cập nhật giá trị enum từ lowercase thành PascalCase
            migrationBuilder.Sql(@"
        UPDATE orders SET payment_status = 'Pending' WHERE payment_status = 'pending';
        UPDATE orders SET payment_status = 'Paid' WHERE payment_status = 'paid';
        UPDATE orders SET payment_status = 'Failed' WHERE payment_status = 'failed';
        UPDATE orders SET payment_status = 'Refunded' WHERE payment_status = 'refunded';
        UPDATE orders SET payment_status = 'PartiallyRefunded' WHERE payment_status = 'partiallyRefunded';
    ");

            // Tạo lại enum type với tên viết hoa
            migrationBuilder.Sql(@"
        DROP TYPE IF EXISTS payment_status CASCADE;
        CREATE TYPE payment_status AS ENUM ('Pending', 'Paid', 'Failed', 'Refunded', 'PartiallyRefunded');
    ");

            // Chuyển đổi cột sang kiểu enum mới
            migrationBuilder.Sql("ALTER TABLE orders ALTER COLUMN payment_status TYPE payment_status USING payment_status::payment_status");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback code để quay lại trạng thái ban đầu
            migrationBuilder.Sql("ALTER TABLE orders ALTER COLUMN payment_status TYPE text");

            migrationBuilder.Sql(@"
        UPDATE orders SET payment_status = 'pending' WHERE payment_status = 'Pending';
        UPDATE orders SET payment_status = 'paid' WHERE payment_status = 'Paid';
        UPDATE orders SET payment_status = 'failed' WHERE payment_status = 'Failed';
        UPDATE orders SET payment_status = 'refunded' WHERE payment_status = 'Refunded';
        UPDATE orders SET payment_status = 'partiallyRefunded' WHERE payment_status = 'PartiallyRefunded';
    ");

            migrationBuilder.Sql(@"
        DROP TYPE IF EXISTS payment_status CASCADE;
        CREATE TYPE payment_status AS ENUM ('pending', 'paid', 'failed', 'refunded', 'partiallyRefunded');
    ");

            migrationBuilder.Sql("ALTER TABLE orders ALTER COLUMN payment_status TYPE payment_status USING payment_status::payment_status");
        }
    }
}
