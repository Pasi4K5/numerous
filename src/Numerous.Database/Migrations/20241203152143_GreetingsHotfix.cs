using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Numerous.Database.Migrations
{
    /// <inheritdoc />
    public partial class GreetingsHotfix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "greet_on_added",
                table: "guild",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "greet_on_added",
                table: "guild");
        }
    }
}
