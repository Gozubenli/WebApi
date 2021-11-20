using Microsoft.EntityFrameworkCore.Migrations;

namespace WebApi.Migrations
{
    public partial class _40 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "period",
                table: "Works",
                newName: "WorkPeriodType");

            migrationBuilder.RenameColumn(
                name: "endDate",
                table: "Works",
                newName: "WorkPeriodEndDate");

            migrationBuilder.RenameColumn(
                name: "WorkPeriod",
                table: "Works",
                newName: "WorkPeriodNumber");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WorkPeriodType",
                table: "Works",
                newName: "period");

            migrationBuilder.RenameColumn(
                name: "WorkPeriodNumber",
                table: "Works",
                newName: "WorkPeriod");

            migrationBuilder.RenameColumn(
                name: "WorkPeriodEndDate",
                table: "Works",
                newName: "endDate");
        }
    }
}
