using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrustTrade.Data.Migrations.TrustTrade
{
    /// <inheritdoc />
    public partial class NotificationSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "NotificationSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    EnableFollowNotifications = table.Column<bool>(type: "bit", nullable: false),
                    EnableLikeNotifications = table.Column<bool>(type: "bit", nullable: false),
                    EnableCommentNotifications = table.Column<bool>(type: "bit", nullable: false),
                    EnableMentionNotifications = table.Column<bool>(type: "bit", nullable: false),
                    EnableMessageNotifications = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationSettings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationSettings_UserId",
                table: "NotificationSettings",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationSettings");

        }
    }
}
