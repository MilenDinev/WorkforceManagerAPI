using Microsoft.EntityFrameworkCore.Migrations;

namespace WorkforceManager.Data.Migrations
{
    public partial class DeleteUserTeamBugFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApproverRequest_TimeOffRequests_RequestId",
                table: "ApproverRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeOffRequests_AspNetUsers_RequesterId",
                table: "TimeOffRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTeams_AspNetUsers_UserId",
                table: "UserTeams");

            migrationBuilder.AddForeignKey(
                name: "FK_ApproverRequest_TimeOffRequests_RequestId",
                table: "ApproverRequest",
                column: "RequestId",
                principalTable: "TimeOffRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeOffRequests_AspNetUsers_RequesterId",
                table: "TimeOffRequests",
                column: "RequesterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTeams_AspNetUsers_UserId",
                table: "UserTeams",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApproverRequest_TimeOffRequests_RequestId",
                table: "ApproverRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeOffRequests_AspNetUsers_RequesterId",
                table: "TimeOffRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTeams_AspNetUsers_UserId",
                table: "UserTeams");

            migrationBuilder.AddForeignKey(
                name: "FK_ApproverRequest_TimeOffRequests_RequestId",
                table: "ApproverRequest",
                column: "RequestId",
                principalTable: "TimeOffRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeOffRequests_AspNetUsers_RequesterId",
                table: "TimeOffRequests",
                column: "RequesterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTeams_AspNetUsers_UserId",
                table: "UserTeams",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
