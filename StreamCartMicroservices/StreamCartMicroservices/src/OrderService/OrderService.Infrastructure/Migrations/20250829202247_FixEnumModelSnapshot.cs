using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixEnumModelSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ✅ Không thay đổi database - chỉ fix model snapshot
            // Database đã có enum types đúng rồi, chỉ cần EF nhận biết

            // ✅ Đảm bảo enum types có đầy đủ values
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    -- Update order_status enum nếu thiếu values
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_enum 
                        WHERE enumtypid = (SELECT oid FROM pg_type WHERE typname = 'order_status') 
                        AND enumlabel = 'completed'
                    ) THEN
                        ALTER TYPE order_status ADD VALUE IF NOT EXISTS 'completed';
                    END IF;

                    IF NOT EXISTS (
                        SELECT 1 FROM pg_enum 
                        WHERE enumtypid = (SELECT oid FROM pg_type WHERE typname = 'order_status') 
                        AND enumlabel = 'packed'
                    ) THEN
                        ALTER TYPE order_status ADD VALUE IF NOT EXISTS 'packed';
                    END IF;

                    IF NOT EXISTS (
                        SELECT 1 FROM pg_enum 
                        WHERE enumtypid = (SELECT oid FROM pg_type WHERE typname = 'order_status') 
                        AND enumlabel = 'on_delivere'
                    ) THEN
                        ALTER TYPE order_status ADD VALUE IF NOT EXISTS 'on_delivere';
                    END IF;

                    IF NOT EXISTS (
                        SELECT 1 FROM pg_enum 
                        WHERE enumtypid = (SELECT oid FROM pg_type WHERE typname = 'order_status') 
                        AND enumlabel = 'returning'
                    ) THEN
                        ALTER TYPE order_status ADD VALUE IF NOT EXISTS 'returning';
                    END IF;

                    IF NOT EXISTS (
                        SELECT 1 FROM pg_enum 
                        WHERE enumtypid = (SELECT oid FROM pg_type WHERE typname = 'order_status') 
                        AND enumlabel = 'refunded'
                    ) THEN
                        ALTER TYPE order_status ADD VALUE IF NOT EXISTS 'refunded';
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No action needed for down
        }
    }
}