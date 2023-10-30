using Microsoft.EntityFrameworkCore.Migrations;

namespace WorkforceManager.Data.Migrations
{
    public partial class FixedThatCreatorIsNotEqualToRequester : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeOffRequests_AspNetUsers_CreatorId",
                table: "TimeOffRequests");

            migrationBuilder.DropIndex(
                name: "IX_TimeOffRequests_CreatorId",
                table: "TimeOffRequests");

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeOffRequests_AspNetUsers_RequesterId",
                table: "TimeOffRequests");

            migrationBuilder.DropIndex(
                name: "IX_TimeOffRequests_RequesterId",
                table: "TimeOffRequests");

            migrationBuilder.DropColumn(
                name: "RequesterId",
                table: "TimeOffRequests");

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
    }
}
