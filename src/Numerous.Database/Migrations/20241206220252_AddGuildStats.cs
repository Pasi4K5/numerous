using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Numerous.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddGuildStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "guild_stats_entry",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    member_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_guild_stats_entry", x => new { x.guild_id, x.timestamp });
                    table.ForeignKey(
                        name: "fk_guild_stats_entry_guild_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guild",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_guild_stats_entry_guild_id",
                table: "guild_stats_entry",
                column: "guild_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "guild_stats_entry");
        }
    }
}
