using Microsoft.AspNetCore.Identity;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WorkforceManager.Data;
using WorkforceManager.Data.Entities;
using WorkforceManager.Models.Requests.UserRequestModels;
using WorkforceManager.Models.Responses.UserResponseModels;
using WorkforceManager.Services;
using WorkforceManager.Services.Interfaces;
using WorkforceManager.UnitTests.Mocks;
using Xunit;

namespace WorkforceManager.UnitTests.Services
{
    public class UserServiceTests
    {
        [Fact]
        public async Task GetAllUsers_ValidData_ShouldSucceed()
        {
            //Arrange
            var data = await GetDatabaseMockAsync();
            var mapper = MapperMock.Instance;
            var userManager = new Mock<IUserManager>();
            userManager.Setup(x => x.GetAllAsync().Result).Returns(data.Users.ToList());
            var userService = new UserService(userManager.Object, data, mapper);

            //Act
            var result = await userService.GetAllUsersAsync();

            //Assert
            Assert.NotEmpty(result);
            Assert.IsAssignableFrom<ICollection<UserResponseModel>>(result);
            Assert.Equal(4, result.ToList().Count);
        }

        [Fact]
        public async Task GetCurrentUser_ValidData_ShouldSucceed()
        {
            //Arrange
            var current = new User
            {
                UserName = "current"
            };

            var data = await GetDatabaseMockAsync();
            var mapper = MapperMock.Instance;
            var userManager = new Mock<IUserManager>();
            userManager.Setup(getUser => getUser.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(current);
            var userService = new UserService(userManager.Object, data, mapper);

            //Act
            var result = await userService.GetCurrentUserAsync(new ClaimsPrincipal());

            //Assert
            Assert.Equal("current", result.UserName);
            userManager.Verify(mock => mock.GetUserAsync((It.IsAny<ClaimsPrincipal>())), Times.Once);
        }

        [Fact]
        public async Task IsUserAdmin_UserIsAdmin_ShouldSucceed()
        {
            //Arrange
            var data = await GetDatabaseMockAsync();
            var user = data.Users.FirstOrDefault(u => u.UserName == "admin");
            var mapper = MapperMock.Instance;
            var userManager = new Mock<IUserManager>();
            userManager.Setup(getUser => getUser.GetUserRolesAsync(user)).ReturnsAsync(new List<string> { "admin" });
            var userService = new UserService(userManager.Object, data, mapper);

            //Act
            var result = await userService.IsUserAdminAsync(user);

            //Assert
            Assert.True(result);
            userManager.Verify(mock => mock.GetUserRolesAsync((It.IsAny<User>())), Times.Once);
        }

        [Fact]
        public async Task IsUserAdmin_UserNotAdmin_ShouldSucceed()
        {
            //Arrange
            var data = await GetDatabaseMockAsync();
            var user = data.Users.FirstOrDefault(u => u.UserName == "team1Leader");
            var mapper = MapperMock.Instance;
            var userManager = new Mock<IUserManager>();
            userManager.Setup(getUser => getUser.GetUserRolesAsync(user)).ReturnsAsync(new List<string> { "Regular" });
            var userService = new UserService(userManager.Object, data, mapper);

            //Act
            var result = await userService.IsUserAdminAsync(user);

            //Assert
            Assert.False(result);
            userManager.Verify(mock => mock.GetUserRolesAsync((It.IsAny<User>())), Times.Once);
        }

        [Fact]
        public async Task IsUserTeamLead_UserNotTeamLead_ShouldSucceed()
        {
            //Arrange
            var data = await GetDatabaseMockAsync();
            var mapper = MapperMock.Instance;
            var userManager = new Mock<IUserManager>();
            userManager.Setup(um => um.FindByIdAsync("2")).ReturnsAsync(data.Users.FirstOrDefault(u => u.Id ==2));
            var userService = new UserService(userManager.Object, data, mapper);

            //Act
            var result = await userService.IsTheUserTeamLeadAsync(2);

            //Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsUserTeamLead_UserIsTeamLead_ShouldSucceed()
        {
            //Arrange
            var data = await GetDatabaseMockAsync();
            var mapper = MapperMock.Instance;
            var userManager = new Mock<IUserManager>();
            userManager.Setup(um => um.FindByIdAsync("1")).ReturnsAsync(data.Users.FirstOrDefault(u => u.Id == 1));
            var userService = new UserService(userManager.Object, data, mapper);

            //Act
            var result = await userService.IsTheUserTeamLeadAsync(1);

            //Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CreateUser_ValidData_ShouldSucceed()
        {
            //Arrange
            var data = await GetDatabaseMockAsync();
            var mapper = MapperMock.Instance;
            var userManager = new Mock<IUserManager>();
            userManager.Setup(x => x.FindByNameAsync("admin")).ReturnsAsync(data.Users.FirstOrDefault(u => u.UserName == "admin"));
            userManager.Setup(x => x.FindByEmailAsync("newuser@test.com")).ReturnsAsync(data.Users.FirstOrDefault(u => u.UserName == "admin"));
            var userService = new UserService(userManager.Object, data, mapper);

            var newUser = new UserCreateRequestModel
            {
                UserName = "NewUser",
                FirstName = "NewUser",
                LastName = "NewUser",
                Password = "NewUser",
                Role = "regular",
                Email = "newuser2@test.com"
            };

            //Act
            var result = await userService.CreateAsync(newUser, 4);

            //Assert
            Assert.IsType<CreatedUserResponseModel>(result);
            Assert.Equal("NewUser", result.Username);
            userManager.Verify(mock => mock.AddToRoleAsync((It.IsAny<User>()), "regular"), Times.Once);
            userManager.Verify(mock => mock.CreateAsync((It.IsAny<User>()), "NewUser"), Times.Once);
        }

        [Fact]
        public async Task CreateUserAndAssignToTeam_ValidData_ShouldSucceed()
        {
            //Arrange
            var data = await GetDatabaseMockAsync();
            var mapper = MapperMock.Instance;
            var userManager = new Mock<IUserManager>();
            userManager.Setup(x => x.FindByNameAsync("admin")).ReturnsAsync(data.Users.FirstOrDefault(u => u.UserName == "admin"));
            userManager.Setup(x => x.FindByEmailAsync("newuser@test.com")).ReturnsAsync(data.Users.FirstOrDefault(u => u.UserName == "admin"));
            var userService = new UserService(userManager.Object, data, mapper);

            var newUser = new UserCreateRequestModel
            {
                UserName = "NewUser",
                FirstName = "NewUser",
                LastName = "NewUser",
                Password = "NewUser",
                Role = "regular",
                Email = "newuser2@test.com",
                TeamId = "1"
            };

            //Act
            var result = await userService.CreateAsync(newUser, 4);

            //Assert
            Assert.IsType<CreatedUserResponseModel>(result);
            Assert.NotEqual("none", result.AddedToTeam);
            userManager.Verify(mock => mock.AddToRoleAsync((It.IsAny<User>()), "regular"), Times.Once);
            userManager.Verify(mock => mock.CreateAsync((It.IsAny<User>()), "NewUser"), Times.Once);
        }

        [Fact]
        public async Task CreateUser_ExistingUserName_ThrowsException()
        {
            //Arrange
            var data = await GetDatabaseMockAsync();
            var mapper = MapperMock.Instance;
            var userManager = new Mock<IUserManager>();
            userManager.Setup(x => x.FindByNameAsync("admin")).ReturnsAsync(data.Users.FirstOrDefault(u => u.UserName == "admin"));
            userManager.Setup(x => x.FindByEmailAsync("newuser@test.com")).ReturnsAsync(data.Users.FirstOrDefault(u => u.UserName == "admin"));
            var userService = new UserService(userManager.Object, data, mapper);

            var newUser = new UserCreateRequestModel
            {
                UserName = "admin",
                FirstName = "NewUser",
                LastName = "NewUser",
                Password = "NewUser",
                Role = "regular",
                Email = "newuser2@test.com"
            };

            //Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(() => userService.CreateAsync(newUser, 4));
            Assert.Equal("User with username 'admin' already exists!", exception.Message);
        }

        [Fact]
        public async Task CreateUser_ExistingEmail_ThrowsException()
        {
            //Arrange
            var data = await GetDatabaseMockAsync();
            var mapper = MapperMock.Instance;
            var userManager = new Mock<IUserManager>();
            userManager.Setup(x => x.FindByNameAsync("admin")).ReturnsAsync(data.Users.FirstOrDefault(u => u.UserName == "admin"));
            userManager.Setup(x => x.FindByEmailAsync("newuser@test.com")).ReturnsAsync(data.Users.FirstOrDefault(u => u.UserName == "admin"));
            var userService = new UserService(userManager.Object, data, mapper);

            var newUser = new UserCreateRequestModel
            {
                UserName = "NewUser",
                FirstName = "NewUser",
                LastName = "NewUser",
                Password = "NewUser",
                Role = "regular",
                Email = "newuser@test.com"
            };

            //Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(() => userService.CreateAsync(newUser, 4));
            Assert.Equal("Email address 'newuser@test.com' already exists!", exception.Message);
        }

        [Fact]
        public async Task EditUser_ValidData_ShouldSucceed()
        {
            //Arrange
            var data = await GetDatabaseMockAsync();
            var mapper = MapperMock.Instance;
            var userManager = new Mock<IUserManager>();
            userManager.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(data.Users.FirstOrDefault(u => u.Id == 1));
            var userService = new UserService(userManager.Object, data, mapper);

            var newUser = new UserEditRequestModel
            {
                UserName = "NewUserName",
                Password = "NewUserPassword",
                Email = "newuseremail@test.com"
            };

            //Act
            var result = await userService.EditAsync(newUser, 1, 4);

            //Assert
            Assert.IsType<UserResponseModel>(result);
            userManager.Verify(mock => mock.UpdateAsync((It.IsAny<User>())), Times.Once);
        }

        [Fact]
        public async Task EditUser_InvalidUserId_ThrowsException()
        {
            //Arrange
            var data = await GetDatabaseMockAsync();
            var mapper = MapperMock.Instance;
            var userManager = new Mock<IUserManager>();
            var userService = new UserService(userManager.Object, data, mapper);

            var newUser = new UserEditRequestModel
            {
                UserName = "NewUserName",
                Password = "NewUserPassword",
                Email = "newuseremail@test.com"
            };

            //Assert
            KeyNotFoundException exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => userService.EditAsync(newUser, 10, 4));
            Assert.Equal("User with id '10' does not exists!", exception.Message);
        }

        [Fact]
        public async Task EditUser_ExsistingUserName_ThrowsException()
        {
            //Arrange
            var data = await GetDatabaseMockAsync();
            var mapper = MapperMock.Instance;
            var userManager = new Mock<IUserManager>();
            userManager.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(data.Users.FirstOrDefault(u => u.Id == 1));
            userManager.Setup(x => x.FindByNameAsync("admin")).ReturnsAsync(data.Users.FirstOrDefault(u => u.UserName == "admin"));
            var userService = new UserService(userManager.Object, data, mapper);

            var newUser = new UserEditRequestModel
            {
                UserName = "admin",
                Password = "NewUserPassword",
                Email = "newuseremail@test.com"
            };

            //Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(() => userService.EditAsync(newUser, 1, 4));
            Assert.Equal("User with username 'admin' already exists!", exception.Message);
        }

        [Fact]
        public async Task DeleteUser_ValidData_ShouldSucceed()
        {
            //Arrange
            var data = await GetDatabaseMockAsync();
            var mapper = MapperMock.Instance;
            var userManager = new Mock<IUserManager>();
            userManager.Setup(x => x.FindByIdAsync("2")).ReturnsAsync(data.Users.FirstOrDefault(u => u.Id == 2));
            var userService = new UserService(userManager.Object, data, mapper);

            //Act
            var result = await userService.DeleteAsync(2);

            //Assert
            Assert.IsType<DeletedUserResponseModel>(result);
            Assert.Equal(2, result.Id);
            userManager.Verify(mock => mock.DeleteAsync((It.IsAny<User>())), Times.Once);
        }

        [Fact]
        public async Task DeleteUser_InvalidUserId_ThrowsException()
        {
            //Arrange
            var data = await GetDatabaseMockAsync();
            var mapper = MapperMock.Instance;
            var userManager = new Mock<IUserManager>();
            var userService = new UserService(userManager.Object, data, mapper);

            //Assert
            KeyNotFoundException exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => userService.DeleteAsync(10));
        }

        [Fact]
        public async Task GetUserByName_NonExistingUser_ReturnsNull()
        {
            var data = await GetDatabaseMockAsync();
            var mapper = MapperMock.Instance;
            var userManager = new Mock<IUserManager>();
            var userService = new UserService(userManager.Object, data, mapper);

            var result = await userService.GetUserByNameAsync("nonExistingUser");
            Assert.Null(result);
        }

        private async Task<WorkforceManagerDbContext> GetDatabaseMockAsync()
        {
            var context = DatabaseMock.Instance;

            //add Users in the in memory database
            var team1Leader = new User
            {
                UserName = "team1Leader",
                FirstName = "Leader",
                LastName = "Team 1"
            };

            var team1Regular1 = new User
            {
                UserName = "team1Regular1",
                FirstName = "Regular 1",
                LastName = "Team 1"
            };

            var team1Regular2 = new User
            {
                UserName = "team1Regular2",
                FirstName = "Regular 2",
                LastName = "Team 1"
            };

            var admin = new User
            {
                UserName = "admin",
                Email = "newuser@test.com"
            };

            await context.Users.AddRangeAsync(team1Leader, team1Regular1, team1Regular2, admin);
            await context.SaveChangesAsync();

            //add Roles to the created users
            for (int i = 1; i < 4; i++)
            {
                context.UserRoles.Add(new IdentityUserRole<int>()
                {
                    RoleId = 2,
                    UserId = i
                }
                );
            }

            context.UserRoles.Add(new IdentityUserRole<int>()
            {
                RoleId = 1,
                UserId = 4
            }
            );

            await context.SaveChangesAsync();

            var team1 = new Team
            {
                Title = "Team1",
                Description = "This is testTeam 1",
                TeamLeaderId = team1Leader.Id
            };

            team1.Members.Add(team1Leader);
            team1.Members.Add(team1Regular1);
            team1.Members.Add(team1Regular2);

            await context.Teams.AddAsync(team1);
            await context.SaveChangesAsync();

            return context;
        }
    }
}
