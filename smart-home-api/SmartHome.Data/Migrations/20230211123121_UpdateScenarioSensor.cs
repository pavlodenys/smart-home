using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartHome.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateScenarioSensor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "ScenarioSensors");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeviceId",
                table: "ScenarioSensors",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
