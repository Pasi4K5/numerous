using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Numerous.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddMapfeedChannels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MapfeedChannelId",
                table: "Guild",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Guild_MapfeedChannelId",
                table: "Guild",
                column: "MapfeedChannelId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Guild_Channel_MapfeedChannelId",
                table: "Guild",
                column: "MapfeedChannelId",
                principalTable: "Channel",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Guild_Channel_MapfeedChannelId",
                table: "Guild");

            migrationBuilder.DropIndex(
                name: "IX_Guild_MapfeedChannelId",
                table: "Guild");

            migrationBuilder.DropColumn(
                name: "MapfeedChannelId",
                table: "Guild");
        }
    }
}
