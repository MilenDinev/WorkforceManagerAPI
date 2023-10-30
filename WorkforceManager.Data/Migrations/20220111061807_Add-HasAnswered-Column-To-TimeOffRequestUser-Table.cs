using Microsoft.EntityFrameworkCore.Migrations;

namespace WorkforceManager.Data.Migrations
{
    public partial class AddHasAnsweredColumnToTimeOffRequestUserTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasAnswered",
                table: "TimeOffRequestUser",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasAnswered",
                table: "TimeOffRequestUser");
        }
    }
}
