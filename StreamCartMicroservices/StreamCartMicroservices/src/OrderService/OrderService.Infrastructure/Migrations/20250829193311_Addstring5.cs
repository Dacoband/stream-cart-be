using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Addstring5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ✅ Đảm bảo enum types tồn tại với đúng values
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    -- Tạo order_status enum nếu chưa có hoặc update nếu cần
                    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'order_status') THEN
                        CREATE TYPE order_status AS ENUM (
                            'waiting','pending','processing','shipped','delivered',
                            'cancelled','packed','on_delivere','returning','refunded','completed'
                        );
                    END IF;
                    
                    -- Tạo payment_status enum nếu chưa có
                    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'payment_status') THEN
                        CREATE TYPE payment_status AS ENUM (
                            'pending','paid','failed','refunded','partially_refunded'
                        );
                    END IF;
                END $$;
            ");

            // ✅ Đảm bảo columns đang sử dụng enum type đúng
            migrationBuilder.Sql(@"
                -- Kiểm tra và fix order_status column nếu cần
                DO $$
                BEGIN
                    IF (SELECT data_type FROM information_schema.columns 
                        WHERE table_name = 'orders' AND column_name = 'order_status') != 'USER-DEFINED' THEN
                        
                        -- Nếu column không phải enum, convert nó
                        ALTER TABLE orders 
                        ALTER COLUMN order_status TYPE order_status 
                        USING 
                            CASE LOWER(order_status::text)
                                WHEN 'waiting' THEN 'waiting'::order_status
                                WHEN 'pending' THEN 'pending'::order_status
                                WHEN 'processing' THEN 'processing'::order_status
                                WHEN 'shipped' THEN 'shipped'::order_status
                                WHEN 'delivered' THEN 'delivered'::order_status
                                WHEN 'cancelled' THEN 'cancelled'::order_status
                                WHEN 'packed' THEN 'packed'::order_status
                                WHEN 'on_delivere' THEN 'on_delivere'::order_status
                                WHEN 'returning' THEN 'returning'::order_status
                                WHEN 'refunded' THEN 'refunded'::order_status
                                WHEN 'completed' THEN 'completed'::order_status
                                ELSE 'waiting'::order_status
                            END;
                    END IF;
                END $$;
            ");

            // ✅ Đảm bảo payment_status column cũng đúng
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF (SELECT data_type FROM information_schema.columns 
                        WHERE table_name = 'orders' AND column_name = 'payment_status') != 'USER-DEFINED' THEN
                        
                        ALTER TABLE orders 
                        ALTER COLUMN payment_status TYPE payment_status 
                        USING 
                            CASE LOWER(payment_status::text)
                                WHEN 'pending' THEN 'pending'::payment_status
                                WHEN 'paid' THEN 'paid'::payment_status
                                WHEN 'failed' THEN 'failed'::payment_status
                                WHEN 'refunded' THEN 'refunded'::payment_status
                                WHEN 'partially_refunded' THEN 'partially_refunded'::payment_status
                                ELSE 'pending'::payment_status
                            END;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Không cần down migration - giữ enum state
        }
    }
}