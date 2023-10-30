using Microsoft.EntityFrameworkCore.Migrations;

namespace WorkforceManager.Data.Migrations
{
    public partial class TeamUserApproversRequestsAndUserRequestConfigurationsApplied : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeOffRequests_RequestTypes_TypeId",
                table: "TimeOffRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeOffRequests_Statuses_StatusId",
                table: "TimeOffRequests");

            migrationBuilder.DropTable(
                name: "TeamUser");

            migrationBuilder.DropTable(
                name: "TimeOffRequestUser");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Statuses",
                newName: "State");

            migrationBuilder.AlterColumn<int>(
                name: "TypeId",
                table: "TimeOffRequests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "StatusId",
                table: "TimeOffRequests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ApproverRequest",
                columns: table => new
                {
                    ApproverId = table.Column<int>(type: "int", nullable: false),
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    IsProcessed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApproverRequest", x => new { x.ApproverId, x.RequestId });
                    table.ForeignKey(
                        name: "FK_ApproverRequest_AspNetUsers_ApproverId",
                        column: x => x.ApproverId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ApproverRequest_TimeOffRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "TimeOffRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserTeams",
                columns: table => new
                {
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTeams", x => new { x.TeamId, x.UserId });
                    table.ForeignKey(
                        name: "FK_UserTeams_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserTeams_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApproverRequest_RequestId",
                table: "ApproverRequest",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTeams_UserId",
                table: "UserTeams",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeOffRequests_RequestTypes_TypeId",
                table: "TimeOffRequests",
                column: "TypeId",
                principalTable: "RequestTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeOffRequests_Statuses_StatusId",
                table: "TimeOffRequests",
                column: "StatusId",
                principalTable: "Statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeOffRequests_RequestTypes_TypeId",
                table: "TimeOffRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeOffRequests_Statuses_StatusId",
                table: "TimeOffRequests");

            migrationBuilder.DropTable(
                name: "ApproverRequest");

            migrationBuilder.DropTable(
                name: "UserTeams");

            migrationBuilder.RenameColumn(
                name: "State",
                table: "Statuses",
                newName: "Status");

            migrationBuilder.AlterColumn<int>(
                name: "TypeId",
                table: "TimeOffRequests",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "StatusId",
                table: "TimeOffRequests",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "TeamUser",
                columns: table => new
                {
                    MembersId = table.Column<int>(type: "int", nullable: false),
                    TeamsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamUser", x => new { x.MembersId, x.TeamsId });
                    table.ForeignKey(
                        name: "FK_TeamUser_AspNetUsers_MembersId",
                        column: x => x.MembersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamUser_Teams_TeamsId",
                        column: x => x.TeamsId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimeOffRequestUser",
                columns: table => new
                {
                    ApproversId = table.Column<int>(type: "int", nullable: false),
                    RequestsToApproveId = table.Column<int>(type: "int", nullable: false),
                    HasAnswered = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeOffRequestUser", x => new { x.ApproversId, x.RequestsToApproveId });
                    table.ForeignKey(
                        name: "FK_TimeOffRequestUser_AspNetUsers_ApproversId",
                        column: x => x.ApproversId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TimeOffRequestUser_TimeOffRequests_RequestsToApproveId",
                        column: x => x.RequestsToApproveId,
                        principalTable: "TimeOffRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeamUser_TeamsId",
                table: "TeamUser",
                column: "TeamsId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeOffRequestUser_RequestsToApproveId",
                table: "TimeOffRequestUser",
                column: "RequestsToApproveId");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeOffRequests_RequestTypes_TypeId",
                table: "TimeOffRequests",
                column: "TypeId",
                principalTable: "RequestTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeOffRequests_Statuses_StatusId",
                table: "TimeOffRequests",
                column: "StatusId",
                principalTable: "Statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
