using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTestResultNewColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CalledUrl",
                table: "TestResults",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CalledUrl",
                table: "TestResults");
        }
    }
}
