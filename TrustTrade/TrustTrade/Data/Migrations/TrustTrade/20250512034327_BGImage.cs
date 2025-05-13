using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrustTrade.Data.Migrations.TrustTrade
{
    /// <inheritdoc />
    public partial class BGImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "BackgroundImage",
                table: "Users",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BackgroundImageUrl",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BackgroundPosition",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                defaultValue: "center");

            migrationBuilder.AddColumn<string>(
                name: "BackgroundSource",
                table: "Users",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                defaultValue: "File");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackgroundImage",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BackgroundImageUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BackgroundPosition",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BackgroundSource",
                table: "Users");
        }
    }
}
