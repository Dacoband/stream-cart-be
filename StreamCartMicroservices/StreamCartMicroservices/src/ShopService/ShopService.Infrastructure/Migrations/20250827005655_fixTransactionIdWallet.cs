using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fixTransactionIdWallet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankAccount",
                table: "Wallet_Transactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankNumber",
                table: "Wallet_Transactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransactionId",
                table: "Wallet_Transactions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankAccount",
                table: "Wallet_Transactions");

            migrationBuilder.DropColumn(
                name: "BankNumber",
                table: "Wallet_Transactions");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "Wallet_Transactions");
        }
    }
}
