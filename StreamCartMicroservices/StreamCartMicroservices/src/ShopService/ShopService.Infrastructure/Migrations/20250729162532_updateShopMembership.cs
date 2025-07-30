using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateShopMembership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Commission",
                table: "Shop_memberships",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxProduct",
                table: "Shop_memberships",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RemainingLivestream",
                table: "Shop_memberships",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Shop_memberships",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "max_product",
                table: "Membership",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "duration",
                table: "Membership",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "commission",
                table: "Membership",
                type: "numeric(5,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Commission",
                table: "Shop_memberships");

            migrationBuilder.DropColumn(
                name: "MaxProduct",
                table: "Shop_memberships");

            migrationBuilder.DropColumn(
                name: "RemainingLivestream",
                table: "Shop_memberships");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Shop_memberships");

            migrationBuilder.AlterColumn<int>(
                name: "max_product",
                table: "Membership",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "duration",
                table: "Membership",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "commission",
                table: "Membership",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)",
                oldNullable: true);
        }
    }
}
