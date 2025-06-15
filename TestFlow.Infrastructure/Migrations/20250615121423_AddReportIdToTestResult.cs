using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReportIdToTestResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestResults_TestReports_TestReportId",
                table: "TestResults");

            migrationBuilder.RenameColumn(
                name: "TestReportId",
                table: "TestResults",
                newName: "ReportId");

            migrationBuilder.RenameIndex(
                name: "IX_TestResults_TestReportId",
                table: "TestResults",
                newName: "IX_TestResults_ReportId");

            migrationBuilder.AddForeignKey(
                name: "FK_TestResults_TestReports_ReportId",
                table: "TestResults",
                column: "ReportId",
                principalTable: "TestReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestResults_TestReports_ReportId",
                table: "TestResults");

            migrationBuilder.RenameColumn(
                name: "ReportId",
                table: "TestResults",
                newName: "TestReportId");

            migrationBuilder.RenameIndex(
                name: "IX_TestResults_ReportId",
                table: "TestResults",
                newName: "IX_TestResults_TestReportId");

            migrationBuilder.AddForeignKey(
                name: "FK_TestResults_TestReports_TestReportId",
                table: "TestResults",
                column: "TestReportId",
                principalTable: "TestReports",
                principalColumn: "Id");
        }
    }
}
