using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class initiateFlashSaleTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           
            migrationBuilder.CreateTable(
                name: "Flash-Sales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductID = table.Column<Guid>(type: "uuid", nullable: false),
                    VariantID = table.Column<Guid>(type: "uuid", nullable: true),
                    FlashSalePrice = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    QuantityAvailable = table.Column<int>(type: "integer", nullable: false),
                    QuantitySold = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flash-Sales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlashSales_Products",
                        column: x => x.ProductID,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            

           

            migrationBuilder.CreateIndex(
                name: "IX_Flash-Sales_ProductID",
                table: "Flash-Sales",
                column: "ProductID");

           
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.DropTable(
                name: "Flash-Sales");

           
        }
    }
}
