using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartHome.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateChartData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sensors_Data_DataId",
                table: "Sensors");

            migrationBuilder.DropIndex(
                name: "IX_Sensors_DataId",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "DataId",
                table: "Sensors");

            migrationBuilder.AddColumn<int>(
                name: "SensorId",
                table: "Data",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Data_SensorId",
                table: "Data",
                column: "SensorId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Data_Sensors_SensorId",
            //    table: "Data",
            //    column: "SensorId",
            //    principalTable: "Sensors",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_Data_Sensors_SensorId",
            //    table: "Data");

            migrationBuilder.DropIndex(
                name: "IX_Data_SensorId",
                table: "Data");

            migrationBuilder.DropColumn(
                name: "SensorId",
                table: "Data");

            migrationBuilder.AddColumn<int>(
                name: "DataId",
                table: "Sensors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_DataId",
                table: "Sensors",
                column: "DataId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sensors_Data_DataId",
                table: "Sensors",
                column: "DataId",
                principalTable: "Data",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
