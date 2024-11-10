using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Numerous.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddDmReminders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_reminder_message_channel_channel_id",
                table: "reminder");

            migrationBuilder.AlterColumn<decimal>(
                name: "channel_id",
                table: "reminder",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

            migrationBuilder.AddForeignKey(
                name: "fk_reminder_message_channel_channel_id",
                table: "reminder",
                column: "channel_id",
                principalTable: "message_channel",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_reminder_message_channel_channel_id",
                table: "reminder");

            migrationBuilder.AlterColumn<decimal>(
                name: "channel_id",
                table: "reminder",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_reminder_message_channel_channel_id",
                table: "reminder",
                column: "channel_id",
                principalTable: "message_channel",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
