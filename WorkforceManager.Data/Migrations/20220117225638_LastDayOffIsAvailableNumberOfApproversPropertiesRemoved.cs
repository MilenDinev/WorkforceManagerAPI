using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WorkforceManager.Data.Migrations
{
    public partial class LastDayOffIsAvailableNumberOfApproversPropertiesRemoved : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumberOfApprovals",
                table: "TimeOffRequests");

            migrationBuilder.DropColumn(
                name: "LastDayOff",
                table: "AspNetUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumberOfApprovals",
                table: "TimeOffRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastDayOff",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
