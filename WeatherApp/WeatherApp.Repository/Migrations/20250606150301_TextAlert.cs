using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeatherApp.Repository.Migrations
{
    /// <inheritdoc />
    public partial class TextAlert : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RecommendationText",
                table: "WeatherAlerts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecommendationText",
                table: "WeatherAlerts");
        }
    }
}
