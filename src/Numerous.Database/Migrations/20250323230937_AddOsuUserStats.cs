using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Numerous.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddOsuUserStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_online_beatmap_osu_user_creator_id",
                table: "online_beatmap");

            migrationBuilder.DropIndex(
                name: "ix_online_beatmap_creator_id",
                table: "online_beatmap");

            migrationBuilder.DropColumn(
                name: "creator_id",
                table: "online_beatmap");

            migrationBuilder.CreateTable(
                name: "beatmap_stats",
                columns: table => new
                {
                    beatmap_id = table.Column<long>(type: "bigint", nullable: false),
                    timestamp = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    play_count = table.Column<int>(type: "integer", nullable: false),
                    pass_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_beatmap_stats", x => new { x.beatmap_id, x.timestamp });
                    table.ForeignKey(
                        name: "fk_beatmap_stats_online_beatmap_beatmap_id",
                        column: x => x.beatmap_id,
                        principalTable: "online_beatmap",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "beatmapset_stats",
                columns: table => new
                {
                    beatmapset_id = table.Column<long>(type: "bigint", nullable: false),
                    timestamp = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    favourite_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_beatmapset_stats", x => new { x.beatmapset_id, x.timestamp });
                    table.ForeignKey(
                        name: "fk_beatmapset_stats_online_beatmapset_beatmapset_id",
                        column: x => x.beatmapset_id,
                        principalTable: "online_beatmapset",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "osu_user_stats",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    timestamp = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    follower_count = table.Column<int>(type: "integer", nullable: false),
                    subscriber_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_osu_user_stats", x => new { x.user_id, x.timestamp });
                    table.ForeignKey(
                        name: "fk_osu_user_stats_osu_user_user_id",
                        column: x => x.user_id,
                        principalTable: "osu_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "beatmap_stats");

            migrationBuilder.DropTable(
                name: "beatmapset_stats");

            migrationBuilder.DropTable(
                name: "osu_user_stats");

            migrationBuilder.AddColumn<long>(
                name: "creator_id",
                table: "online_beatmap",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "ix_online_beatmap_creator_id",
                table: "online_beatmap",
                column: "creator_id");

            migrationBuilder.AddForeignKey(
                name: "fk_online_beatmap_osu_user_creator_id",
                table: "online_beatmap",
                column: "creator_id",
                principalTable: "osu_user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
