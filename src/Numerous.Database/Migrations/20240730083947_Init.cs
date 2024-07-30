using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Numerous.Database.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiscordUser",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    TimeZoneId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordUser", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Guild",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    TrackMessages = table.Column<bool>(type: "boolean", nullable: false),
                    UnverifiedRoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guild", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OsuUser",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    DiscordUserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OsuUser", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OsuUser_DiscordUser_DiscordUserId",
                        column: x => x.DiscordUserId,
                        principalTable: "DiscordUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Channel",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Channel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Channel_Guild_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guild",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupRoleMapping",
                columns: table => new
                {
                    GroupId = table.Column<short>(type: "smallint", nullable: false),
                    RoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupRoleMapping", x => new { x.GuildId, x.RoleId, x.GroupId });
                    table.ForeignKey(
                        name: "FK_GroupRoleMapping_Guild_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guild",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ForumChannel",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForumChannel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForumChannel_Channel_Id",
                        column: x => x.Id,
                        principalTable: "Channel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MessageChannel",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    IsReadOnly = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageChannel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageChannel_Channel_Id",
                        column: x => x.Id,
                        principalTable: "Channel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AutoPingMapping",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    TagId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    RoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutoPingMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutoPingMapping_ForumChannel_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "ForumChannel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiscordMessage",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false),
                    ReferenceMessageId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    AuthorId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordMessage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscordMessage_DiscordMessage_ReferenceMessageId",
                        column: x => x.ReferenceMessageId,
                        principalTable: "DiscordMessage",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DiscordMessage_DiscordUser_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "DiscordUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiscordMessage_MessageChannel_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "MessageChannel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JoinMessage",
                columns: table => new
                {
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Description = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JoinMessage", x => x.GuildId);
                    table.CheckConstraint("CK_JoinMessage_HasText", "\"Title\" IS NOT NULL OR \"Description\" IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_JoinMessage_Guild_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guild",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JoinMessage_MessageChannel_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "MessageChannel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reminder",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Message = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: true),
                    UserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reminder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reminder_DiscordUser_UserId",
                        column: x => x.UserId,
                        principalTable: "DiscordUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reminder_MessageChannel_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "MessageChannel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiscordMessageVersion",
                columns: table => new
                {
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    MessageId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    RawContent = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    CleanContent = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true, comment: "If NULL, the clean content is the same as the raw content.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordMessageVersion", x => new { x.MessageId, x.Timestamp });
                    table.ForeignKey(
                        name: "FK_DiscordMessageVersion_DiscordMessage_MessageId",
                        column: x => x.MessageId,
                        principalTable: "DiscordMessage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AutoPingMapping_ChannelId_TagId_RoleId",
                table: "AutoPingMapping",
                columns: new[] { "ChannelId", "TagId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Channel_GuildId",
                table: "Channel",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordMessage_AuthorId",
                table: "DiscordMessage",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordMessage_ChannelId",
                table: "DiscordMessage",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordMessage_ReferenceMessageId",
                table: "DiscordMessage",
                column: "ReferenceMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Guild_UnverifiedRoleId",
                table: "Guild",
                column: "UnverifiedRoleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JoinMessage_ChannelId",
                table: "JoinMessage",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_OsuUser_DiscordUserId",
                table: "OsuUser",
                column: "DiscordUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reminder_ChannelId",
                table: "Reminder",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_Reminder_UserId",
                table: "Reminder",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutoPingMapping");

            migrationBuilder.DropTable(
                name: "DiscordMessageVersion");

            migrationBuilder.DropTable(
                name: "GroupRoleMapping");

            migrationBuilder.DropTable(
                name: "JoinMessage");

            migrationBuilder.DropTable(
                name: "OsuUser");

            migrationBuilder.DropTable(
                name: "Reminder");

            migrationBuilder.DropTable(
                name: "ForumChannel");

            migrationBuilder.DropTable(
                name: "DiscordMessage");

            migrationBuilder.DropTable(
                name: "DiscordUser");

            migrationBuilder.DropTable(
                name: "MessageChannel");

            migrationBuilder.DropTable(
                name: "Channel");

            migrationBuilder.DropTable(
                name: "Guild");
        }
    }
}
