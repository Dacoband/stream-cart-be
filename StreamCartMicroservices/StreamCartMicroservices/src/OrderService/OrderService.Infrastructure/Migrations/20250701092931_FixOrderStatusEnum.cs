using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixOrderStatusEnum : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- Tạo enum mới không có 'Returned'
                CREATE TYPE order_status_new AS ENUM ('Pending', 'Processing', 'Shipped', 'Delivered', 'Cancelled', 'Waiting');

                -- Đổi cột order_status sang enum mới
                ALTER TABLE ""orders""
                ALTER COLUMN ""order_status"" TYPE order_status_new
                USING ""order_status""::text::order_status_new;

                -- Xoá enum cũ
                DROP TYPE order_status;

                -- Đổi tên enum mới thành enum cũ
                ALTER TYPE order_status_new RENAME TO order_status;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- Khôi phục enum cũ
                CREATE TYPE order_status_old AS ENUM ('Pending', 'Processing', 'Shipped', 'Delivered', 'Cancelled', 'Returned');

                ALTER TABLE ""orders""
                ALTER COLUMN ""order_status"" TYPE order_status_old
                USING ""order_status""::text::order_status_old;

                DROP TYPE order_status;

                ALTER TYPE order_status_old RENAME TO order_status;
            ");
        }
    }
}
