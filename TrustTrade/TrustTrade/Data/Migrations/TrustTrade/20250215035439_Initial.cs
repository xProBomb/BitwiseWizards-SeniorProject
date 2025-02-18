using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrustTrade.Data.Migrations.TrustTrade
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Stock",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TickerSymbol = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    StockPrice = table.Column<decimal>(type: "decimal(13,2)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Stock__3214EC2751A28057", x => x.ID);
                    table.UniqueConstraint("AK_Stock_TickerSymbol", x => x.TickerSymbol);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Profile_Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    ProfilePicture = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Bio = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Is_Admin = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    Is_Verified = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    EncryptedAPIKey = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    PlaidEnabled = table.Column<bool>(type: "bit", nullable: true),
                    LastPlaidSync = table.Column<DateTime>(type: "datetime", nullable: true),
                    PlaidStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PlaidSettings = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__3214EC27D63A1A35", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Followers",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FollowerUserID = table.Column<int>(type: "int", nullable: false),
                    FollowingUserID = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Follower__3214EC276AFC0274", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Followers_Follower",
                        column: x => x.FollowerUserID,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Followers_Following",
                        column: x => x.FollowingUserID,
                        principalTable: "Users",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "PlaidConnection",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ItemID = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AccessToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InstitutionID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InstitutionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastSyncTimestamp = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaidConnection", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PlaidConnection_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    Tag = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PrivacySetting = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Public")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Posts__3214EC2757B225A3", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Posts_User",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Trade",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    TickerSymbol = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    TradeType = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    EntryPrice = table.Column<decimal>(type: "decimal(13,2)", nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "decimal(13,2)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Trade__3214EC271BE9AF7E", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Trade_Stock",
                        column: x => x.TickerSymbol,
                        principalTable: "Stock",
                        principalColumn: "TickerSymbol");
                    table.ForeignKey(
                        name: "FK_Trade_User",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvestmentPosition",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlaidConnectionID = table.Column<int>(type: "int", nullable: false),
                    SecurityID = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    CostBasis = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestmentPosition", x => x.ID);
                    table.ForeignKey(
                        name: "FK_InvestmentPosition_PlaidConnection_PlaidConnectionID",
                        column: x => x.PlaidConnectionID,
                        principalTable: "PlaidConnection",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Comments__3214EC275D142D0B", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Comments_Post",
                        column: x => x.PostID,
                        principalTable: "Posts",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comments_User",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Likes",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    PostID = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Likes__3214EC27FF87C594", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Likes_Post",
                        column: x => x.PostID,
                        principalTable: "Posts",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Likes_User",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_PostID",
                table: "Comments",
                column: "PostID");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserID",
                table: "Comments",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Followers_FollowerUserID",
                table: "Followers",
                column: "FollowerUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Followers_FollowingUserID",
                table: "Followers",
                column: "FollowingUserID");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentPosition_PlaidConnectionID",
                table: "InvestmentPosition",
                column: "PlaidConnectionID");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_PostID",
                table: "Likes",
                column: "PostID");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_UserID",
                table: "Likes",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_PlaidConnection_UserID",
                table: "PlaidConnection",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "UQ__PlaidCon__727E83EAA3D35DB3",
                table: "PlaidConnection",
                column: "ItemID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Posts_UserID",
                table: "Posts",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "UQ__Stock__F144591B01402E14",
                table: "Stock",
                column: "TickerSymbol",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trade_TickerSymbol",
                table: "Trade",
                column: "TickerSymbol");

            migrationBuilder.CreateIndex(
                name: "IX_Trade_UserID",
                table: "Trade",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "UQ__Users__536C85E43599A86E",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Users__A9D10534AB940664",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "Followers");

            migrationBuilder.DropTable(
                name: "InvestmentPosition");

            migrationBuilder.DropTable(
                name: "Likes");

            migrationBuilder.DropTable(
                name: "Trade");

            migrationBuilder.DropTable(
                name: "PlaidConnection");

            migrationBuilder.DropTable(
                name: "Posts");

            migrationBuilder.DropTable(
                name: "Stock");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
