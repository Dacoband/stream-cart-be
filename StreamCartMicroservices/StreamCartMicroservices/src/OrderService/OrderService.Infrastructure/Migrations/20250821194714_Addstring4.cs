using Microsoft.EntityFrameworkCore.Migrations;
using OrderService.Domain.Enums;

#nullable disable

namespace OrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Addstring4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:order_status", "waiting,pending,processing,shipped,delivered,cancelled,packed,on_delivere,returning,refunded,completed")
                .Annotation("Npgsql:Enum:payment_status", "pending,paid,failed,refunded,partially_refunded")
                .OldAnnotation("Npgsql:Enum:order_status", "cancelled,completed,delivered,on_delivere,packed,pending,processing,refunded,returning,shipped,waiting")
                .OldAnnotation("Npgsql:Enum:payment_status", "pending,paid,failed,refunded,partially_refunded");

            //migrationBuilder.AlterColumn<OrderStatus>(
            //    name: "order_status",
            //    table: "orders",
            //    type: "order_status",
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "character varying(32)",
            //    oldMaxLength: 32);
            migrationBuilder.Sql("ALTER TABLE orders ALTER COLUMN order_status TYPE order_status USING order_status::order_status;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:order_status", "cancelled,completed,delivered,on_delivere,packed,pending,processing,refunded,returning,shipped,waiting")
                .Annotation("Npgsql:Enum:payment_status", "pending,paid,failed,refunded,partially_refunded")
                .OldAnnotation("Npgsql:Enum:order_status", "waiting,pending,processing,shipped,delivered,cancelled,packed,on_delivere,returning,refunded,completed")
                .OldAnnotation("Npgsql:Enum:payment_status", "pending,paid,failed,refunded,partially_refunded");

            migrationBuilder.Sql("ALTER TABLE orders ALTER COLUMN order_status TYPE character varying(32) USING order_status::text;");

        }
    }
}
