using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrustTrade.Data.Migrations.TrustTrade
{
    /// <inheritdoc />
    public partial class FixPhotoDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Photos_Post",
                table: "Photos");

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_Post",
                table: "Photos",
                column: "PostID",
                principalTable: "Posts",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Photos_Post",
                table: "Photos");

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_Post",
                table: "Photos",
                column: "PostID",
                principalTable: "Posts",
                principalColumn: "ID");
        }
    }
}
