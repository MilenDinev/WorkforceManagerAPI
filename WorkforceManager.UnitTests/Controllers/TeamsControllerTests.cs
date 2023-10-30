

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using WorkforceManager.Data.Entities;
using WorkforceManager.Models.Requests.TeamRequestModels;
using WorkforceManager.Models.Responses.TeamResponseModels;
using WorkforceManager.Services.Interfaces;
using WorkforceManager.UnitTests.Mocks;
using WorkforceManager.Web.Controllers;
using Xunit;

namespace WorkforceManager.UnitTests.Controllers
{
    public class TeamsControllerTests
    {
        private const int CURRENT_USER_ID = 1;

        [Fact]
        public async Task CreateTeam_ValidData_ReturnsCreated()
        {

            var teamService = new Mock<ITeamService>();
            var sut = Setup(teamService.Object);

            var teamRequest = new TeamCreateRequestModel
            {
                Title = "Team",
            };

            teamService.Setup(t => t.CreateAsync(teamRequest, CURRENT_USER_ID)).ReturnsAsync(new CreatedTeamResponseModel());

            var result = await sut.Post(teamRequest);

            teamService.Verify(service => service.CreateAsync(teamRequest, CURRENT_USER_ID), Times.Once);
            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task CreateTeam_UnauthorizedUser_ThrowsException()
        {

            var teamService = new Mock<ITeamService>();
            var sut = SetupUnauthorizedUser(teamService.Object);

            var teamRequest = new TeamCreateRequestModel
            {
                Title = "Team"
            };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => sut.Post(teamRequest)
                );

        }
        

        [Fact]
        public async Task ListTeams_ReturnsTeams()
        {
            var teamService = new Mock<ITeamService>();
            var sut = Setup(teamService.Object);

            var result = await sut.GetAll();

            teamService.Verify(service => service.GetAllTeamsAsync(), Times.Once);
            Assert.IsType<ActionResult<IEnumerable<TeamResponseModel>>>(result);
        }

        [Fact]
        public async Task EditTeam_UnauthorizedUser_ThrowsException()
        {

            var teamService = new Mock<ITeamService>();
            var sut = SetupUnauthorizedUser(teamService.Object);


            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => sut.Put(It.IsAny<TeamEditRequestModel>(), It.IsAny<int>())
                ) ;
        }

        [Fact]
        public async Task AddMemeber_UnauthorizedUser_ThrowsException()
        {

            var teamService = new Mock<ITeamService>();
            var sut = SetupUnauthorizedUser(teamService.Object);


            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => sut.Put(It.IsAny<int>(), It.IsAny<int>())
                );
        }


        [Fact]
        public async Task RemoveMemeber_UnauthorizedUser_ThrowsException()
        {

            var teamService = new Mock<ITeamService>();
            var sut = SetupUnauthorizedUser(teamService.Object);


            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => sut.Remove(It.IsAny<int>(), It.IsAny<int>())
                );
        }
        [Fact]
        public async Task GetTeamById_ReturnsTeamResponse()
        {
            var teamService = new Mock<ITeamService>();
            var sut = Setup(teamService.Object);

            var result = await sut.GetTeamById(It.IsAny<int>());

            teamService.Verify(service => service.GetTeamAsync(It.IsAny<int>()), Times.Once);
            Assert.IsType<ActionResult<TeamResponseModel>>(result);
        }

        

        [Fact]
        public async Task EditTeam_ReturnsTeamResponse()
        {
            var teamService = new Mock<ITeamService>();
            var sut = Setup(teamService.Object);

            var result = await sut.Put(It.IsAny<TeamEditRequestModel>(), It.IsAny<int>());

            teamService.Verify(service => service.EditAsync(It.IsAny<TeamEditRequestModel>(),
                                                            It.IsAny<int>(),
                                                            CURRENT_USER_ID),
                                                  Times.Once);
            Assert.IsType<ActionResult<TeamResponseModel>>(result);
        }

        [Fact]
        public async Task DeleteTeam_Succeds()
        {
            var teamService = new Mock<ITeamService>();
            var sut = Setup(teamService.Object);
            var idToDelete = 5;
            var result = await sut.Delete(idToDelete);

            Assert.IsType<OkObjectResult>(result);
            teamService.Verify(x => x.DeleteAsync(idToDelete), Times.Once());

        }

        [Fact]
        public async Task AddMemberToTeam_Succeeds()
        {
            var teamService = new Mock<ITeamService>();
            var sut = Setup(teamService.Object);
            var teamId = 5;
            var userId = 3;

            var teamResponse = new TeamMemberResponseModel { Team = "test", Member = "test" };
            teamService.Setup(t => t.AddMember(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(teamResponse);
            var result = await sut.Put(teamId, userId);

            Assert.IsType<OkObjectResult>(result);

            teamService.Verify(x => x.AddMember(teamId, userId, It.IsAny<int>()), Times.Once());
        }

        [Fact]
        public async Task RemoveMember_CallServiceSucceeds()
        {
            var teamService = new Mock<ITeamService>();
            var sut = Setup(teamService.Object);


            var teamId = 5;
            var userId = 3;
            var teamResponse = new TeamMemberResponseModel { Team = "test", Member = "test" };
            teamService.Setup(t => t.RemoveMember(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(teamResponse);
            var result = await sut.Remove(teamId, userId);

            Assert.IsType<OkObjectResult>(result);
            teamService.Verify(x => x.RemoveMember(teamId, userId, It.IsAny<int>()), Times.Once());
        }

        [Fact]
        public async Task Promote_CallsServiceOnce()
        {
            var teamService = new Mock<ITeamService>();
            var sut = Setup(teamService.Object);


            var teamId = 5;
            var userId = 3;
            var teamResponse = new TeamMemberResponseModel { Team = "test", Member = "test" };
            teamService.Setup(t => t.PromoteToTeamLeaderAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(teamResponse);
            var result = await sut.Promote(teamId, userId);

            Assert.IsType<OkObjectResult>(result);
            teamService.Verify(x => x.PromoteToTeamLeaderAsync(teamId, userId, CURRENT_USER_ID), Times.Once());
        }

        private TeamsController Setup(ITeamService teamService)
        {
            var userService = new Mock<IUserService>();
            var mapper = MapperMock.Instance;

            userService.Setup(u => u
                                    .GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                                    .ReturnsAsync(new User() { Id = CURRENT_USER_ID });

            var sut = new TeamsController(userService.Object, teamService, mapper);

            var user = new ClaimsPrincipal();
            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };

            sut.ControllerContext = context;
            return sut;
        }

        private TeamsController SetupUnauthorizedUser(ITeamService teamService)
        {
            var userService = new Mock<IUserService>();
            var mapper = MapperMock.Instance;

            userService.Setup(u => u
                                    .GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                                    .ReturnsAsync((User)null);

            var sut = new TeamsController(userService.Object, teamService, mapper);
            
            return sut;
        }

    }
}
