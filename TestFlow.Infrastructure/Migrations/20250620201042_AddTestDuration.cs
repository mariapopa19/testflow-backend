using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTestDuration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "Duration",
                table: "TestRuns",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FinishedAt",
                table: "TestRuns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartedAt",
                table: "TestResults",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Duration",
                table: "TestResults",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FinishedAt",
                table: "TestResults",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "TestRuns");

            migrationBuilder.DropColumn(
                name: "FinishedAt",
                table: "TestRuns");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "TestResults");

            migrationBuilder.DropColumn(
                name: "FinishedAt",
                table: "TestResults");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartedAt",
                table: "TestResults",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }
    }
}
