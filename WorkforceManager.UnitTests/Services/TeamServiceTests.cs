using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using WorkforceManager.Data.Entities;
using WorkforceManager.Services;
using WorkforceManager.UnitTests.Mocks;
using WorkforceManager.Services.Interfaces;
using WorkforceManager.Data;
using Microsoft.AspNetCore.Identity;
using WorkforceManager.Models.Responses.TeamResponseModels;
using WorkforceManager.Models.Requests.TeamRequestModels;
using Microsoft.Extensions.Options;
using WorkforceManager.Models;
using Moq;
using Microsoft.Extensions.Configuration;

namespace WorkforceManager.UnitTests.Services
{
    public class TeamServiceTests
    {

        [Fact]
        public async Task GetAllTeams_ValidData_ShouldSucceed()
        {
            //Arrange
            var teamService = await GetTeamService();

            //Act
            var result = await teamService.GetAllTeamsAsync();

            //Assert
            Assert.NotEmpty(result);
            Assert.IsAssignableFrom<ICollection<TeamResponseModel>>(result);
            Assert.Equal(2, result.ToList().Count);
        }

        [Fact]
        public async Task GetTeamById_ValidData_ShouldSucceed()
        {
            //Arrange
            var teamService = await GetTeamService();

            //Act
            var result = await teamService.GetTeamAsync(1);

            //Assert
            Assert.IsType<Team>(result);
            Assert.Equal("Team1", result.Title);
        }

        [Fact]
        public async Task GetTeamById_InvalidData_ThrowsException()
        {
            //Arrange
            var teamService = await GetTeamService();

            //Act Assert
            KeyNotFoundException exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => teamService.GetTeamAsync(10));
            Assert.Equal("Team with id '10' does not exists!", exception.Message);
        }

        [Fact]
        public async Task CreateTeam_ValidData_ShouldSucceed()
        {
            //Arrange
            var teamToBeCreated = new TeamCreateRequestModel
            {
                Title = "NewTeam",
                TeamLeaderId = "4"
            };
            var teamService = await GetTeamService();

            //Act
            var result = await teamService.CreateAsync(teamToBeCreated, 1);

            //Assert
            Assert.IsType<CreatedTeamResponseModel>(result);
            Assert.Equal("NewTeam", result.Title);
        }

        [Fact]
        public async Task CreateTeam_ExistingTitle_ThrowsException()
        {
            //Arrange
            var teamToBeCreated = new TeamCreateRequestModel
            {
                Title = "Team1",
                TeamLeaderId = "1"
            };
            var teamService = await GetTeamService();

            //Act Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(() => teamService.CreateAsync(teamToBeCreated, 1));
            Assert.Equal("Team with title 'Team1' already exists!", exception.Message);
        }

        [Fact]
        public async Task CreateTeam_NonExistingIdForTeamLeader_ThrowsException()
        {
            //Arrange
            var teamToBeCreated = new TeamCreateRequestModel
            {
                Title = "New Team",
                TeamLeaderId = "10"
            };
            var teamService = await GetTeamService();

            //Act Assert
            KeyNotFoundException exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => teamService.CreateAsync(teamToBeCreated, 1));
            Assert.Equal("User with ID '10' does not exist!", exception.Message);
        }

        [Fact]
        public async Task EditTeam_ValidData_ShouldSucceed()
        {
            //Arrange
            var teamNewValuesModel = new TeamEditRequestModel
            {
                Title = "NewTeamEdited",
                TeamLeaderId = "1"
            };
            var teamService = await GetTeamService();

            //Act
            var result = await teamService.EditAsync(teamNewValuesModel, 1, 1);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<TeamResponseModel>(result);
            Assert.Equal(teamNewValuesModel.Title, result.Title);
        }

        [Fact]
        public async Task EditTeam_TeamLeadNotATeamMember_ThrowsException()
        {
            //Arrange
            var teamNewValuesModel = new TeamEditRequestModel
            {
                Title = "NewTeamEdited",
                TeamLeaderId = "2"
            };
            var teamService = await GetTeamService();

            //Act Assert
            KeyNotFoundException exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => teamService.EditAsync(teamNewValuesModel, 1, 1));
            Assert.Equal("User with id '2' is not part of 'Team1' !", exception.Message);
        }

        [Fact]
        public async Task EditTeam_ExistingTitle_ThrowsException()
        {
            //Arrange
            var teamNewValuesModel = new TeamEditRequestModel
            {
                Title = "Team2",
                TeamLeaderId = "1"
            };
            var teamService = await GetTeamService();

            //Act Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(() => teamService.EditAsync(teamNewValuesModel, 1, 1));
            Assert.Equal($"Team with title 'Team2' already exists!", exception.Message);
        }

        [Fact]
        public async Task DeleteTeam_ValidData_ShouldSucceed()
        {
            //Arrange
            var teamService = await GetTeamService();

            //Act
            var result = await teamService.DeleteAsync(2);
            var getTeamsAfterDelete = await teamService.GetAllTeamsAsync();

            //Assert
            Assert.IsType<TeamResponseModel>(result);
            Assert.Single(getTeamsAfterDelete.ToList());
        }

        [Fact]
        public async Task AddMember_ValidData_ShouldSucceed()
        {
            //Arrange
            var teamService = await GetTeamService();

            //Act
            var result = await teamService.AddMember(1, 2, 1);
            var team1 = await teamService.GetTeamAsync(1);

            //Assert
            Assert.IsType<TeamMemberResponseModel>(result);
            Assert.Equal(4, team1.Members.Count);
        }

        [Fact]
        public async Task AddMember_AlreadyTeamMember_ThrowsException()
        {
            //Arrange
            var teamService = await GetTeamService();

            //Act Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(() => teamService.AddMember(1, 1, 1));
            Assert.Equal("User 'team1Leader' is already part of team 'Team1' !", exception.Message);
        }

        [Fact]
        public async Task AddMember_InvalidUserId_ThrowsException()
        {
            //Arrange
            var teamService = await GetTeamService();

            //Act Assert
            KeyNotFoundException exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => teamService.AddMember(1, 10, 1));
            Assert.Equal("User with ID '10' does not exist!", exception.Message);
        }

        [Fact]
        public async Task RemoveMember_ValidData_ShouldSucceed()
        {
            //Arrange
            var teamService = await GetTeamService();

            //Act
            var result = await teamService.RemoveMember(1, 3, 1);
            var team1 = await teamService.GetTeamAsync(1);

            //Assert
            Assert.IsType<TeamMemberResponseModel>(result);
            Assert.Equal(2, team1.Members.Count);
        }

        [Fact]
        public async Task RemoveMember_UserNotMember_ThrowsException()
        {
            //Arrange
            var teamService = await GetTeamService();

            //Act Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(() => teamService.RemoveMember(1, 2, 1));
        }

        [Fact]
        public async Task RemoveMember_UserTeamLeader_ThrowsException()
        {
            //Arrange
            var teamService = await GetTeamService();
            var team1 = await teamService.GetTeamAsync(1);
            //Act
            await teamService.RemoveMember(1, 1, 1);
            //Assert
            Assert.Null(team1.TeamLeader);
        }

        [Fact]
        public async Task RemoveMember_NotexistingUser_ThrowsException()
        {
            //Arrange
            var teamService = await GetTeamService();

            //Act Assert
            KeyNotFoundException exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => teamService.RemoveMember(1, 10, 1));
            Assert.Equal("User with ID '10' does not exist!", exception.Message);
        }

        [Fact]
        public async Task PromoteToTeamLeader_NonExistingUser_ThrowsException()
        {
            var teamService = await GetTeamService();

            await Assert.ThrowsAsync<KeyNotFoundException>(() => teamService.PromoteToTeamLeaderAsync(1, 10, 1));
        }

        [Fact]
        public async Task PromoteToTeamLeader_AlreadyTeamLeader_ThrowsException()
        {
            var teamService = await GetTeamService();

            await Assert.ThrowsAsync<ArgumentException>(() => teamService.PromoteToTeamLeaderAsync(1, 1, 1));
        }

        [Fact]
        public async Task PromoteToTeamLeader_NotTeamMember_ThrowsException()
        {
            var teamService = await GetTeamService();

            await Assert.ThrowsAsync<ArgumentException>(() => teamService.PromoteToTeamLeaderAsync(1, 2, 1));
        }

        [Fact]
        public async Task PromoteToTeamLeader_TeamMemberNotLeader_ChangesLeader()
        {
            var teamService = await GetTeamService();

            var result = await teamService.PromoteToTeamLeaderAsync(1, 3, 1);

            Assert.Equal("Team1",result.Team);
            Assert.Equal("team1Regular1", result.Member);
        } 

        private async Task<ITeamService> GetTeamService()
        {
            SmtpSettings settings;
            Mock<IOptions<SmtpSettings>> mockOptions;
            var config = ConfigInitializer.InitConfig();
            settings = config.GetSection(nameof(SmtpSettings)).Get<SmtpSettings>();
            mockOptions = new Mock<IOptions<SmtpSettings>>();
            mockOptions.Setup(m => m.Value).Returns(settings);

            var emailNotificationService = new EmailNotificationService(mockOptions.Object);
            var data = await GetDatabaseMockAsync();
            var mapper = MapperMock.Instance;
            return new TeamService(data, mapper, emailNotificationService);
        }

        private async Task<WorkforceManagerDbContext> GetDatabaseMockAsync()
        {
            var context = DatabaseMock.Instance;

            //add Users in the in memory database
            var team1Leader = new User
            {
                UserName = "team1Leader",
                FirstName = "Leader",
                LastName = "Team 1",
                Email = "test1teamlead@test.com"
            };

            var team2Leader = new User
            {
                UserName = "team2Leader",
                FirstName = "Leader",
                LastName = "Team 2",
                Email = "test2teamlead@test.com"
            };

            var team1Regular1 = new User
            {
                UserName = "team1Regular1",
                FirstName = "Regular 1",
                LastName = "Team 1",
                Email = "test1regular1@test.com"
            };

            var team1Regular2 = new User
            {
                UserName = "team1Regular2",
                FirstName = "Regular 2",
                LastName = "Team 1",
                Email = "test1regular2@test.com"
            };

            var team2Regular1 = new User
            {
                UserName = "team2Regular1",
                FirstName = "Regular 1",
                LastName = "Team 2",
                Email = "test2regular1@test.com"
                
            };

            await context.Users.AddRangeAsync(team1Leader, team2Leader, team1Regular1, team1Regular2, team2Regular1);
            await context.SaveChangesAsync();

            //add Roles to the created users
            for (int i = 1; i < 6; i++)
            {
                context.UserRoles.Add(new IdentityUserRole<int>()
                {
                    RoleId = 2,
                    UserId = i
                }
                );
            }
            await context.SaveChangesAsync();

            //add Teams in the in memory database
            var team1 = new Team
            {
                Title = "Team1",
                Description = "This is testTeam 1",
                TeamLeaderId = team1Leader.Id
            };

            team1.Members.Add(team1Leader);
            team1.Members.Add(team1Regular1);
            team1.Members.Add(team1Regular2);

            var team2 = new Team
            {
                Title = "Team2",
                Description = "This is testTeam 2",
                TeamLeaderId = team2Leader.Id
            };

            team2.Members.Add(team2Leader);
            team2.Members.Add(team2Regular1);

            await context.Teams.AddRangeAsync(team1, team2);

            await context.SaveChangesAsync();

            return context;
        }
    }
}
