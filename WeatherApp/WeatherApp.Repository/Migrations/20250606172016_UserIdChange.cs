using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeatherApp.Repository.Migrations
{
    /// <inheritdoc />
    public partial class UserIdChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSettings_AspNetUsers_UserId",
                table: "UserSettings");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserSettings",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSettings_AspNetUsers_UserId",
                table: "UserSettings",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSettings_AspNetUsers_UserId",
                table: "UserSettings");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserSettings",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSettings_AspNetUsers_UserId",
                table: "UserSettings",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
