using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CartService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateAttributesOfCartItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Dictionary<string, string>>(
                name: "Attributes",
                table: "CartItems",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(Dictionary<string, string>),
                oldType: "jsonb");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Dictionary<string, string>>(
                name: "Attributes",
                table: "CartItems",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(Dictionary<string, string>),
                oldType: "jsonb",
                oldNullable: true);
        }
    }
}
