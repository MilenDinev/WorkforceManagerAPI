using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WorkforceManager.Data.Migrations
{
    public partial class EntityTimeOffRequest_DaysOff_To_StartDate_EndDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeOffRequests_AspNetUsers_RequesterId",
                table: "TimeOffRequests");

            migrationBuilder.DropIndex(
                name: "IX_TimeOffRequests_RequesterId",
                table: "TimeOffRequests");

            migrationBuilder.DropColumn(
                name: "DaysOff",
                table: "TimeOffRequests");

            migrationBuilder.DropColumn(
                name: "RequesterId",
                table: "TimeOffRequests");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "TimeOffRequests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "TimeOffRequests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_TimeOffRequests_CreatorId",
                table: "TimeOffRequests",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeOffRequests_AspNetUsers_CreatorId",
                table: "TimeOffRequests",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeOffRequests_AspNetUsers_CreatorId",
                table: "TimeOffRequests");

            migrationBuilder.DropIndex(
                name: "IX_TimeOffRequests_CreatorId",
                table: "TimeOffRequests");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "TimeOffRequests");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "TimeOffRequests");

            migrationBuilder.AddColumn<int>(
                name: "DaysOff",
                table: "TimeOffRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RequesterId",
                table: "TimeOffRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TimeOffRequests_RequesterId",
                table: "TimeOffRequests",
                column: "RequesterId");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeOffRequests_AspNetUsers_RequesterId",
                table: "TimeOffRequests",
                column: "RequesterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
