using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Numerous.Database.Migrations
{
    /// <inheritdoc />
    public partial class ChangeNamingConventions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AutoPingMapping_ForumChannel_ChannelId",
                table: "AutoPingMapping");

            migrationBuilder.DropForeignKey(
                name: "FK_BeatmapCompetition_Guild_GuildId",
                table: "BeatmapCompetition");

            migrationBuilder.DropForeignKey(
                name: "FK_BeatmapCompetition_LocalBeatmap_LocalBeatmapId",
                table: "BeatmapCompetition");

            migrationBuilder.DropForeignKey(
                name: "FK_BeatmapCompetitionScore_BeatmapCompetition_GuildId_StartTime",
                table: "BeatmapCompetitionScore");

            migrationBuilder.DropForeignKey(
                name: "FK_BeatmapCompetitionScore_OsuUser_PlayerId",
                table: "BeatmapCompetitionScore");

            migrationBuilder.DropForeignKey(
                name: "FK_Channel_Guild_GuildId",
                table: "Channel");

            migrationBuilder.DropForeignKey(
                name: "FK_DiscordMessage_DiscordMessage_ReferenceMessageId",
                table: "DiscordMessage");

            migrationBuilder.DropForeignKey(
                name: "FK_DiscordMessage_DiscordUser_AuthorId",
                table: "DiscordMessage");

            migrationBuilder.DropForeignKey(
                name: "FK_DiscordMessage_MessageChannel_ChannelId",
                table: "DiscordMessage");

            migrationBuilder.DropForeignKey(
                name: "FK_DiscordMessageVersion_DiscordMessage_MessageId",
                table: "DiscordMessageVersion");

            migrationBuilder.DropForeignKey(
                name: "FK_ForumChannel_Channel_Id",
                table: "ForumChannel");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupRoleMapping_Guild_GuildId",
                table: "GroupRoleMapping");

            migrationBuilder.DropForeignKey(
                name: "FK_Guild_Channel_MapfeedChannelId",
                table: "Guild");

            migrationBuilder.DropForeignKey(
                name: "FK_JoinMessage_Guild_GuildId",
                table: "JoinMessage");

            migrationBuilder.DropForeignKey(
                name: "FK_JoinMessage_MessageChannel_ChannelId",
                table: "JoinMessage");

            migrationBuilder.DropForeignKey(
                name: "FK_LocalBeatmap_OnlineBeatmap_OnlineBeatmapId",
                table: "LocalBeatmap");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageChannel_Channel_Id",
                table: "MessageChannel");

            migrationBuilder.DropForeignKey(
                name: "FK_OnlineBeatmap_OnlineBeatmapset_OnlineBeatmapsetId",
                table: "OnlineBeatmap");

            migrationBuilder.DropForeignKey(
                name: "FK_OnlineBeatmap_OsuUser_CreatorId",
                table: "OnlineBeatmap");

            migrationBuilder.DropForeignKey(
                name: "FK_OnlineBeatmapset_OsuUser_CreatorId",
                table: "OnlineBeatmapset");

            migrationBuilder.DropForeignKey(
                name: "FK_OsuUser_DiscordUser_DiscordUserId",
                table: "OsuUser");

            migrationBuilder.DropForeignKey(
                name: "FK_Reminder_DiscordUser_UserId",
                table: "Reminder");

            migrationBuilder.DropForeignKey(
                name: "FK_Reminder_MessageChannel_ChannelId",
                table: "Reminder");

            migrationBuilder.DropForeignKey(
                name: "FK_Replay_BeatmapCompetitionScore_Md5Hash",
                table: "Replay");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Replay",
                table: "Replay");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reminder",
                table: "Reminder");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Guild",
                table: "Guild");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Channel",
                table: "Channel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OsuUser",
                table: "OsuUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OnlineBeatmapset",
                table: "OnlineBeatmapset");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OnlineBeatmap",
                table: "OnlineBeatmap");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MessageChannel",
                table: "MessageChannel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LocalBeatmap",
                table: "LocalBeatmap");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JoinMessage",
                table: "JoinMessage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupRoleMapping",
                table: "GroupRoleMapping");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ForumChannel",
                table: "ForumChannel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DiscordUser",
                table: "DiscordUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DiscordMessageVersion",
                table: "DiscordMessageVersion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DiscordMessage",
                table: "DiscordMessage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BeatmapCompetitionScore",
                table: "BeatmapCompetitionScore");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BeatmapCompetition",
                table: "BeatmapCompetition");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AutoPingMapping",
                table: "AutoPingMapping");

            migrationBuilder.RenameTable(
                name: "Replay",
                newName: "replay");

            migrationBuilder.RenameTable(
                name: "Reminder",
                newName: "reminder");

            migrationBuilder.RenameTable(
                name: "Guild",
                newName: "guild");

            migrationBuilder.RenameTable(
                name: "Channel",
                newName: "channel");

            migrationBuilder.RenameTable(
                name: "OsuUser",
                newName: "osu_user");

            migrationBuilder.RenameTable(
                name: "OnlineBeatmapset",
                newName: "online_beatmapset");

            migrationBuilder.RenameTable(
                name: "OnlineBeatmap",
                newName: "online_beatmap");

            migrationBuilder.RenameTable(
                name: "MessageChannel",
                newName: "message_channel");

            migrationBuilder.RenameTable(
                name: "LocalBeatmap",
                newName: "local_beatmap");

            migrationBuilder.RenameTable(
                name: "JoinMessage",
                newName: "join_message");

            migrationBuilder.RenameTable(
                name: "GroupRoleMapping",
                newName: "group_role_mapping");

            migrationBuilder.RenameTable(
                name: "ForumChannel",
                newName: "forum_channel");

            migrationBuilder.RenameTable(
                name: "DiscordUser",
                newName: "discord_user");

            migrationBuilder.RenameTable(
                name: "DiscordMessageVersion",
                newName: "discord_message_version");

            migrationBuilder.RenameTable(
                name: "DiscordMessage",
                newName: "discord_message");

            migrationBuilder.RenameTable(
                name: "BeatmapCompetitionScore",
                newName: "beatmap_competition_score");

            migrationBuilder.RenameTable(
                name: "BeatmapCompetition",
                newName: "beatmap_competition");

            migrationBuilder.RenameTable(
                name: "AutoPingMapping",
                newName: "auto_ping_mapping");

            migrationBuilder.RenameColumn(
                name: "Data",
                table: "replay",
                newName: "data");

            migrationBuilder.RenameColumn(
                name: "Md5Hash",
                table: "replay",
                newName: "md5_hash");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "reminder",
                newName: "timestamp");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "reminder",
                newName: "message");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "reminder",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "reminder",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "ChannelId",
                table: "reminder",
                newName: "channel_id");

            migrationBuilder.RenameIndex(
                name: "IX_Reminder_UserId",
                table: "reminder",
                newName: "ix_reminder_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_Reminder_ChannelId",
                table: "reminder",
                newName: "ix_reminder_channel_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "guild",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UnverifiedRoleId",
                table: "guild",
                newName: "unverified_role_id");

            migrationBuilder.RenameColumn(
                name: "TrackMessages",
                table: "guild",
                newName: "track_messages");

            migrationBuilder.RenameColumn(
                name: "MapfeedChannelId",
                table: "guild",
                newName: "mapfeed_channel_id");

            migrationBuilder.RenameIndex(
                name: "IX_Guild_UnverifiedRoleId",
                table: "guild",
                newName: "ix_guild_unverified_role_id");

            migrationBuilder.RenameIndex(
                name: "IX_Guild_MapfeedChannelId",
                table: "guild",
                newName: "ix_guild_mapfeed_channel_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "channel",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "GuildId",
                table: "channel",
                newName: "guild_id");

            migrationBuilder.RenameIndex(
                name: "IX_Channel_GuildId",
                table: "channel",
                newName: "ix_channel_guild_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "osu_user",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "DiscordUserId",
                table: "osu_user",
                newName: "discord_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_OsuUser_DiscordUserId",
                table: "osu_user",
                newName: "ix_osu_user_discord_user_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "online_beatmapset",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "online_beatmapset",
                newName: "creator_id");

            migrationBuilder.RenameIndex(
                name: "IX_OnlineBeatmapset_CreatorId",
                table: "online_beatmapset",
                newName: "ix_online_beatmapset_creator_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "online_beatmap",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "OnlineBeatmapsetId",
                table: "online_beatmap",
                newName: "online_beatmapset_id");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "online_beatmap",
                newName: "creator_id");

            migrationBuilder.RenameIndex(
                name: "IX_OnlineBeatmap_OnlineBeatmapsetId",
                table: "online_beatmap",
                newName: "ix_online_beatmap_online_beatmapset_id");

            migrationBuilder.RenameIndex(
                name: "IX_OnlineBeatmap_CreatorId",
                table: "online_beatmap",
                newName: "ix_online_beatmap_creator_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "message_channel",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "IsReadOnly",
                table: "message_channel",
                newName: "is_read_only");

            migrationBuilder.RenameColumn(
                name: "OszHash",
                table: "local_beatmap",
                newName: "osz_hash");

            migrationBuilder.RenameColumn(
                name: "OsuText",
                table: "local_beatmap",
                newName: "osu_text");

            migrationBuilder.RenameColumn(
                name: "OnlineBeatmapId",
                table: "local_beatmap",
                newName: "online_beatmap_id");

            migrationBuilder.RenameColumn(
                name: "MaxCombo",
                table: "local_beatmap",
                newName: "max_combo");

            migrationBuilder.RenameColumn(
                name: "Md5Hash",
                table: "local_beatmap",
                newName: "md5_hash");

            migrationBuilder.RenameIndex(
                name: "IX_LocalBeatmap_OnlineBeatmapId",
                table: "local_beatmap",
                newName: "ix_local_beatmap_online_beatmap_id");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "join_message",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "join_message",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "ChannelId",
                table: "join_message",
                newName: "channel_id");

            migrationBuilder.RenameColumn(
                name: "GuildId",
                table: "join_message",
                newName: "guild_id");

            migrationBuilder.RenameIndex(
                name: "IX_JoinMessage_ChannelId",
                table: "join_message",
                newName: "ix_join_message_channel_id");

            migrationBuilder.RenameColumn(
                name: "GroupId",
                table: "group_role_mapping",
                newName: "group_id");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "group_role_mapping",
                newName: "role_id");

            migrationBuilder.RenameColumn(
                name: "GuildId",
                table: "group_role_mapping",
                newName: "guild_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "forum_channel",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "discord_user",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TimeZoneId",
                table: "discord_user",
                newName: "time_zone_id");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "discord_message_version",
                newName: "timestamp");

            migrationBuilder.RenameColumn(
                name: "RawContent",
                table: "discord_message_version",
                newName: "raw_content");

            migrationBuilder.RenameColumn(
                name: "CleanContent",
                table: "discord_message_version",
                newName: "clean_content");

            migrationBuilder.RenameColumn(
                name: "MessageId",
                table: "discord_message_version",
                newName: "message_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "discord_message",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ReferenceMessageId",
                table: "discord_message",
                newName: "reference_message_id");

            migrationBuilder.RenameColumn(
                name: "IsHidden",
                table: "discord_message",
                newName: "is_hidden");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "discord_message",
                newName: "deleted_at");

            migrationBuilder.RenameColumn(
                name: "ChannelId",
                table: "discord_message",
                newName: "channel_id");

            migrationBuilder.RenameColumn(
                name: "AuthorId",
                table: "discord_message",
                newName: "author_id");

            migrationBuilder.RenameIndex(
                name: "IX_DiscordMessage_ReferenceMessageId",
                table: "discord_message",
                newName: "ix_discord_message_reference_message_id");

            migrationBuilder.RenameIndex(
                name: "IX_DiscordMessage_ChannelId",
                table: "discord_message",
                newName: "ix_discord_message_channel_id");

            migrationBuilder.RenameIndex(
                name: "IX_DiscordMessage_AuthorId",
                table: "discord_message",
                newName: "ix_discord_message_author_id");

            migrationBuilder.RenameColumn(
                name: "Mods",
                table: "beatmap_competition_score",
                newName: "mods");

            migrationBuilder.RenameColumn(
                name: "Accuracy",
                table: "beatmap_competition_score",
                newName: "accuracy");

            migrationBuilder.RenameColumn(
                name: "TotalScore",
                table: "beatmap_competition_score",
                newName: "total_score");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "beatmap_competition_score",
                newName: "start_time");

            migrationBuilder.RenameColumn(
                name: "PlayerId",
                table: "beatmap_competition_score",
                newName: "player_id");

            migrationBuilder.RenameColumn(
                name: "OnlineId",
                table: "beatmap_competition_score",
                newName: "online_id");

            migrationBuilder.RenameColumn(
                name: "OkCount",
                table: "beatmap_competition_score",
                newName: "ok_count");

            migrationBuilder.RenameColumn(
                name: "MissCount",
                table: "beatmap_competition_score",
                newName: "miss_count");

            migrationBuilder.RenameColumn(
                name: "MehCount",
                table: "beatmap_competition_score",
                newName: "meh_count");

            migrationBuilder.RenameColumn(
                name: "MaxCombo",
                table: "beatmap_competition_score",
                newName: "max_combo");

            migrationBuilder.RenameColumn(
                name: "GuildId",
                table: "beatmap_competition_score",
                newName: "guild_id");

            migrationBuilder.RenameColumn(
                name: "GreatCount",
                table: "beatmap_competition_score",
                newName: "great_count");

            migrationBuilder.RenameColumn(
                name: "DateTime",
                table: "beatmap_competition_score",
                newName: "date_time");

            migrationBuilder.RenameIndex(
                name: "IX_BeatmapCompetitionScore_PlayerId",
                table: "beatmap_competition_score",
                newName: "ix_beatmap_competition_score_player_id");

            migrationBuilder.RenameIndex(
                name: "IX_BeatmapCompetitionScore_OnlineId",
                table: "beatmap_competition_score",
                newName: "ix_beatmap_competition_score_online_id");

            migrationBuilder.RenameIndex(
                name: "IX_BeatmapCompetitionScore_GuildId_StartTime",
                table: "beatmap_competition_score",
                newName: "ix_beatmap_competition_score_guild_id_start_time");

            migrationBuilder.RenameColumn(
                name: "LocalBeatmapId",
                table: "beatmap_competition",
                newName: "local_beatmap_id");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "beatmap_competition",
                newName: "end_time");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "beatmap_competition",
                newName: "start_time");

            migrationBuilder.RenameColumn(
                name: "GuildId",
                table: "beatmap_competition",
                newName: "guild_id");

            migrationBuilder.RenameIndex(
                name: "IX_BeatmapCompetition_LocalBeatmapId",
                table: "beatmap_competition",
                newName: "ix_beatmap_competition_local_beatmap_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "auto_ping_mapping",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TagId",
                table: "auto_ping_mapping",
                newName: "tag_id");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "auto_ping_mapping",
                newName: "role_id");

            migrationBuilder.RenameColumn(
                name: "ChannelId",
                table: "auto_ping_mapping",
                newName: "channel_id");

            migrationBuilder.RenameIndex(
                name: "IX_AutoPingMapping_ChannelId_TagId_RoleId",
                table: "auto_ping_mapping",
                newName: "ix_auto_ping_mapping_channel_id_tag_id_role_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_replay",
                table: "replay",
                column: "md5_hash");

            migrationBuilder.AddPrimaryKey(
                name: "pk_reminder",
                table: "reminder",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_guild",
                table: "guild",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_channel",
                table: "channel",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_osu_user",
                table: "osu_user",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_online_beatmapset",
                table: "online_beatmapset",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_online_beatmap",
                table: "online_beatmap",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_message_channel",
                table: "message_channel",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_local_beatmap",
                table: "local_beatmap",
                column: "md5_hash");

            migrationBuilder.AddPrimaryKey(
                name: "pk_join_message",
                table: "join_message",
                column: "guild_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_group_role_mapping",
                table: "group_role_mapping",
                columns: new[] { "guild_id", "role_id", "group_id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_forum_channel",
                table: "forum_channel",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_discord_user",
                table: "discord_user",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_discord_message_version",
                table: "discord_message_version",
                columns: new[] { "message_id", "timestamp" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_discord_message",
                table: "discord_message",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_beatmap_competition_score",
                table: "beatmap_competition_score",
                column: "Md5Hash");

            migrationBuilder.AddPrimaryKey(
                name: "pk_beatmap_competition",
                table: "beatmap_competition",
                columns: new[] { "guild_id", "start_time" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_auto_ping_mapping",
                table: "auto_ping_mapping",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_auto_ping_mapping_forum_channel_channel_id",
                table: "auto_ping_mapping",
                column: "channel_id",
                principalTable: "forum_channel",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_beatmap_competition_guild_guild_id",
                table: "beatmap_competition",
                column: "guild_id",
                principalTable: "guild",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_beatmap_competition_local_beatmap_local_beatmap_id",
                table: "beatmap_competition",
                column: "local_beatmap_id",
                principalTable: "local_beatmap",
                principalColumn: "md5_hash",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_beatmap_competition_score_beatmap_competition_guild_id_star",
                table: "beatmap_competition_score",
                columns: new[] { "guild_id", "start_time" },
                principalTable: "beatmap_competition",
                principalColumns: new[] { "guild_id", "start_time" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_beatmap_competition_score_osu_user_player_id",
                table: "beatmap_competition_score",
                column: "player_id",
                principalTable: "osu_user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_channel_guild_guild_id",
                table: "channel",
                column: "guild_id",
                principalTable: "guild",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_discord_message_discord_message_reference_message_id",
                table: "discord_message",
                column: "reference_message_id",
                principalTable: "discord_message",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_discord_message_discord_user_author_id",
                table: "discord_message",
                column: "author_id",
                principalTable: "discord_user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_discord_message_message_channel_channel_id",
                table: "discord_message",
                column: "channel_id",
                principalTable: "message_channel",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_discord_message_version_discord_message_message_id",
                table: "discord_message_version",
                column: "message_id",
                principalTable: "discord_message",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_forum_channel_channel_id",
                table: "forum_channel",
                column: "id",
                principalTable: "channel",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_group_role_mapping_guild_guild_id",
                table: "group_role_mapping",
                column: "guild_id",
                principalTable: "guild",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_guild_channel_mapfeed_channel_id",
                table: "guild",
                column: "mapfeed_channel_id",
                principalTable: "channel",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_join_message_guild_guild_id",
                table: "join_message",
                column: "guild_id",
                principalTable: "guild",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_join_message_message_channel_channel_id",
                table: "join_message",
                column: "channel_id",
                principalTable: "message_channel",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_local_beatmap_online_beatmap_online_beatmap_id",
                table: "local_beatmap",
                column: "online_beatmap_id",
                principalTable: "online_beatmap",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_message_channel_channel_id",
                table: "message_channel",
                column: "id",
                principalTable: "channel",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_online_beatmap_online_beatmapset_online_beatmapset_id",
                table: "online_beatmap",
                column: "online_beatmapset_id",
                principalTable: "online_beatmapset",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_online_beatmap_osu_user_creator_id",
                table: "online_beatmap",
                column: "creator_id",
                principalTable: "osu_user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_online_beatmapset_osu_user_creator_id",
                table: "online_beatmapset",
                column: "creator_id",
                principalTable: "osu_user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_osu_user_discord_user_discord_user_id",
                table: "osu_user",
                column: "discord_user_id",
                principalTable: "discord_user",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_reminder_discord_user_user_id",
                table: "reminder",
                column: "user_id",
                principalTable: "discord_user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_reminder_message_channel_channel_id",
                table: "reminder",
                column: "channel_id",
                principalTable: "message_channel",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_replay_beatmap_competition_score_md5_hash",
                table: "replay",
                column: "md5_hash",
                principalTable: "beatmap_competition_score",
                principalColumn: "Md5Hash",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_auto_ping_mapping_forum_channel_channel_id",
                table: "auto_ping_mapping");

            migrationBuilder.DropForeignKey(
                name: "fk_beatmap_competition_guild_guild_id",
                table: "beatmap_competition");

            migrationBuilder.DropForeignKey(
                name: "fk_beatmap_competition_local_beatmap_local_beatmap_id",
                table: "beatmap_competition");

            migrationBuilder.DropForeignKey(
                name: "fk_beatmap_competition_score_beatmap_competition_guild_id_star",
                table: "beatmap_competition_score");

            migrationBuilder.DropForeignKey(
                name: "fk_beatmap_competition_score_osu_user_player_id",
                table: "beatmap_competition_score");

            migrationBuilder.DropForeignKey(
                name: "fk_channel_guild_guild_id",
                table: "channel");

            migrationBuilder.DropForeignKey(
                name: "fk_discord_message_discord_message_reference_message_id",
                table: "discord_message");

            migrationBuilder.DropForeignKey(
                name: "fk_discord_message_discord_user_author_id",
                table: "discord_message");

            migrationBuilder.DropForeignKey(
                name: "fk_discord_message_message_channel_channel_id",
                table: "discord_message");

            migrationBuilder.DropForeignKey(
                name: "fk_discord_message_version_discord_message_message_id",
                table: "discord_message_version");

            migrationBuilder.DropForeignKey(
                name: "fk_forum_channel_channel_id",
                table: "forum_channel");

            migrationBuilder.DropForeignKey(
                name: "fk_group_role_mapping_guild_guild_id",
                table: "group_role_mapping");

            migrationBuilder.DropForeignKey(
                name: "fk_guild_channel_mapfeed_channel_id",
                table: "guild");

            migrationBuilder.DropForeignKey(
                name: "fk_join_message_guild_guild_id",
                table: "join_message");

            migrationBuilder.DropForeignKey(
                name: "fk_join_message_message_channel_channel_id",
                table: "join_message");

            migrationBuilder.DropForeignKey(
                name: "fk_local_beatmap_online_beatmap_online_beatmap_id",
                table: "local_beatmap");

            migrationBuilder.DropForeignKey(
                name: "fk_message_channel_channel_id",
                table: "message_channel");

            migrationBuilder.DropForeignKey(
                name: "fk_online_beatmap_online_beatmapset_online_beatmapset_id",
                table: "online_beatmap");

            migrationBuilder.DropForeignKey(
                name: "fk_online_beatmap_osu_user_creator_id",
                table: "online_beatmap");

            migrationBuilder.DropForeignKey(
                name: "fk_online_beatmapset_osu_user_creator_id",
                table: "online_beatmapset");

            migrationBuilder.DropForeignKey(
                name: "fk_osu_user_discord_user_discord_user_id",
                table: "osu_user");

            migrationBuilder.DropForeignKey(
                name: "fk_reminder_discord_user_user_id",
                table: "reminder");

            migrationBuilder.DropForeignKey(
                name: "fk_reminder_message_channel_channel_id",
                table: "reminder");

            migrationBuilder.DropForeignKey(
                name: "fk_replay_beatmap_competition_score_md5_hash",
                table: "replay");

            migrationBuilder.DropPrimaryKey(
                name: "pk_replay",
                table: "replay");

            migrationBuilder.DropPrimaryKey(
                name: "pk_reminder",
                table: "reminder");

            migrationBuilder.DropPrimaryKey(
                name: "pk_guild",
                table: "guild");

            migrationBuilder.DropPrimaryKey(
                name: "PK_channel",
                table: "channel");

            migrationBuilder.DropPrimaryKey(
                name: "pk_osu_user",
                table: "osu_user");

            migrationBuilder.DropPrimaryKey(
                name: "pk_online_beatmapset",
                table: "online_beatmapset");

            migrationBuilder.DropPrimaryKey(
                name: "pk_online_beatmap",
                table: "online_beatmap");

            migrationBuilder.DropPrimaryKey(
                name: "PK_message_channel",
                table: "message_channel");

            migrationBuilder.DropPrimaryKey(
                name: "pk_local_beatmap",
                table: "local_beatmap");

            migrationBuilder.DropPrimaryKey(
                name: "pk_join_message",
                table: "join_message");

            migrationBuilder.DropPrimaryKey(
                name: "pk_group_role_mapping",
                table: "group_role_mapping");

            migrationBuilder.DropPrimaryKey(
                name: "PK_forum_channel",
                table: "forum_channel");

            migrationBuilder.DropPrimaryKey(
                name: "pk_discord_user",
                table: "discord_user");

            migrationBuilder.DropPrimaryKey(
                name: "pk_discord_message_version",
                table: "discord_message_version");

            migrationBuilder.DropPrimaryKey(
                name: "pk_discord_message",
                table: "discord_message");

            migrationBuilder.DropPrimaryKey(
                name: "pk_beatmap_competition_score",
                table: "beatmap_competition_score");

            migrationBuilder.DropPrimaryKey(
                name: "pk_beatmap_competition",
                table: "beatmap_competition");

            migrationBuilder.DropPrimaryKey(
                name: "pk_auto_ping_mapping",
                table: "auto_ping_mapping");

            migrationBuilder.RenameTable(
                name: "replay",
                newName: "Replay");

            migrationBuilder.RenameTable(
                name: "reminder",
                newName: "Reminder");

            migrationBuilder.RenameTable(
                name: "guild",
                newName: "Guild");

            migrationBuilder.RenameTable(
                name: "channel",
                newName: "Channel");

            migrationBuilder.RenameTable(
                name: "osu_user",
                newName: "OsuUser");

            migrationBuilder.RenameTable(
                name: "online_beatmapset",
                newName: "OnlineBeatmapset");

            migrationBuilder.RenameTable(
                name: "online_beatmap",
                newName: "OnlineBeatmap");

            migrationBuilder.RenameTable(
                name: "message_channel",
                newName: "MessageChannel");

            migrationBuilder.RenameTable(
                name: "local_beatmap",
                newName: "LocalBeatmap");

            migrationBuilder.RenameTable(
                name: "join_message",
                newName: "JoinMessage");

            migrationBuilder.RenameTable(
                name: "group_role_mapping",
                newName: "GroupRoleMapping");

            migrationBuilder.RenameTable(
                name: "forum_channel",
                newName: "ForumChannel");

            migrationBuilder.RenameTable(
                name: "discord_user",
                newName: "DiscordUser");

            migrationBuilder.RenameTable(
                name: "discord_message_version",
                newName: "DiscordMessageVersion");

            migrationBuilder.RenameTable(
                name: "discord_message",
                newName: "DiscordMessage");

            migrationBuilder.RenameTable(
                name: "beatmap_competition_score",
                newName: "BeatmapCompetitionScore");

            migrationBuilder.RenameTable(
                name: "beatmap_competition",
                newName: "BeatmapCompetition");

            migrationBuilder.RenameTable(
                name: "auto_ping_mapping",
                newName: "AutoPingMapping");

            migrationBuilder.RenameColumn(
                name: "data",
                table: "Replay",
                newName: "Data");

            migrationBuilder.RenameColumn(
                name: "md5_hash",
                table: "Replay",
                newName: "Md5Hash");

            migrationBuilder.RenameColumn(
                name: "timestamp",
                table: "Reminder",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "message",
                table: "Reminder",
                newName: "Message");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Reminder",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "Reminder",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "channel_id",
                table: "Reminder",
                newName: "ChannelId");

            migrationBuilder.RenameIndex(
                name: "ix_reminder_user_id",
                table: "Reminder",
                newName: "IX_Reminder_UserId");

            migrationBuilder.RenameIndex(
                name: "ix_reminder_channel_id",
                table: "Reminder",
                newName: "IX_Reminder_ChannelId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Guild",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "unverified_role_id",
                table: "Guild",
                newName: "UnverifiedRoleId");

            migrationBuilder.RenameColumn(
                name: "track_messages",
                table: "Guild",
                newName: "TrackMessages");

            migrationBuilder.RenameColumn(
                name: "mapfeed_channel_id",
                table: "Guild",
                newName: "MapfeedChannelId");

            migrationBuilder.RenameIndex(
                name: "ix_guild_unverified_role_id",
                table: "Guild",
                newName: "IX_Guild_UnverifiedRoleId");

            migrationBuilder.RenameIndex(
                name: "ix_guild_mapfeed_channel_id",
                table: "Guild",
                newName: "IX_Guild_MapfeedChannelId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Channel",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "guild_id",
                table: "Channel",
                newName: "GuildId");

            migrationBuilder.RenameIndex(
                name: "ix_channel_guild_id",
                table: "Channel",
                newName: "IX_Channel_GuildId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "OsuUser",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "discord_user_id",
                table: "OsuUser",
                newName: "DiscordUserId");

            migrationBuilder.RenameIndex(
                name: "ix_osu_user_discord_user_id",
                table: "OsuUser",
                newName: "IX_OsuUser_DiscordUserId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "OnlineBeatmapset",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "creator_id",
                table: "OnlineBeatmapset",
                newName: "CreatorId");

            migrationBuilder.RenameIndex(
                name: "ix_online_beatmapset_creator_id",
                table: "OnlineBeatmapset",
                newName: "IX_OnlineBeatmapset_CreatorId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "OnlineBeatmap",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "online_beatmapset_id",
                table: "OnlineBeatmap",
                newName: "OnlineBeatmapsetId");

            migrationBuilder.RenameColumn(
                name: "creator_id",
                table: "OnlineBeatmap",
                newName: "CreatorId");

            migrationBuilder.RenameIndex(
                name: "ix_online_beatmap_online_beatmapset_id",
                table: "OnlineBeatmap",
                newName: "IX_OnlineBeatmap_OnlineBeatmapsetId");

            migrationBuilder.RenameIndex(
                name: "ix_online_beatmap_creator_id",
                table: "OnlineBeatmap",
                newName: "IX_OnlineBeatmap_CreatorId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "MessageChannel",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "is_read_only",
                table: "MessageChannel",
                newName: "IsReadOnly");

            migrationBuilder.RenameColumn(
                name: "osz_hash",
                table: "LocalBeatmap",
                newName: "OszHash");

            migrationBuilder.RenameColumn(
                name: "osu_text",
                table: "LocalBeatmap",
                newName: "OsuText");

            migrationBuilder.RenameColumn(
                name: "online_beatmap_id",
                table: "LocalBeatmap",
                newName: "OnlineBeatmapId");

            migrationBuilder.RenameColumn(
                name: "max_combo",
                table: "LocalBeatmap",
                newName: "MaxCombo");

            migrationBuilder.RenameColumn(
                name: "md5_hash",
                table: "LocalBeatmap",
                newName: "Md5Hash");

            migrationBuilder.RenameIndex(
                name: "ix_local_beatmap_online_beatmap_id",
                table: "LocalBeatmap",
                newName: "IX_LocalBeatmap_OnlineBeatmapId");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "JoinMessage",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "JoinMessage",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "channel_id",
                table: "JoinMessage",
                newName: "ChannelId");

            migrationBuilder.RenameColumn(
                name: "guild_id",
                table: "JoinMessage",
                newName: "GuildId");

            migrationBuilder.RenameIndex(
                name: "ix_join_message_channel_id",
                table: "JoinMessage",
                newName: "IX_JoinMessage_ChannelId");

            migrationBuilder.RenameColumn(
                name: "group_id",
                table: "GroupRoleMapping",
                newName: "GroupId");

            migrationBuilder.RenameColumn(
                name: "role_id",
                table: "GroupRoleMapping",
                newName: "RoleId");

            migrationBuilder.RenameColumn(
                name: "guild_id",
                table: "GroupRoleMapping",
                newName: "GuildId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "ForumChannel",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "DiscordUser",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "time_zone_id",
                table: "DiscordUser",
                newName: "TimeZoneId");

            migrationBuilder.RenameColumn(
                name: "timestamp",
                table: "DiscordMessageVersion",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "raw_content",
                table: "DiscordMessageVersion",
                newName: "RawContent");

            migrationBuilder.RenameColumn(
                name: "clean_content",
                table: "DiscordMessageVersion",
                newName: "CleanContent");

            migrationBuilder.RenameColumn(
                name: "message_id",
                table: "DiscordMessageVersion",
                newName: "MessageId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "DiscordMessage",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "reference_message_id",
                table: "DiscordMessage",
                newName: "ReferenceMessageId");

            migrationBuilder.RenameColumn(
                name: "is_hidden",
                table: "DiscordMessage",
                newName: "IsHidden");

            migrationBuilder.RenameColumn(
                name: "deleted_at",
                table: "DiscordMessage",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "channel_id",
                table: "DiscordMessage",
                newName: "ChannelId");

            migrationBuilder.RenameColumn(
                name: "author_id",
                table: "DiscordMessage",
                newName: "AuthorId");

            migrationBuilder.RenameIndex(
                name: "ix_discord_message_reference_message_id",
                table: "DiscordMessage",
                newName: "IX_DiscordMessage_ReferenceMessageId");

            migrationBuilder.RenameIndex(
                name: "ix_discord_message_channel_id",
                table: "DiscordMessage",
                newName: "IX_DiscordMessage_ChannelId");

            migrationBuilder.RenameIndex(
                name: "ix_discord_message_author_id",
                table: "DiscordMessage",
                newName: "IX_DiscordMessage_AuthorId");

            migrationBuilder.RenameColumn(
                name: "mods",
                table: "BeatmapCompetitionScore",
                newName: "Mods");

            migrationBuilder.RenameColumn(
                name: "accuracy",
                table: "BeatmapCompetitionScore",
                newName: "Accuracy");

            migrationBuilder.RenameColumn(
                name: "total_score",
                table: "BeatmapCompetitionScore",
                newName: "TotalScore");

            migrationBuilder.RenameColumn(
                name: "start_time",
                table: "BeatmapCompetitionScore",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "player_id",
                table: "BeatmapCompetitionScore",
                newName: "PlayerId");

            migrationBuilder.RenameColumn(
                name: "online_id",
                table: "BeatmapCompetitionScore",
                newName: "OnlineId");

            migrationBuilder.RenameColumn(
                name: "ok_count",
                table: "BeatmapCompetitionScore",
                newName: "OkCount");

            migrationBuilder.RenameColumn(
                name: "miss_count",
                table: "BeatmapCompetitionScore",
                newName: "MissCount");

            migrationBuilder.RenameColumn(
                name: "meh_count",
                table: "BeatmapCompetitionScore",
                newName: "MehCount");

            migrationBuilder.RenameColumn(
                name: "max_combo",
                table: "BeatmapCompetitionScore",
                newName: "MaxCombo");

            migrationBuilder.RenameColumn(
                name: "guild_id",
                table: "BeatmapCompetitionScore",
                newName: "GuildId");

            migrationBuilder.RenameColumn(
                name: "great_count",
                table: "BeatmapCompetitionScore",
                newName: "GreatCount");

            migrationBuilder.RenameColumn(
                name: "date_time",
                table: "BeatmapCompetitionScore",
                newName: "DateTime");

            migrationBuilder.RenameIndex(
                name: "ix_beatmap_competition_score_player_id",
                table: "BeatmapCompetitionScore",
                newName: "IX_BeatmapCompetitionScore_PlayerId");

            migrationBuilder.RenameIndex(
                name: "ix_beatmap_competition_score_online_id",
                table: "BeatmapCompetitionScore",
                newName: "IX_BeatmapCompetitionScore_OnlineId");

            migrationBuilder.RenameIndex(
                name: "ix_beatmap_competition_score_guild_id_start_time",
                table: "BeatmapCompetitionScore",
                newName: "IX_BeatmapCompetitionScore_GuildId_StartTime");

            migrationBuilder.RenameColumn(
                name: "local_beatmap_id",
                table: "BeatmapCompetition",
                newName: "LocalBeatmapId");

            migrationBuilder.RenameColumn(
                name: "end_time",
                table: "BeatmapCompetition",
                newName: "EndTime");

            migrationBuilder.RenameColumn(
                name: "start_time",
                table: "BeatmapCompetition",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "guild_id",
                table: "BeatmapCompetition",
                newName: "GuildId");

            migrationBuilder.RenameIndex(
                name: "ix_beatmap_competition_local_beatmap_id",
                table: "BeatmapCompetition",
                newName: "IX_BeatmapCompetition_LocalBeatmapId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "AutoPingMapping",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tag_id",
                table: "AutoPingMapping",
                newName: "TagId");

            migrationBuilder.RenameColumn(
                name: "role_id",
                table: "AutoPingMapping",
                newName: "RoleId");

            migrationBuilder.RenameColumn(
                name: "channel_id",
                table: "AutoPingMapping",
                newName: "ChannelId");

            migrationBuilder.RenameIndex(
                name: "ix_auto_ping_mapping_channel_id_tag_id_role_id",
                table: "AutoPingMapping",
                newName: "IX_AutoPingMapping_ChannelId_TagId_RoleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Replay",
                table: "Replay",
                column: "Md5Hash");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reminder",
                table: "Reminder",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Guild",
                table: "Guild",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Channel",
                table: "Channel",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OsuUser",
                table: "OsuUser",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OnlineBeatmapset",
                table: "OnlineBeatmapset",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OnlineBeatmap",
                table: "OnlineBeatmap",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MessageChannel",
                table: "MessageChannel",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LocalBeatmap",
                table: "LocalBeatmap",
                column: "Md5Hash");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JoinMessage",
                table: "JoinMessage",
                column: "GuildId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupRoleMapping",
                table: "GroupRoleMapping",
                columns: new[] { "GuildId", "RoleId", "GroupId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ForumChannel",
                table: "ForumChannel",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DiscordUser",
                table: "DiscordUser",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DiscordMessageVersion",
                table: "DiscordMessageVersion",
                columns: new[] { "MessageId", "Timestamp" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_DiscordMessage",
                table: "DiscordMessage",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BeatmapCompetitionScore",
                table: "BeatmapCompetitionScore",
                column: "Md5Hash");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BeatmapCompetition",
                table: "BeatmapCompetition",
                columns: new[] { "GuildId", "StartTime" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AutoPingMapping",
                table: "AutoPingMapping",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AutoPingMapping_ForumChannel_ChannelId",
                table: "AutoPingMapping",
                column: "ChannelId",
                principalTable: "ForumChannel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BeatmapCompetition_Guild_GuildId",
                table: "BeatmapCompetition",
                column: "GuildId",
                principalTable: "Guild",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BeatmapCompetition_LocalBeatmap_LocalBeatmapId",
                table: "BeatmapCompetition",
                column: "LocalBeatmapId",
                principalTable: "LocalBeatmap",
                principalColumn: "Md5Hash",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BeatmapCompetitionScore_BeatmapCompetition_GuildId_StartTime",
                table: "BeatmapCompetitionScore",
                columns: new[] { "GuildId", "StartTime" },
                principalTable: "BeatmapCompetition",
                principalColumns: new[] { "GuildId", "StartTime" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BeatmapCompetitionScore_OsuUser_PlayerId",
                table: "BeatmapCompetitionScore",
                column: "PlayerId",
                principalTable: "OsuUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Channel_Guild_GuildId",
                table: "Channel",
                column: "GuildId",
                principalTable: "Guild",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DiscordMessage_DiscordMessage_ReferenceMessageId",
                table: "DiscordMessage",
                column: "ReferenceMessageId",
                principalTable: "DiscordMessage",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DiscordMessage_DiscordUser_AuthorId",
                table: "DiscordMessage",
                column: "AuthorId",
                principalTable: "DiscordUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DiscordMessage_MessageChannel_ChannelId",
                table: "DiscordMessage",
                column: "ChannelId",
                principalTable: "MessageChannel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DiscordMessageVersion_DiscordMessage_MessageId",
                table: "DiscordMessageVersion",
                column: "MessageId",
                principalTable: "DiscordMessage",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ForumChannel_Channel_Id",
                table: "ForumChannel",
                column: "Id",
                principalTable: "Channel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupRoleMapping_Guild_GuildId",
                table: "GroupRoleMapping",
                column: "GuildId",
                principalTable: "Guild",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Guild_Channel_MapfeedChannelId",
                table: "Guild",
                column: "MapfeedChannelId",
                principalTable: "Channel",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_JoinMessage_Guild_GuildId",
                table: "JoinMessage",
                column: "GuildId",
                principalTable: "Guild",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JoinMessage_MessageChannel_ChannelId",
                table: "JoinMessage",
                column: "ChannelId",
                principalTable: "MessageChannel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LocalBeatmap_OnlineBeatmap_OnlineBeatmapId",
                table: "LocalBeatmap",
                column: "OnlineBeatmapId",
                principalTable: "OnlineBeatmap",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageChannel_Channel_Id",
                table: "MessageChannel",
                column: "Id",
                principalTable: "Channel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OnlineBeatmap_OnlineBeatmapset_OnlineBeatmapsetId",
                table: "OnlineBeatmap",
                column: "OnlineBeatmapsetId",
                principalTable: "OnlineBeatmapset",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OnlineBeatmap_OsuUser_CreatorId",
                table: "OnlineBeatmap",
                column: "CreatorId",
                principalTable: "OsuUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OnlineBeatmapset_OsuUser_CreatorId",
                table: "OnlineBeatmapset",
                column: "CreatorId",
                principalTable: "OsuUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OsuUser_DiscordUser_DiscordUserId",
                table: "OsuUser",
                column: "DiscordUserId",
                principalTable: "DiscordUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reminder_DiscordUser_UserId",
                table: "Reminder",
                column: "UserId",
                principalTable: "DiscordUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reminder_MessageChannel_ChannelId",
                table: "Reminder",
                column: "ChannelId",
                principalTable: "MessageChannel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Replay_BeatmapCompetitionScore_Md5Hash",
                table: "Replay",
                column: "Md5Hash",
                principalTable: "BeatmapCompetitionScore",
                principalColumn: "Md5Hash",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
