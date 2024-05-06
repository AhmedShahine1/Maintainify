using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maintainify.Core.Migrations
{
    /// <inheritdoc />
    public partial class editImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_PathFiles_pathFilesId",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Images_pathFilesId",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "pathFilesId",
                table: "Images");

            migrationBuilder.AlterColumn<string>(
                name: "PathId",
                table: "Images",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Images_PathId",
                table: "Images",
                column: "PathId");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_PathFiles_PathId",
                table: "Images",
                column: "PathId",
                principalTable: "PathFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_PathFiles_PathId",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Images_PathId",
                table: "Images");

            migrationBuilder.AlterColumn<string>(
                name: "PathId",
                table: "Images",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "pathFilesId",
                table: "Images",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Images_pathFilesId",
                table: "Images",
                column: "pathFilesId");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_PathFiles_pathFilesId",
                table: "Images",
                column: "pathFilesId",
                principalTable: "PathFiles",
                principalColumn: "Id");
        }
    }
}
