using Microsoft.EntityFrameworkCore.Migrations;

namespace WebApi.Migrations
{
    public partial class _27 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MenuColor2",
                table: "WebSettings",
                newName: "TitleColor");

            migrationBuilder.RenameColumn(
                name: "MenuColor1",
                table: "WebSettings",
                newName: "MenuColor");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TitleColor",
                table: "WebSettings",
                newName: "MenuColor2");

            migrationBuilder.RenameColumn(
                name: "MenuColor",
                table: "WebSettings",
                newName: "MenuColor1");
        }
    }
}
