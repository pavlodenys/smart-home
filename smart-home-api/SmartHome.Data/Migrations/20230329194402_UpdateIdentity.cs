using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartHome.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccessFailedCount",
                table: "HomeUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "HomeUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailConfirmed",
                table: "HomeUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LockoutEnabled",
                table: "HomeUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LockoutEnd",
                table: "HomeUsers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedEmail",
                table: "HomeUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedUserName",
                table: "HomeUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "HomeUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PhoneNumberConfirmed",
                table: "HomeUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SecurityStamp",
                table: "HomeUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TwoFactorEnabled",
                table: "HomeUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessFailedCount",
                table: "HomeUsers");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "HomeUsers");

            migrationBuilder.DropColumn(
                name: "EmailConfirmed",
                table: "HomeUsers");

            migrationBuilder.DropColumn(
                name: "LockoutEnabled",
                table: "HomeUsers");

            migrationBuilder.DropColumn(
                name: "LockoutEnd",
                table: "HomeUsers");

            migrationBuilder.DropColumn(
                name: "NormalizedEmail",
                table: "HomeUsers");

            migrationBuilder.DropColumn(
                name: "NormalizedUserName",
                table: "HomeUsers");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "HomeUsers");

            migrationBuilder.DropColumn(
                name: "PhoneNumberConfirmed",
                table: "HomeUsers");

            migrationBuilder.DropColumn(
                name: "SecurityStamp",
                table: "HomeUsers");

            migrationBuilder.DropColumn(
                name: "TwoFactorEnabled",
                table: "HomeUsers");
        }
    }
}
