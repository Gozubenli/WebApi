using Microsoft.EntityFrameworkCore.Migrations;

namespace WebApi.Migrations
{
    public partial class _42 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Employees",
                newName: "Password");

            migrationBuilder.AddColumn<string>(
                name: "ImageName",
                table: "Employees",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageName",
                table: "Employees");

            migrationBuilder.RenameColumn(
                name: "Password",
                table: "Employees",
                newName: "ImageUrl");
        }
    }
}
