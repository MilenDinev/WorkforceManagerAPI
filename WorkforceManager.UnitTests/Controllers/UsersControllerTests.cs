using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WorkforceManager.Data.Entities;
using WorkforceManager.Models.Requests.UserRequestModels;
using WorkforceManager.Models.Responses.UserResponseModels;
using WorkforceManager.Services.Interfaces;
using WorkforceManager.Web.Controllers;
using Xunit;

namespace WorkforceManager.UnitTests.Controllers
{
    public class UsersControllerTests
    {
        [Fact]
        public void Post_UnauthorizedUser_ThrowsUnauthorizedAccessException()
        {
            // arrange
            var userService = new Mock<IUserService>();
            var usersController = new UsersController(userService.Object);

            var user = new UserCreateRequestModel
            {
                UserName = "fake-user-name",
                Password = "fake-password",
                FirstName = "fake-name",
                LastName = "fake-last-name",
                Email = "fake-email-address@gmail.com"
            };

            //act
            Func<Task> action = async () => { await usersController.Post(user); };

            //assert
            Task<UnauthorizedAccessException> exception = Assert.ThrowsAsync<UnauthorizedAccessException>(action);
            Assert.Equal("Bad credentials, please try again!", exception.Result.Message);
        }

        [Fact]
        public void Post_ValidModelState_ShouldCallUserService()
        {
            // arrange
            var adminUser = new User
            {
                Id = 1,
                UserName = "admin",
                Email = "admin@test.com"
            };

            var newUser = new User
            {
                Id = 2,
                UserName = "fake-user-name",
                Email = "fake-email-address@gmail.com"
            };

            var userService = new Mock<IUserService>();
            userService.Setup(us => us.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult(adminUser));
            var usersController = new UsersController(userService.Object);
            
            var user = new UserCreateRequestModel
            {
                UserName = "fake-user-name",
                Password = "fake-password",
                FirstName = "fake-name",
                LastName = "fake-last-name",
                Email = "fake-email-address@gmail.com",
                Role = "regular"
            };

            userService.Setup(us => us.CreateAsync(user, 1)).Returns(Task.FromResult(new CreatedUserResponseModel()));

            //act
            var result = usersController.Post(user);

            //assert
            userService.Verify(service => service.CreateAsync(user, 1), Times.Once);
            Assert.IsType<CreatedAtActionResult>(result.Result);
        }

        [Fact]
        public void Post_InvalidModelState_ShouldReturnBadRequest()
        {
            // arrange
            var userService = new Mock<IUserService>();
            var usersController = new UsersController(userService.Object);
            usersController.ModelState.AddModelError("error", "error error");

            var user = new UserCreateRequestModel
            {
                UserName = "fake-user-name",
                Password = "fake-password",
                FirstName = "fake-name",
                LastName = "fake-last-name",
                Email = "fake-email-address@gmail.com",
                Role = "regular"
            };

            //act
            var result = usersController.Post(user);

            //assert
            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public void Put_UnauthorizedUser_ThrowsUnauthorizedAccessException()
        {
            // arrange
            var userService = new Mock<IUserService>();
            var usersController = new UsersController(userService.Object);

            var user = new UserEditRequestModel
            {
                UserName = "fake-user-name",
                Password = "fake-password",
                Email = "fake-email-address@gmail.com"
            };

            //act
            Func<Task> action = async () => { await usersController.Put(user, 1); };

            //assert
            Task<UnauthorizedAccessException> exception = Assert.ThrowsAsync<UnauthorizedAccessException>(action);
            Assert.Equal("Bad credentials, please try again!", exception.Result.Message);
        }

        [Fact]
        public void Put_ValidModelState_ShouldCallUserService()
        {
            // arrange
            var adminUser = new User
            {
                Id = 1,
                UserName = "admin",
                Email = "admin@test.com"
            };

            var userService = new Mock<IUserService>();
            userService.Setup(us => us.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult(adminUser));
            var usersController = new UsersController(userService.Object);
            
            var user = new UserEditRequestModel
            {
                UserName = "fake-user-name",
                Password = "fake-password",
                Email = "fake-email-address@gmail.com"
            };

            //act
            var result = usersController.Put(user, 1);

            //assert
            userService.Verify(service => service.EditAsync(user, 1, 1), Times.Once);
            Assert.IsType<ActionResult<UserResponseModel>>(result.Result);
        }

        [Fact]
        public void Delete_ValidModelState_ShouldCallUserService()
        {
            // arrange
            var userService = new Mock<IUserService>();
            var usersController = new UsersController(userService.Object);

            //act
            var result = usersController.Delete(2);

            //assert
            userService.Verify(service => service.DeleteAsync(2), Times.Once);
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public void Get_ValidModelState_ShouldCallUserService()
        {
            // arrange
            var userService = new Mock<IUserService>();
            var usersController = new UsersController(userService.Object);

            //act
            var result = usersController.Get();

            //assert
            userService.Verify(service => service.GetAllUsersAsync(), Times.Once);
        }
    }
}
