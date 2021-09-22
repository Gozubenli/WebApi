using Microsoft.EntityFrameworkCore.Migrations;

namespace WebApi.Migrations
{
    public partial class _17 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MainColor",
                table: "AppSettings",
                newName: "TextColor");

            migrationBuilder.AddColumn<int>(
                name: "DefaultPadding",
                table: "WebSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MenuColor1",
                table: "WebSettings",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "MenuColor2",
                table: "WebSettings",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TextColor",
                table: "WebSettings",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Employees",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "DefaultPadding",
                table: "AppSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MenuColor1",
                table: "AppSettings",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "MenuColor2",
                table: "AppSettings",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PrimaryColor",
                table: "AppSettings",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultPadding",
                table: "WebSettings");

            migrationBuilder.DropColumn(
                name: "MenuColor1",
                table: "WebSettings");

            migrationBuilder.DropColumn(
                name: "MenuColor2",
                table: "WebSettings");

            migrationBuilder.DropColumn(
                name: "TextColor",
                table: "WebSettings");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DefaultPadding",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "MenuColor1",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "MenuColor2",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "PrimaryColor",
                table: "AppSettings");

            migrationBuilder.RenameColumn(
                name: "TextColor",
                table: "AppSettings",
                newName: "MainColor");
        }
    }
}
