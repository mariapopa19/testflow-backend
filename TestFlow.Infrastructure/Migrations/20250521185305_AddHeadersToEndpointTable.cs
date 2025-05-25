using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHeadersToEndpointTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HeadersJson",
                table: "Endpoints",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeadersJson",
                table: "Endpoints");
        }
    }
}
