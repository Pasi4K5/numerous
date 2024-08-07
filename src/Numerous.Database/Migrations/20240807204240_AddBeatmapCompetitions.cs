using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Numerous.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddBeatmapCompetitions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OsuUser_DiscordUser_DiscordUserId",
                table: "OsuUser");

            migrationBuilder.AlterColumn<decimal>(
                name: "DiscordUserId",
                table: "OsuUser",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

            migrationBuilder.CreateTable(
                name: "OnlineBeatmapset",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    CreatorId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnlineBeatmapset", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnlineBeatmapset_OsuUser_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "OsuUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OnlineBeatmap",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    OnlineBeatmapsetId = table.Column<long>(type: "bigint", nullable: false),
                    CreatorId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnlineBeatmap", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnlineBeatmap_OnlineBeatmapset_OnlineBeatmapsetId",
                        column: x => x.OnlineBeatmapsetId,
                        principalTable: "OnlineBeatmapset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OnlineBeatmap_OsuUser_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "OsuUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocalBeatmap",
                columns: table => new
                {
                    Md5Hash = table.Column<Guid>(type: "uuid", nullable: false),
                    OsuText = table.Column<string>(type: "text", nullable: false),
                    OszHash = table.Column<byte[]>(type: "bytea", nullable: false),
                    MaxCombo = table.Column<long>(type: "bigint", nullable: false),
                    OnlineBeatmapId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalBeatmap", x => x.Md5Hash);
                    table.CheckConstraint("CK_LocalBeatmap_ValidSha256", "length(\"OszHash\") = 256 / 8");
                    table.ForeignKey(
                        name: "FK_LocalBeatmap_OnlineBeatmap_OnlineBeatmapId",
                        column: x => x.OnlineBeatmapId,
                        principalTable: "OnlineBeatmap",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BeatmapCompetition",
                columns: table => new
                {
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    StartTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LocalBeatmapId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeatmapCompetition", x => new { x.GuildId, x.StartTime });
                    table.CheckConstraint("CK_BeatmapCompetition_ValidTime", "\"StartTime\" < \"EndTime\"");
                    table.ForeignKey(
                        name: "FK_BeatmapCompetition_Guild_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guild",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BeatmapCompetition_LocalBeatmap_LocalBeatmapId",
                        column: x => x.LocalBeatmapId,
                        principalTable: "LocalBeatmap",
                        principalColumn: "Md5Hash",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BeatmapCompetitionScore",
                columns: table => new
                {
                    Md5Hash = table.Column<Guid>(type: "uuid", nullable: false),
                    OnlineId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    TotalScore = table.Column<long>(type: "bigint", nullable: false),
                    Mods = table.Column<string[]>(type: "char(2)[]", nullable: false),
                    Accuracy = table.Column<double>(type: "double precision", nullable: false),
                    MaxCombo = table.Column<long>(type: "bigint", nullable: false),
                    GreatCount = table.Column<long>(type: "bigint", nullable: false),
                    OkCount = table.Column<long>(type: "bigint", nullable: false),
                    MehCount = table.Column<long>(type: "bigint", nullable: false),
                    MissCount = table.Column<long>(type: "bigint", nullable: false),
                    DateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    StartTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PlayerId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeatmapCompetitionScore", x => x.Md5Hash);
                    table.ForeignKey(
                        name: "FK_BeatmapCompetitionScore_BeatmapCompetition_GuildId_StartTime",
                        columns: x => new { x.GuildId, x.StartTime },
                        principalTable: "BeatmapCompetition",
                        principalColumns: new[] { "GuildId", "StartTime" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BeatmapCompetitionScore_OsuUser_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "OsuUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Replay",
                columns: table => new
                {
                    Md5Hash = table.Column<Guid>(type: "uuid", nullable: false),
                    Data = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Replay", x => x.Md5Hash);
                    table.ForeignKey(
                        name: "FK_Replay_BeatmapCompetitionScore_Md5Hash",
                        column: x => x.Md5Hash,
                        principalTable: "BeatmapCompetitionScore",
                        principalColumn: "Md5Hash",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BeatmapCompetition_LocalBeatmapId",
                table: "BeatmapCompetition",
                column: "LocalBeatmapId");

            migrationBuilder.CreateIndex(
                name: "IX_BeatmapCompetitionScore_GuildId_StartTime",
                table: "BeatmapCompetitionScore",
                columns: new[] { "GuildId", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_BeatmapCompetitionScore_OnlineId",
                table: "BeatmapCompetitionScore",
                column: "OnlineId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BeatmapCompetitionScore_PlayerId",
                table: "BeatmapCompetitionScore",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalBeatmap_OnlineBeatmapId",
                table: "LocalBeatmap",
                column: "OnlineBeatmapId");

            migrationBuilder.CreateIndex(
                name: "IX_OnlineBeatmap_CreatorId",
                table: "OnlineBeatmap",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_OnlineBeatmap_OnlineBeatmapsetId",
                table: "OnlineBeatmap",
                column: "OnlineBeatmapsetId");

            migrationBuilder.CreateIndex(
                name: "IX_OnlineBeatmapset_CreatorId",
                table: "OnlineBeatmapset",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_OsuUser_DiscordUser_DiscordUserId",
                table: "OsuUser",
                column: "DiscordUserId",
                principalTable: "DiscordUser",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OsuUser_DiscordUser_DiscordUserId",
                table: "OsuUser");

            migrationBuilder.DropTable(
                name: "Replay");

            migrationBuilder.DropTable(
                name: "BeatmapCompetitionScore");

            migrationBuilder.DropTable(
                name: "BeatmapCompetition");

            migrationBuilder.DropTable(
                name: "LocalBeatmap");

            migrationBuilder.DropTable(
                name: "OnlineBeatmap");

            migrationBuilder.DropTable(
                name: "OnlineBeatmapset");

            migrationBuilder.AlterColumn<decimal>(
                name: "DiscordUserId",
                table: "OsuUser",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OsuUser_DiscordUser_DiscordUserId",
                table: "OsuUser",
                column: "DiscordUserId",
                principalTable: "DiscordUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
