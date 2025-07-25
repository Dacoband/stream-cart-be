using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LivestreamService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJoinTokenToLivestream : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JoinToken",
                table: "Livestreams",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JoinToken",
                table: "Livestreams");
        }
    }
}
