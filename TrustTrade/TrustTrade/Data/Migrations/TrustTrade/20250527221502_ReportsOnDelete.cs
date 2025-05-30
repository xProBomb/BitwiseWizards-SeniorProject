using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrustTrade.Data.Migrations.TrustTrade
{
    /// <inheritdoc />
    public partial class ReportsOnDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_ReportedPost",
                table: "Reports");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_ReportedPost",
                table: "Reports",
                column: "ReportedPostId",
                principalTable: "Posts",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_ReportedPost",
                table: "Reports");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_ReportedPost",
                table: "Reports",
                column: "ReportedPostId",
                principalTable: "Posts",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
