using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedTestReportTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TestReportId",
                table: "TestResults",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TestReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TestRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TestType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalTests = table.Column<int>(type: "int", nullable: false),
                    PassedTests = table.Column<int>(type: "int", nullable: false),
                    FailedTests = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestReports_TestRuns_TestRunId",
                        column: x => x.TestRunId,
                        principalTable: "TestRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TestReports_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestResults_TestReportId",
                table: "TestResults",
                column: "TestReportId");

            migrationBuilder.CreateIndex(
                name: "IX_TestReports_TestRunId",
                table: "TestReports",
                column: "TestRunId");

            migrationBuilder.CreateIndex(
                name: "IX_TestReports_UserId",
                table: "TestReports",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TestResults_TestReports_TestReportId",
                table: "TestResults",
                column: "TestReportId",
                principalTable: "TestReports",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestResults_TestReports_TestReportId",
                table: "TestResults");

            migrationBuilder.DropTable(
                name: "TestReports");

            migrationBuilder.DropIndex(
                name: "IX_TestResults_TestReportId",
                table: "TestResults");

            migrationBuilder.DropColumn(
                name: "TestReportId",
                table: "TestResults");
        }
    }
}
