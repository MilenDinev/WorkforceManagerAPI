

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using WorkforceManager.Data.Entities;
using WorkforceManager.Models.Requests.RequestRequestModels;
using WorkforceManager.Models.Responses.RequestsResponseModels;
using WorkforceManager.Services.Interfaces;
using WorkforceManager.Web.Controllers;
using Xunit;

namespace WorkforceManager.UnitTests.Controllers
{
    public class RequestsControllerTests
    {
        private const int CURRENT_USER_ID = 2;
        private const int TARGETED_REQUEST_ID = 1;
        
        [Fact]
        public async Task CreateRequest_ValidData_ReturnsCreated()
        {

            var requestService = new Mock<IRequestService>();
            var sut = Setup(requestService.Object);

            var request = new RequestCreateRequestModel();
            

            requestService.Setup(t => t.CreateAsync(request,CURRENT_USER_ID)).ReturnsAsync(new RequestResponseModel());
            
            var result = await sut.Post(request);

            Assert.IsType<CreatedAtActionResult>(result);
            requestService.Verify(service => service.CreateAsync(request, CURRENT_USER_ID), Times.Once);
        }

        [Fact]
        public async Task CreateForUser_ValidData_ReturnsCreated()
        {

            var requestService = new Mock<IRequestService>();
            var sut = Setup(requestService.Object);

            var request = new RequestCreateRequestModel();
            var userId = 5;

            requestService.Setup(t => t.CreateAsync(request, CURRENT_USER_ID, userId)).ReturnsAsync(new RequestResponseModel());

            var result = await sut.CreateForUser(request, userId);

            Assert.IsType<CreatedAtActionResult>(result);
            requestService.Verify(service => service.CreateAsync(request, CURRENT_USER_ID, userId), Times.Once);
        }

        [Fact]
        public void CreateRequest_UnauthorizedUser_ThrowsException()
        {
            var requestService = new Mock<IRequestService>();
            var userService = new Mock<IUserService>();
            var requestController = new RequestsController(userService.Object, requestService.Object);

            var request = new RequestCreateRequestModel();

            //act
            Func<Task> action = async () => { await requestController.Post(request); };

            //assert
            Task<UnauthorizedAccessException> exception = Assert.ThrowsAsync<UnauthorizedAccessException>(action);
            Assert.Equal("Bad credentials, please try again!", exception.Result.Message);
        }


        [Fact]
        public void CreateRequest_InvalidModelState_ReturnsBadRequest()
        {
            var requestService = new Mock<IRequestService>();
            var userService = new Mock<IUserService>();
            var requestController = new RequestsController(userService.Object, requestService.Object);
            requestController.ModelState.AddModelError("error", "error error");

            var request = new RequestCreateRequestModel();

            //act
            var result = requestController.Post(request);

            //assert
            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public async Task DeleteRequest_Succeds()
        {
            var requestService = new Mock<IRequestService>();
            var sut = Setup(requestService.Object);

            requestService.Setup(t => t.DeleteAsync(TARGETED_REQUEST_ID)).ReturnsAsync(new RequestResponseModel());

            var result = await sut.Delete(TARGETED_REQUEST_ID);

            Assert.IsType<OkObjectResult>(result);
            requestService.Verify(service => service.DeleteAsync(TARGETED_REQUEST_ID), Times.Once);
        }

        [Fact]
        public async Task EditRequest_Succeeds()
        {
            var requestService = new Mock<IRequestService>();
            var sut = Setup(requestService.Object);

            var request = new RequestCreateRequestModel();

            requestService.Setup(t => t.EditAsync(request, TARGETED_REQUEST_ID,CURRENT_USER_ID)).ReturnsAsync(new RequestResponseModel());

            var result = await sut.Put(request, TARGETED_REQUEST_ID);

            Assert.IsType<ActionResult<RequestResponseModel>>(result);
            requestService.Verify(service => service.EditAsync(request, TARGETED_REQUEST_ID, CURRENT_USER_ID), Times.Once);
        }

        [Fact]
        public void EditRequest_UnauthorizedUser_ThrowsException()
        {
            var requestService = new Mock<IRequestService>();
            var userService = new Mock<IUserService>();
            var requestController = new RequestsController(userService.Object, requestService.Object);

            var request = new RequestCreateRequestModel();

            //act
            Func<Task> action = async () => { await requestController.Put(request, TARGETED_REQUEST_ID); };

            //assert
            Task<UnauthorizedAccessException> exception = Assert.ThrowsAsync<UnauthorizedAccessException>(action);
            Assert.Equal("Bad credentials, please try again!", exception.Result.Message);
        }

        [Fact]
        public async Task Submit_Succeeds()
        {
            var requestService = new Mock<IRequestService>();
            var sut = Setup(requestService.Object);

            requestService.Setup(t => t.SubmitAsync(TARGETED_REQUEST_ID)).ReturnsAsync(new SubmitRequestResponseModel());
            var result = await sut.Submit(TARGETED_REQUEST_ID);

            Assert.IsType<ActionResult<RequestResponseModel>>(result);
            requestService.Verify(service => service.SubmitAsync(TARGETED_REQUEST_ID), Times.Once);
        }

        [Fact]
        public async Task Approve_Succeeds()
        {
            var requestService = new Mock<IRequestService>();
            var sut = Setup(requestService.Object);

            requestService.Setup(t => t.ApproveAsync(TARGETED_REQUEST_ID, CURRENT_USER_ID)).ReturnsAsync(new ApprovedRequestResponseModel());
            
            var result = await sut.Approve(TARGETED_REQUEST_ID);

            Assert.IsType<ActionResult<ApprovedRequestResponseModel>>(result);
            requestService.Verify(service => service.ApproveAsync(TARGETED_REQUEST_ID, CURRENT_USER_ID), Times.Once);
        }

        [Fact]
        public async Task Reject_Succeeds()
        {
            var requestService = new Mock<IRequestService>();
            var sut = Setup(requestService.Object);

            requestService.Setup(t => t.RejectAsync(TARGETED_REQUEST_ID, CURRENT_USER_ID)).ReturnsAsync(new RejectedRequestResponseModel());
            
            var result = await sut.Reject(TARGETED_REQUEST_ID);

            Assert.IsType<ActionResult<RejectedRequestResponseModel>>(result);
            requestService.Verify(service => service.RejectAsync(TARGETED_REQUEST_ID, CURRENT_USER_ID), Times.Once);
        }

        [Fact]
        public async Task GetRequestById_ValidModelState_ShouldSucceed()
        {
            var requestService = new Mock<IRequestService>();
            var sut = Setup(requestService.Object);

            requestService.Setup(t => t.GetRequestByIdDetailedAsync(TARGETED_REQUEST_ID)).ReturnsAsync(new RequestDetailedResponseModel());

            var result = await sut.GetRequest(TARGETED_REQUEST_ID);

            Assert.IsType<ActionResult<RequestDetailedResponseModel>>(result);
            requestService.Verify(service => service.GetRequestByIdDetailedAsync(TARGETED_REQUEST_ID), Times.Once);
        }

        [Fact]
        public async Task GetRequestsByStatusApproved_ValidModelState_ShouldSucceed()
        {
            var requestService = new Mock<IRequestService>();
            var sut = Setup(requestService.Object);

            var approvedRequests = new List<RequestResponseModel>();
            approvedRequests.Add(new RequestResponseModel { Id = 1 });
            approvedRequests.Add(new RequestResponseModel { Id = 2 });

            requestService.Setup(t => t.GetRequestsByStatusAsync("Approved")).ReturnsAsync(approvedRequests);

            var result = await sut.GetByStatus("Approved");

            Assert.IsType<ActionResult<IEnumerable<RequestResponseModel>>>(result);
            requestService.Verify(service => service.GetRequestsByStatusAsync("Approved"), Times.Once);
        }

        [Fact]
        public async Task GetRequestsMadeByUser_ValidModelState_ShouldSucceed()
        {
            var requestService = new Mock<IRequestService>();
            var sut = Setup(requestService.Object);

            var approvedRequests = new List<RequestResponseModel>();
            approvedRequests.Add(new RequestResponseModel { Id = 1 });
            approvedRequests.Add(new RequestResponseModel { Id = 2 });

            requestService.Setup(t => t.GetRequestsMadeByUserAsync(1)).ReturnsAsync(approvedRequests);

            var result = await sut.GetByUser(1);

            Assert.IsType<ActionResult<IEnumerable<RequestResponseModel>>>(result);
            requestService.Verify(service => service.GetRequestsMadeByUserAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetMyRequests_ValidModelState_ShouldSucceed()
        {
            var requestService = new Mock<IRequestService>();
            var sut = Setup(requestService.Object);

            var approvedRequests = new List<RequestResponseModel>();
            approvedRequests.Add(new RequestResponseModel { Id = 1 });
            approvedRequests.Add(new RequestResponseModel { Id = 2 });

            requestService.Setup(t => t.GetRequestsMadeByUserAsync(CURRENT_USER_ID)).ReturnsAsync(approvedRequests);

            var result = await sut.Get();

            Assert.IsType<ActionResult<IEnumerable<RequestResponseModel>>>(result);
            requestService.Verify(service => service.GetRequestsMadeByUserAsync(CURRENT_USER_ID), Times.Once);
        }

        [Fact]
        public async Task GetRequestsThatNeedToBeProcessed_ValidModelState_ShouldSucceed()
        {
            var requestService = new Mock<IRequestService>();
            var sut = Setup(requestService.Object);

            var approvedRequests = new List<RequestResponseModel>();
            approvedRequests.Add(new RequestResponseModel { Id = 1 });
            approvedRequests.Add(new RequestResponseModel { Id = 2 });

            requestService.Setup(t => t.GetRequestsToBeProcessedByApproverAsync(It.IsAny<User>())).ReturnsAsync(approvedRequests);

            var result = await sut.GetRequestsToBeProcessed();

            Assert.IsType<ActionResult<IEnumerable<RequestResponseModel>>>(result);
            requestService.Verify(service => service.GetRequestsToBeProcessedByApproverAsync(It.IsAny<User>()), Times.Once);
        }

        private RequestsController Setup(IRequestService requestService)
        {
            var userService = new Mock<IUserService>();

            var sut = new RequestsController(userService.Object, requestService);
            var request = new RequestCreateRequestModel();
            var currentUser = new User { Id = CURRENT_USER_ID };
            var user = new ClaimsPrincipal();
            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };
            sut.ControllerContext = context;

            userService.Setup(u => u.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(currentUser);

            return sut;
        }
    }
}
