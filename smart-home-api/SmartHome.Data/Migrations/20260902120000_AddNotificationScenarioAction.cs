using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SmartHome.Data;

#nullable disable

namespace SmartHome.Data.Migrations
{
    [DbContext(typeof(SmartHomeDbContext))]
    [Migration("20260902120000_AddNotificationScenarioAction")]
    public partial class AddNotificationScenarioAction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SensorValue",
                table: "Scenarios");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "Scenarios",
                newName: "Threshold");

            migrationBuilder.AlterColumn<double>(
                name: "Threshold",
                table: "Scenarios",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "ActionType",
                table: "Scenarios",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "Hysteresis",
                table: "Scenarios",
                type: "double precision",
                nullable: false,
                defaultValue: 2.0);

            migrationBuilder.AddColumn<bool>(
                name: "IsConditionActive",
                table: "Scenarios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastTriggeredAt",
                table: "Scenarios",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionType",
                table: "Scenarios");

            migrationBuilder.DropColumn(
                name: "Hysteresis",
                table: "Scenarios");

            migrationBuilder.DropColumn(
                name: "IsConditionActive",
                table: "Scenarios");

            migrationBuilder.DropColumn(
                name: "LastTriggeredAt",
                table: "Scenarios");

            migrationBuilder.Sql(
                "ALTER TABLE \"Scenarios\" ALTER COLUMN \"Threshold\" TYPE integer "
                + "USING ROUND(\"Threshold\")::integer;");

            migrationBuilder.RenameColumn(
                name: "Threshold",
                table: "Scenarios",
                newName: "Value");

            migrationBuilder.AddColumn<int>(
                name: "SensorValue",
                table: "Scenarios",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
