using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTestCaseNewColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomUrl",
                table: "TestCases",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomUrl",
                table: "TestCases");
        }
    }
}
