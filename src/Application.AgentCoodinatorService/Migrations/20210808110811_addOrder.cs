using Microsoft.EntityFrameworkCore.Migrations;

namespace Application.AgentCoodinatorService.Migrations
{
    public partial class addOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssignOrder",
                table: "Seniorities",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignOrder",
                table: "Seniorities");
        }
    }
}
