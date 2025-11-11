using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Numerous.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddForumTopicSubscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "forum_topic_subscription",
                columns: table => new
                {
                    forum_topic_id = table.Column<int>(type: "integer", nullable: false),
                    channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_forum_topic_subscription", x => new { x.forum_topic_id, x.channel_id });
                    table.ForeignKey(
                        name: "fk_forum_topic_subscription_message_channel_channel_id",
                        column: x => x.channel_id,
                        principalTable: "message_channel",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_forum_topic_subscription_channel_id",
                table: "forum_topic_subscription",
                column: "channel_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "forum_topic_subscription");
        }
    }
}
