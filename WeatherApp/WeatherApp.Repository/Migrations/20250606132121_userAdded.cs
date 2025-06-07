using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeatherApp.Repository.Migrations
{
    /// <inheritdoc />
    public partial class userAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSettings_AspNetUsers_WeatherAppUserId",
                table: "UserSettings");

            migrationBuilder.DropIndex(
                name: "IX_UserSettings_WeatherAppUserId",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "WeatherAppUserId",
                table: "UserSettings");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "UserSettings",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_UserSettings_UserId",
                table: "UserSettings",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSettings_AspNetUsers_UserId",
                table: "UserSettings",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSettings_AspNetUsers_UserId",
                table: "UserSettings");

            migrationBuilder.DropIndex(
                name: "IX_UserSettings_UserId",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserSettings");

            migrationBuilder.AddColumn<string>(
                name: "WeatherAppUserId",
                table: "UserSettings",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSettings_WeatherAppUserId",
                table: "UserSettings",
                column: "WeatherAppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSettings_AspNetUsers_WeatherAppUserId",
                table: "UserSettings",
                column: "WeatherAppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
