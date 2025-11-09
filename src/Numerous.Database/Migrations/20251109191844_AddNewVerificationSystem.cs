using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Numerous.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddNewVerificationSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "join_message");

            migrationBuilder.DropColumn(
                name: "greet_on_added",
                table: "guild");

            migrationBuilder.RenameColumn(
                name: "unverified_role_id",
                table: "guild",
                newName: "verified_role_id");

            migrationBuilder.RenameIndex(
                name: "ix_guild_unverified_role_id",
                table: "guild",
                newName: "ix_guild_verified_role_id");

            migrationBuilder.AddColumn<decimal>(
                name: "user_log_channel_id",
                table: "guild",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_guild_user_log_channel_id",
                table: "guild",
                column: "user_log_channel_id");

            migrationBuilder.AddForeignKey(
                name: "fk_guild_channel_user_log_channel_id",
                table: "guild",
                column: "user_log_channel_id",
                principalTable: "channel",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_guild_channel_user_log_channel_id",
                table: "guild");

            migrationBuilder.DropIndex(
                name: "ix_guild_user_log_channel_id",
                table: "guild");

            migrationBuilder.DropColumn(
                name: "user_log_channel_id",
                table: "guild");

            migrationBuilder.RenameColumn(
                name: "verified_role_id",
                table: "guild",
                newName: "unverified_role_id");

            migrationBuilder.RenameIndex(
                name: "ix_guild_verified_role_id",
                table: "guild",
                newName: "ix_guild_unverified_role_id");

            migrationBuilder.AddColumn<bool>(
                name: "greet_on_added",
                table: "guild",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "join_message",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    description = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true),
                    title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_join_message", x => x.guild_id);
                    table.CheckConstraint("CK_JoinMessage_HasText", "\"Title\" IS NOT NULL OR \"Description\" IS NOT NULL");
                    table.ForeignKey(
                        name: "fk_join_message_guild_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guild",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_join_message_message_channel_channel_id",
                        column: x => x.channel_id,
                        principalTable: "message_channel",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_join_message_channel_id",
                table: "join_message",
                column: "channel_id");
        }
    }
}
