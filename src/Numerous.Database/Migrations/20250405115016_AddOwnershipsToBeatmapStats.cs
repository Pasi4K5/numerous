using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Numerous.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnershipsToBeatmapStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "beatmap_ownership_stats",
                columns: table => new
                {
                    owner_id = table.Column<int>(type: "integer", nullable: false),
                    beatmap_id = table.Column<int>(type: "integer", nullable: false),
                    timestamp = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_beatmap_ownership_stats", x => new { x.owner_id, x.beatmap_id, x.user_id, x.timestamp });
                    table.ForeignKey(
                        name: "fk_beatmap_ownership_stats_beatmap_stats_beatmap_id_timestamp_",
                        columns: x => new { x.beatmap_id, x.timestamp, x.user_id },
                        principalTable: "beatmap_stats",
                        principalColumns: new[] { "beatmap_id", "timestamp", "user_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_beatmap_ownership_stats_osu_user_owner_id",
                        column: x => x.owner_id,
                        principalTable: "osu_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_beatmap_ownership_stats_beatmap_id_timestamp_user_id",
                table: "beatmap_ownership_stats",
                columns: new[] { "beatmap_id", "timestamp", "user_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "beatmap_ownership_stats");
        }
    }
}
