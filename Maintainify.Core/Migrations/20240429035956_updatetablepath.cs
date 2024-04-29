using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maintainify.Core.Migrations
{
    /// <inheritdoc />
    public partial class updatetablepath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "type",
                table: "PathFiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "type",
                table: "PathFiles");
        }
    }
}
