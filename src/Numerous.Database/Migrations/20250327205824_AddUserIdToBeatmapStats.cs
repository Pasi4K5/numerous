using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Numerous.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToBeatmapStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_beatmapset_stats",
                table: "beatmapset_stats");

            migrationBuilder.DropPrimaryKey(
                name: "pk_beatmap_stats",
                table: "beatmap_stats");

            migrationBuilder.AddColumn<int>(
                name: "user_id",
                table: "beatmapset_stats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "user_id",
                table: "beatmap_stats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "pk_beatmapset_stats",
                table: "beatmapset_stats",
                columns: new[] { "beatmapset_id", "timestamp", "user_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_beatmap_stats",
                table: "beatmap_stats",
                columns: new[] { "beatmap_id", "timestamp", "user_id" });

            migrationBuilder.CreateIndex(
                name: "ix_beatmapset_stats_user_id",
                table: "beatmapset_stats",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_beatmap_stats_user_id",
                table: "beatmap_stats",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_beatmap_stats_osu_user_user_id",
                table: "beatmap_stats",
                column: "user_id",
                principalTable: "osu_user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_beatmapset_stats_osu_user_user_id",
                table: "beatmapset_stats",
                column: "user_id",
                principalTable: "osu_user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_beatmap_stats_osu_user_user_id",
                table: "beatmap_stats");

            migrationBuilder.DropForeignKey(
                name: "fk_beatmapset_stats_osu_user_user_id",
                table: "beatmapset_stats");

            migrationBuilder.DropPrimaryKey(
                name: "pk_beatmapset_stats",
                table: "beatmapset_stats");

            migrationBuilder.DropIndex(
                name: "ix_beatmapset_stats_user_id",
                table: "beatmapset_stats");

            migrationBuilder.DropPrimaryKey(
                name: "pk_beatmap_stats",
                table: "beatmap_stats");

            migrationBuilder.DropIndex(
                name: "ix_beatmap_stats_user_id",
                table: "beatmap_stats");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "beatmapset_stats");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "beatmap_stats");

            migrationBuilder.AddPrimaryKey(
                name: "pk_beatmapset_stats",
                table: "beatmapset_stats",
                columns: new[] { "beatmapset_id", "timestamp" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_beatmap_stats",
                table: "beatmap_stats",
                columns: new[] { "beatmap_id", "timestamp" });
        }
    }
}
