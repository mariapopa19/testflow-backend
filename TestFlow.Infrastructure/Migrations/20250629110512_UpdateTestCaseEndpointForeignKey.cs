using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTestCaseEndpointForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestCases_Endpoints_EndpointId",
                table: "TestCases");

            migrationBuilder.DropForeignKey(
                name: "FK_TestRuns_Endpoints_EndpointId",
                table: "TestRuns");

            migrationBuilder.AddForeignKey(
                name: "FK_TestCases_Endpoints_EndpointId",
                table: "TestCases",
                column: "EndpointId",
                principalTable: "Endpoints",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TestRuns_Endpoints_EndpointId",
                table: "TestRuns",
                column: "EndpointId",
                principalTable: "Endpoints",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestCases_Endpoints_EndpointId",
                table: "TestCases");

            migrationBuilder.DropForeignKey(
                name: "FK_TestRuns_Endpoints_EndpointId",
                table: "TestRuns");

            migrationBuilder.AddForeignKey(
                name: "FK_TestCases_Endpoints_EndpointId",
                table: "TestCases",
                column: "EndpointId",
                principalTable: "Endpoints",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TestRuns_Endpoints_EndpointId",
                table: "TestRuns",
                column: "EndpointId",
                principalTable: "Endpoints",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
