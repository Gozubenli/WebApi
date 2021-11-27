using Microsoft.EntityFrameworkCore.Migrations;

namespace WebApi.Migrations
{
    public partial class _44 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Employee_Role",
                table: "Employee_Role");

            migrationBuilder.RenameTable(
                name: "Employee_Role",
                newName: "Employee_Roles");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Employee_Roles",
                table: "Employee_Roles",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Employee_Roles",
                table: "Employee_Roles");

            migrationBuilder.RenameTable(
                name: "Employee_Roles",
                newName: "Employee_Role");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Employee_Role",
                table: "Employee_Role",
                column: "Id");
        }
    }
}
