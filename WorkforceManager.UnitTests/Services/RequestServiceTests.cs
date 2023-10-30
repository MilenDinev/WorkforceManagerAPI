
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkforceManager.Data;
using WorkforceManager.Data.Entities;
using WorkforceManager.Models.Requests.RequestRequestModels;
using WorkforceManager.Models.Responses.RequestsResponseModels;
using WorkforceManager.Models.Responses.UserResponseModels;
using WorkforceManager.Services;
using WorkforceManager.Services.Exceptions;
using WorkforceManager.Services.Interfaces;
using WorkforceManager.UnitTests.Mocks;
using Xunit;

namespace WorkforceManager.UnitTests.Services
{
    public class RequestServiceTests
    {
        private const int REQUESTER_ID = 1;
        private const string REQUESTER_USERNAME = "requester";
        
        private const int APPROVER1_ID = 2;
        private const string APPROVER1_USERNAME = "approver1";
        private const string APPROVER1_EMAIL = "approver1@test.com";

        private const int APPROVER2_ID = 3;
        private const string APPROVER2_USERNAME = "approver2";
        private const string APPROVER2_EMAIL = "approver2@test.com";

        #region Create
        [Fact]
        public async Task CreateRequest_WithNoApprovers_Succeeds()
        {
            var context = await GetSeededDatabase();
            var sut = await GetBasicSutSetup(context);

            var requestModel = new RequestCreateRequestModel()
            {
                StartDate = "07/12/2022",
                EndDate = "07/12/2022",
                Description = "Test description",
                Type = "Paid"
            };
            var requester = await context.Users.FirstOrDefaultAsync(u => u.Id == 1);
            var result = await sut.CreateAsync(requestModel, requester.Id);

            var createdRequest = context.TimeOffRequests.FirstOrDefault();
            Assert.Equal(1, createdRequest.CreatorId);
            Assert.Equal(1, requester.Requests.Count);
            Assert.Equal(1, context.TimeOffRequests.Count());
            Assert.IsType<RequestResponseModel>(result);
        }

        [Fact]
        public async Task CreateRequest_ForAnotherUser_Succeeds()
        {
            var context = await GetSeededDatabase();
            var sut = await GetSutSetupWithApprovers(context);

            var requestModel = new RequestCreateRequestModel()
            {
                StartDate = "07/12/2022",
                EndDate = "07/12/2022",
                Description = "Test description",
                Type = "Paid"
            };
            var requester = await context.Users.FirstOrDefaultAsync(u => u.Id == REQUESTER_ID);
            var result = await sut.CreateAsync(requestModel,APPROVER1_ID, REQUESTER_ID);

            var createdRequest = context.TimeOffRequests.FirstOrDefault();
            Assert.Equal(APPROVER1_ID, createdRequest.CreatorId);
            Assert.Equal(1, requester.Requests.Count);
            Assert.Equal(1, context.TimeOffRequests.Count());
            Assert.IsType<RequestResponseModel>(result);
        }

        [Fact]
        public async Task CreateRequest_OverlappingRequest_ThrowsException()
        {
            var context = await GetSeededDatabase();
            var sut = await GetBasicSutSetup(context);

            var previouslyMadeRequest = await CreateRequestByTypeAndStatus("Unpaid", "Approved", context);

            previouslyMadeRequest.RequesterId = REQUESTER_ID;
            previouslyMadeRequest.StartDate = DateTime.ParseExact("11/12/2022", "dd/MM/yyyy",
                       System.Globalization.CultureInfo.InvariantCulture);
            previouslyMadeRequest.EndDate = DateTime.ParseExact("12/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture);

                

            await context.AddAsync(previouslyMadeRequest);
            await context.SaveChangesAsync();
            var requestModel = new RequestCreateRequestModel()
            {
                StartDate = "12/07/2022",
                EndDate = "12/12/2022",
                Description = "Test description",
                Type = "Paid"
            };
            var requester = await context.Users.FirstOrDefaultAsync(u => u.Id == 1);

            await Assert.ThrowsAsync<ArgumentException>(
                () =>
                    sut.CreateAsync(requestModel, REQUESTER_ID));    
        }
        #endregion

        #region Delete
        [Fact]
        public async Task DeleteExistingRequest_RemovesRequest()
        {
            var context = await GetSeededDatabase();
            var mapper = MapperMock.Instance;
            var userManager = new Mock<IUserManager>();
            var emailService = new Mock<IEmailNotificationService>();

            var requestToBeDeleted = new TimeOffRequest
            {
                Id = 2,
                StatusId = 1,
                TypeId = 1,
                
            };
            var requestToStayInDb = new TimeOffRequest
            {
                Id = 3,
                StatusId = 1,
                TypeId = 1,
            };
            await context.TimeOffRequests.AddRangeAsync(requestToBeDeleted, requestToStayInDb);
            await context.SaveChangesAsync();
            var sut = new RequestService(userManager.Object, context, mapper, emailService.Object);

            var result = await sut.DeleteAsync(2);
            var expectedResult = MapperMock.Instance.Map<RequestResponseModel>(requestToBeDeleted);

            Assert.Equal(result.ToString(), expectedResult.ToString());
            Assert.Contains(requestToStayInDb, context.TimeOffRequests.ToList());
            Assert.DoesNotContain(requestToBeDeleted, context.TimeOffRequests.ToList());

        }

        [Fact]
        public async Task Delete_NonExistingRequest_ThrowsException()
        {
            var context = await GetSeededDatabase();
            var mapper = MapperMock.Instance;
            var userManager = new Mock<IUserManager>();
            var emailService = new Mock<IEmailNotificationService>();

            var sut = new RequestService(userManager.Object, context, mapper, emailService.Object);

            await Assert
                .ThrowsAsync<System.Collections.Generic.KeyNotFoundException>(() => sut.DeleteAsync(1));
        }
        #endregion

        #region Edit
        [Fact]
        public async Task EditRequest_ExistingRequest_ChangesProperties()
        {
            var context = await GetSeededDatabase();
            var sut = await GetBasicSutSetup(context);

            var typePaid = await context.RequestTypes.FirstOrDefaultAsync(t => t.Type == "Paid");
            var statusCreated = await context.Statuses.FirstOrDefaultAsync(s => s.State == "Created");

            var request = new TimeOffRequest()
            {
                Id = 1,
                StartDate = DateTime.ParseExact("12/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                EndDate = DateTime.ParseExact("13/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                Description = "Test description",
                Type = typePaid,
                Status = statusCreated,
                RequesterId = REQUESTER_ID
            };

            await context.AddAsync(request);
            await context.SaveChangesAsync();

            var requestModel = new RequestCreateRequestModel()
            {
                StartDate = "12/07/2022",
                EndDate = "12/10/2022",
                Description = "Test description changed",
                Type = "Unpaid"
            };

            var result = await sut.EditAsync(requestModel, 1, REQUESTER_ID);
            var timeElapsed = (DateTime.Now - Convert.ToDateTime(result.LastModificationDate)).TotalMilliseconds;
            const double MAX_MILISECONDS = 1100;
            Assert.Equal("Test description changed", result.Description);
            Assert.Equal("Unpaid", result.Type);
            Assert.Equal("Created", result.Status);
            Assert.Equal(REQUESTER_ID.ToString(), result.LastModifierId);
            Assert.True(timeElapsed < MAX_MILISECONDS);
        }

        [Fact]
        public async Task EditRequest_NonExistingRequest_ThrowsException()
        {
            var context = await GetSeededDatabase();
            var sut = await GetBasicSutSetup(context);

            var requestModel = new RequestCreateRequestModel()
            {
                StartDate = "15/12/2022",
                EndDate = "18/12/2022",
                Description = "Test description changed",
                Type = "Unpaid"
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => sut.EditAsync(requestModel, 1, REQUESTER_ID)
                );
        } 
        #endregion

        #region Submit        
        [Fact]
        public async Task SubmitRequest_AssignsApprovalsRight_AllAvailable()
        {
            var context = await GetSeededDatabase();
            var sut = await GetSutSetupWithApprovers(context);

            var typePaid = await context.RequestTypes.FirstOrDefaultAsync(t => t.Type == "Paid");
            var statusCreated = await context.Statuses.FirstOrDefaultAsync(s => s.State == "Created");
            var request = new TimeOffRequest()
            {
                Id = 1,
                StartDate = DateTime.ParseExact("12/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                EndDate = DateTime.ParseExact("13/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                Description = "Test description",
                Type = typePaid,
                Status = statusCreated,
                RequesterId = REQUESTER_ID
            };

            await context.AddAsync(request);
            await context.SaveChangesAsync();

            var result = await sut.SubmitAsync(1);
            var approver1 = await GetApprover1(context);
            var approver2 = await GetApprover2(context);

            var createdRequest = context.TimeOffRequests.FirstOrDefault();

            var expectedResult = MapperMock.Instance.Map<SubmitRequestResponseModel>(request);

            Assert.Equal(2, createdRequest.Approvers.Count);
            Assert.Contains(approver1, request.Approvers);
            Assert.Contains(approver2, request.Approvers);
            Assert.Equal(result.ToString(), expectedResult.ToString());

        }

        [Fact]
        public async Task SubmitRequest_AssignsApprovalsRight_OnlyAvailable()
        {
            var context = await GetSeededDatabase();
            var sut = await GetSutSetupWithApprovers(context);

            var statusApproved = await context.Statuses.FirstOrDefaultAsync(s => s.State == "Approved");
            var approver2Request = new TimeOffRequest

            {
                Id = 2,
                RequesterId = APPROVER2_ID,
                Status = statusApproved,
                StartDate = DateTime.Today.AddDays(-1),
                EndDate = DateTime.Today.AddDays(2)
            };

            var statusCreated = await context.Statuses.FirstOrDefaultAsync(s => s.State == "Created");
            var typePaid = await context.RequestTypes.FirstOrDefaultAsync(t => t.Type == "Paid");
            var createdRequest = new TimeOffRequest
            {
                Id = 1,
                RequesterId = 1,
                StartDate = DateTime.ParseExact("12/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                EndDate = DateTime.ParseExact("13/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                Description = "Test description",
                Type = typePaid,
                Status = statusCreated
            };

            await context.AddRangeAsync(approver2Request, createdRequest);
            await context.SaveChangesAsync();




            var result = await sut.SubmitAsync(1);
            var expectedResult = MapperMock.Instance.Map<SubmitRequestResponseModel>(createdRequest);

            var approver1 = await GetApprover1(context);
            var approver2 = await GetApprover2(context);
            Assert.Contains(approver1, createdRequest.Approvers);
            Assert.DoesNotContain(approver2, createdRequest.Approvers);
            Assert.Equal(expectedResult.ToString(), result.ToString());

        }

        [Fact]
        public async Task SubmitRequest_NoApprovers_IsAutoApproved()
        {
            var context = await GetSeededDatabase();
            var sut = await GetBasicSutSetup(context);

            var statusCreated = await context.Statuses.FirstOrDefaultAsync(s => s.State == "Created");
            var typePaid = await context.RequestTypes.FirstOrDefaultAsync(t => t.Type == "Paid");
            var createdRequest = new TimeOffRequest
            {
                Id = 1,
                RequesterId = REQUESTER_ID,
                StartDate = DateTime.ParseExact("12/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                EndDate = DateTime.ParseExact("13/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                Description = "Test description",
                Type = typePaid,
                Status = statusCreated
            };
            await context.AddAsync(createdRequest);
            await context.SaveChangesAsync();

            var result = await sut.SubmitAsync(1);
            var expectedResult = MapperMock.Instance.Map<SubmitRequestResponseModel>(createdRequest);
            Assert.Equal(expectedResult.ToString(), result.ToString());
            Assert.Equal("Approved", createdRequest.Status.State);
        }

        [Fact]
        public async Task SubmitRequest_StatusNotCreated_ThrowsException()
        {
            var context = await GetSeededDatabase();
            var sut = await GetBasicSutSetup(context);

            var statusAwaiting = await context.Statuses.FirstOrDefaultAsync(s => s.State == "Awaiting");
            var statusApproved = await context.Statuses.FirstOrDefaultAsync(s => s.State == "Approved");
            var statusRejected = await context.Statuses.FirstOrDefaultAsync(s => s.State == "Rejected");
            var typePaid = await context.RequestTypes.FirstOrDefaultAsync(t => t.Type == "Paid");
            var awaitingRequest = new TimeOffRequest
            {
                Id = 1,
                RequesterId = REQUESTER_ID,
                StartDate = DateTime.ParseExact("12/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                EndDate = DateTime.ParseExact("13/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                Description = "Test description",
                Type = typePaid,
                Status = statusAwaiting
            };
            var approvedRequest = new TimeOffRequest
            {
                Id = 2,
                RequesterId = REQUESTER_ID,
                StartDate = DateTime.ParseExact("12/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                EndDate = DateTime.ParseExact("13/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                Description = "Test description",
                Type = typePaid,
                Status = statusApproved
            };
            var rejectedRequest = new TimeOffRequest
            {
                Id = 3,
                RequesterId = REQUESTER_ID,
                StartDate = DateTime.ParseExact("12/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                EndDate = DateTime.ParseExact("13/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                Description = "Test description",
                Type = typePaid,
                Status = statusRejected,
            };

            await context.AddRangeAsync(awaitingRequest, approvedRequest, rejectedRequest);
            await context.SaveChangesAsync();

            await Assert.ThrowsAsync<InvalidRequestStatusTransitionException>(() => sut.SubmitAsync(1));
            await Assert.ThrowsAsync<InvalidRequestStatusTransitionException>(() => sut.SubmitAsync(2));
            await Assert.ThrowsAsync<InvalidRequestStatusTransitionException>(() => sut.SubmitAsync(3));

        }

        [Fact]
        public async Task SubmitRequest_TypeSick_WithApprovers_IsAutoApproved()
        {
            var context = await GetSeededDatabase();
            var sut = await GetSutSetupWithApprovers(context);

            
            var statusCreated = await context.Statuses.FirstOrDefaultAsync(s => s.State == "Created");
            var typeSick = await context.RequestTypes.FirstOrDefaultAsync(t => t.Type == "Sick");
            var createdRequest = new TimeOffRequest
            {
                Id = 1,
                RequesterId = REQUESTER_ID,
                StartDate = DateTime.ParseExact("12/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                EndDate = DateTime.ParseExact("13/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                Description = "Test description",
                Type = typeSick,
                Status = statusCreated
            };
            await context.AddAsync(createdRequest);
            await context.SaveChangesAsync();          

            var result = await sut.SubmitAsync(1);
            var expectedResult = MapperMock.Instance.Map<SubmitRequestResponseModel>(createdRequest);
            Assert.Equal(expectedResult.ToString(), result.ToString());
            Assert.Equal("Approved", createdRequest.Status.State);
        }

        [Fact]
        public async Task SubmitRequest_NoSick_WithApprovers_IsAwaiting()
        {
            var context = await GetSeededDatabase();
            var sut = await GetSutSetupWithApprovers(context);

            
            var statusCreated = await context.Statuses.FirstOrDefaultAsync(s => s.State == "Created");
            var typeUnpaid = await context.RequestTypes.FirstOrDefaultAsync(t => t.Type == "Unpaid");

            var createdRequest = new TimeOffRequest
            {
                Id = 1,
                RequesterId = REQUESTER_ID,
                StartDate = DateTime.ParseExact("12/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                EndDate = DateTime.ParseExact("13/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                Description = "Test description",
                Type = typeUnpaid,
                Status = statusCreated
            };
            
            await context.AddAsync(createdRequest);
            await context.SaveChangesAsync();
            
            var result = await sut.SubmitAsync(1);
            var expectedResult = MapperMock.Instance.Map<SubmitRequestResponseModel>(createdRequest);

            Assert.Equal(expectedResult.ToString(), result.ToString());
            Assert.Equal("Awaiting", createdRequest.Status.State);         
        }

        #endregion

        #region Approve
        [Fact]
        public async Task ApproveRequest_NotAllApprovers_StillAwaiting()
        {
            var context = await GetSeededDatabase();
            var sut = await GetSutSetupWithApprovers(context);

            var approver1 = await GetApprover1(context);
            var approver2 = await GetApprover2(context);

            var statusAwaiting = await context.Statuses.FirstOrDefaultAsync(s => s.State == "Awaiting");
            var typeUnpaid = await context.RequestTypes.FirstOrDefaultAsync(t => t.Type == "Unpaid");
            var createdRequest = new TimeOffRequest
            {
                Id = 1,
                RequesterId = 1,
                StartDate = DateTime.ParseExact("12/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                EndDate = DateTime.ParseExact("13/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                Description = "Test description",
                Type = typeUnpaid,
                Status = statusAwaiting
            };
            createdRequest.Approvers.Add(approver1);
            createdRequest.Approvers.Add(approver2);
            await context.AddAsync(createdRequest);
            await context.SaveChangesAsync();

            var result = await sut.ApproveAsync(1, 2);
            var expectedResult = MapperMock.Instance.Map<ApprovedRequestResponseModel>(createdRequest);

            Assert.Equal(expectedResult.ToString(), result.ToString());
            Assert.Equal("Awaiting", createdRequest.Status.State);
            var hasResponded = await context
                                       .Set<ApproverRequest>()
                                       .FirstOrDefaultAsync(ru =>
                                              ru.ApproverId == 2 &&
                                              ru.RequestId == 1);
            Assert.True(hasResponded.IsProcessed);
            var requestInDb = await context.TimeOffRequests.FirstOrDefaultAsync(r => r.Id == 1);
            Assert.Contains(approver1, requestInDb.Approvers);
            Assert.Contains(approver2, requestInDb.Approvers);
        }

        [Fact]
        public async Task ApproveRequest_AllApprovers_GetsApproved()
        {
            var context = await GetSeededDatabase();
            var sut = await GetSutSetupWithApprovers(context);

            var statusAwaiting = await context.Statuses.FirstOrDefaultAsync(s => s.State == "Awaiting");
            var typeUnpaid = await context.RequestTypes.FirstOrDefaultAsync(t => t.Type == "Unpaid");
            var approver1 = await GetApprover1(context);
            var approver2 = await GetApprover2(context);

            var createdRequest = new TimeOffRequest
            {
                Id = 1,
                RequesterId = REQUESTER_ID,
                StartDate = DateTime.ParseExact("12/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                EndDate = DateTime.ParseExact("13/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),

                Description = "Test description",
                Type = typeUnpaid,
                Status = statusAwaiting
            };
            createdRequest.Approvers.Add(approver1);
            createdRequest.Approvers.Add(approver2);
            await context.AddAsync(createdRequest);
            await context.SaveChangesAsync();

            var result1 = await sut.ApproveAsync(1, 2);
            var expectedResult1 = MapperMock.Instance.Map<ApprovedRequestResponseModel>(createdRequest);

            var result2 = await sut.ApproveAsync(1, 3);
            var expectedResult2 = MapperMock.Instance.Map<ApprovedRequestResponseModel>(createdRequest);

            Assert.Equal(expectedResult1.ToString(), result1.ToString());
            Assert.Equal(expectedResult2.ToString(), result2.ToString());

            Assert.Equal("Approved", createdRequest.Status.State);
        }

        [Fact]
        public async Task ApproveRequest_StatusNotAwaiting_ThrowsException()
        {
            var context = await GetSeededDatabase();
            var sut = await GetSutSetupWithApprovers(context);

           ;
            var statusCreated = await context.Statuses.FirstOrDefaultAsync(s =>  s.State == "Created");
            var statusApproved = await context.Statuses.FirstOrDefaultAsync(s => s.State == "Approved");
            var statusRejected = await context.Statuses.FirstOrDefaultAsync(s => s.State == "Rejected");

            var approver1 = await GetApprover1(context);
            var createdRequest = new TimeOffRequest
            {
                Id = 1,
                RequesterId = 1,
                StartDate = DateTime.ParseExact("12/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                EndDate = DateTime.ParseExact("13/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),

                Description = "Test description",
                Status = statusCreated
            };
            var approvedRequest = new TimeOffRequest
            {
                Id = 2,
                RequesterId = 1,
                StartDate = DateTime.ParseExact("12/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                EndDate = DateTime.ParseExact("13/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),

                Description = "Test description",
                Status = statusApproved
            };
            var rejectedRequest = new TimeOffRequest
            {
                Id = 3,
                RequesterId = 1,
                StartDate = DateTime.ParseExact("12/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                EndDate = DateTime.ParseExact("13/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),

                Description = "Test description",
                Status = statusRejected
            };
            createdRequest.Approvers.Add(approver1);
            approvedRequest.Approvers.Add(approver1);
            rejectedRequest.Approvers.Add(approver1);
            await context.AddRangeAsync(createdRequest, approvedRequest, rejectedRequest);
            await context.SaveChangesAsync();

            await Assert.ThrowsAsync<InvalidRequestStatusTransitionException>(
                () => sut.ApproveAsync(1, 2));

            await Assert.ThrowsAsync<InvalidRequestStatusTransitionException>(
                   () => sut.ApproveAsync(2, 2));
            
            await Assert.ThrowsAsync<InvalidRequestStatusTransitionException>(
                () => sut.ApproveAsync(3, 2));
        }

        [Fact]
        public async Task ApproveRequest_NonExistingRequest_ThrowsException()
        {
            var context = await GetSeededDatabase();
            var mapper = MapperMock.Instance;
            var userManager = new Mock<IUserManager>();
            var emailService = new Mock<IEmailNotificationService>();

            var user = new User() { UserName = APPROVER1_USERNAME, Id = APPROVER1_ID };



            await context.AddAsync(user);
            await context.SaveChangesAsync();

            var sut = new RequestService(userManager.Object, context, mapper, emailService.Object);

            await Assert.ThrowsAsync<System.Collections.Generic.KeyNotFoundException>(
                () => sut.ApproveAsync(1, APPROVER1_ID));
        }

        [Fact]
        public async Task ApproveRequest_TwiceBySameUser_ThrowsException()
        {
            var context = await GetSeededDatabase();
            var sut = await GetSutSetupWithApprovers(context);
            
            var statusAwaiting = await context.Statuses.FirstOrDefaultAsync(s => s.State == "Awaiting");
            var typeUnpaid = await context.RequestTypes.FirstOrDefaultAsync(t => t.Type == "Unpaid");
            var approver1 = await GetApprover1(context);

            var awaitingRequest = new TimeOffRequest
            {
                Id = 1,
                RequesterId = REQUESTER_ID,
                StartDate = DateTime.ParseExact("12/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                EndDate = DateTime.ParseExact("13/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),

                Description = "Test description",
                Type = typeUnpaid,
                Status = statusAwaiting
            };
            awaitingRequest.Approvers.Add(approver1);
            
            await context.AddAsync(awaitingRequest);
            await context.SaveChangesAsync();

            var result = await sut.ApproveAsync(1, APPROVER1_ID);

            await Assert.ThrowsAsync<InvalidRequestStatusTransitionException>(
                () => sut.ApproveAsync(1, APPROVER1_ID)
                );
        }
        #endregion

        #region Reject
        [Fact]
        public async Task RejectRequest_OnlyOneApprover_GetRejected()
        {
            var context = await GetSeededDatabase();
            var sut = await GetSutSetupWithApprovers(context);

            var statusAwaiting = await context.Statuses.FirstOrDefaultAsync(s => s.State == "Awaiting");
            var typeUnpaid = await context.RequestTypes.FirstOrDefaultAsync(t => t.Type == "Unpaid");

            var approver1 = await GetApprover1(context);
            var approver2 = await GetApprover2(context);

            var createdRequest = new TimeOffRequest
            {
                Id = 1,
                RequesterId = REQUESTER_ID,
                StartDate = DateTime.ParseExact("12/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                EndDate = DateTime.ParseExact("13/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                Description = "Test description",
                Type = typeUnpaid,
                Status = statusAwaiting
            };
            createdRequest.Approvers.Add(approver1);
            createdRequest.Approvers.Add(approver2);
            await context.AddAsync(createdRequest);
            await context.SaveChangesAsync();

            var result = await sut.RejectAsync(1, APPROVER1_ID);
            var expectedResult = MapperMock.Instance.Map<RejectedRequestResponseModel>(createdRequest);


            Assert.Equal(result.ToString(), expectedResult.ToString());
            Assert.Equal("Rejected", createdRequest.Status.State);
            var hasResponded = await context
                                       .Set<ApproverRequest>()
                                       .FirstOrDefaultAsync(ru =>
                                              ru.ApproverId == 2 &&
                                              ru.RequestId == 1);
            Assert.True(hasResponded.IsProcessed);
        }

        [Fact]
        public async Task RejectRequest_StatusNotAwaiting_ThrowsException()
        {
            var context = await GetSeededDatabase();
            var sut = await GetSutSetupWithApprovers(context);

            var statusCreated = await context.Statuses.FirstOrDefaultAsync(s =>  s.State == "Created");
            var statusRejected = await context.Statuses.FirstOrDefaultAsync(s => s.State == "Rejected");
            var statusApproved = await context.Statuses.FirstOrDefaultAsync(s => s.State == "Approved");

            var approver1 = await GetApprover1(context);
            
            var typeUnpaid = await context.RequestTypes.FirstOrDefaultAsync(t => t.Type == "Unpaid");
            var createdRequest = new TimeOffRequest
            {
                Id = 1,
                RequesterId = 1,
                StartDate = DateTime.ParseExact("12/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                EndDate = DateTime.ParseExact("13/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                Description = "Test description",
                Type = typeUnpaid,
                Status = statusCreated
            };
            var approvedRequest = new TimeOffRequest
            {
                Id = 2,
                RequesterId = 1,
                StartDate = DateTime.ParseExact("12/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                EndDate = DateTime.ParseExact("13/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                Description = "Test description",
                Type = typeUnpaid,
                Status = statusApproved
            };
            var rejectedRequest = new TimeOffRequest
            {
                Id = 3,
                RequesterId = 1,
                StartDate = DateTime.ParseExact("12/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                EndDate = DateTime.ParseExact("13/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                Description = "Test description",
                Type = typeUnpaid,
                Status = statusRejected
            };
            createdRequest.Approvers.Add(approver1);
            approvedRequest.Approvers.Add(approver1);
            rejectedRequest.Approvers.Add(approver1);
            await context.AddRangeAsync(createdRequest, approvedRequest, rejectedRequest);
            await context.SaveChangesAsync();

            await Assert.ThrowsAsync<InvalidRequestStatusTransitionException>(
                () => sut.ApproveAsync(1, 2));

            await Assert.ThrowsAsync<InvalidRequestStatusTransitionException>(
                   () => sut.ApproveAsync(2, 2));
            
            await Assert.ThrowsAsync<InvalidRequestStatusTransitionException>(
                () => sut.ApproveAsync(3, 2));
        }
        #endregion

        #region Get-methods
        [Fact]
        public async Task GetRequestById_ExistingRequest_Succeeds()
        {
            var context = await GetSeededDatabase();
            var sut = await GetBasicSutSetup(context);

            var statusAwaiting = await context.Statuses.FirstOrDefaultAsync(s => s.State == "Awaiting");
            var typeUnpaid = await context.RequestTypes.FirstOrDefaultAsync(t => t.Type == "Unpaid");

            var requestToBeTaken = new TimeOffRequest()
            {
                Id = 1,
                RequesterId = REQUESTER_ID,
                StartDate = DateTime.ParseExact("12/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                EndDate = DateTime.ParseExact("13/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                Description = "Test description",
                Type = typeUnpaid,
                Status = statusAwaiting
            };

            var fakeRequest = new TimeOffRequest()
            {
                Id = 2
            };

            await context.AddRangeAsync(requestToBeTaken, fakeRequest);
            await context.SaveChangesAsync();

            var result = await sut.GetRequestByIdDetailedAsync(1);

            Assert.Equal(requestToBeTaken.Id, result.Id);
        }

        [Fact]
        public async Task GetRequestById_ExistingRequest_GetApprovers()
        {
            var context = await GetSeededDatabase();
            var sut = await GetSutSetupWithApprovers(context);
            var statusAwaiting = await context.Statuses.FirstOrDefaultAsync(s => s.State == "Awaiting");
            var typeUnpaid = await context.RequestTypes.FirstOrDefaultAsync(t => t.Type == "Unpaid");

            var approver1 = await GetApprover1(context);
            var approver2 = await GetApprover2(context);

            var requestToBeTaken = new TimeOffRequest()
            {
                Id = 1,
                RequesterId = REQUESTER_ID,
                StartDate = DateTime.ParseExact("12/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                EndDate = DateTime.ParseExact("13/12/2022", "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture),
                Description = "Test description",
                Type = typeUnpaid,
                Status = statusAwaiting
            };
            requestToBeTaken.Approvers.Add(approver1);
            requestToBeTaken.Approvers.Add(approver2);
            await context.AddAsync(requestToBeTaken);
            await context.SaveChangesAsync();

            var expectedListModel =
                MapperMock
                .Instance
                .Map<ICollection<string>>(new[] { approver1.UserName, approver2.UserName });

            var result = await sut.GetRequestByIdDetailedAsync(1);

            Assert.Collection(result.Approvers,
                                u => expectedListModel.Contains(u),
                                u => expectedListModel.Contains(u));
        }

        [Fact]
        public async Task GetRequestById_NonExistingRequest_ThrowsException()
        {
            var context = DatabaseMock.Instance;
            var sut = await GetBasicSutSetup(context);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => sut.GetRequestByIdDetailedAsync(1));
        }

        //To fix the model, this test must succeed when done
        [Fact]
        public async Task GetRequestsByUser_ReturnsOnlyByUserAndNoOthers()
        {
            var context = await GetSeededDatabase();
            var sut = await GetSutSetupWithApprovers(context);

            var requests = await GetRequests(context);

            await context.AddRangeAsync(requests);
            await context.SaveChangesAsync();

            var result = await sut.GetRequestsMadeByUserAsync(REQUESTER_ID);

            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.Equal(REQUESTER_ID.ToString(), r.RequesterId));

        }

        [Fact]
        public async Task GetRequestsByUser_ReturnsEmptyWhenThereAreNone()
        {
            var context = DatabaseMock.Instance;
            var sut = await GetBasicSutSetup(context);

            var result = await sut.GetRequestsMadeByUserAsync(1);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetRequestsByUser_NonExistingUser_ThrowsException()
        {
            var context = DatabaseMock.Instance;
            var userManagerMock = new Mock<IUserManager>();
            var emailServiceMock = new Mock<IEmailNotificationService>();
            var mapper = MapperMock.Instance;

            var nonExistingUser = new User { Id = 5 };
            userManagerMock.Setup(getUser => getUser.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(nonExistingUser); ;

            var sut = new RequestService(userManagerMock.Object, context, mapper, emailServiceMock.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.GetRequestsMadeByUserAsync(0));
        }

        [Fact]
        public async Task GetRequestsByStatus_Succeds()
        {
            var context = await GetSeededDatabase();
            var sut = await GetBasicSutSetup(context);

            var requests = await GetRequests(context);
            await context.AddRangeAsync(requests);
            await context.SaveChangesAsync();
            var result = await sut.GetRequestsByStatusAsync("Created");

            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.Equal("Created", r.Status));

        }

        [Fact]
        public async Task GetRequestsToBeProcessedByUser_ReturnsOnlyAwaitingRequests()
        {
            var context = await GetSeededDatabase();
            var sut = await GetSutSetupWithApprovers(context);
            var requests = await GetRequests(context);

            var approver1 = await GetApprover1(context);
            var request4 = await CreateRequestByTypeAndStatus("Unpaid", "Awaiting", context);
            var request5 = await CreateRequestByTypeAndStatus("Paid", "Awaiting", context);

            request4.RequesterId = request5.RequesterId = REQUESTER_ID;
            request4.Approvers.Add(approver1);
            request5.Approvers.Add(approver1);

            request4.Id = 5;

            await context.AddRangeAsync(requests);
            await context.AddRangeAsync(request4, request5);
            await context.SaveChangesAsync();

            var resultBeforeApproving = await sut.GetRequestsToBeProcessedByApproverAsync(approver1);

            Assert.Equal(2, resultBeforeApproving.Count);
            Assert.All(resultBeforeApproving, r => Assert.Equal("Awaiting", r.Status));

            await sut.ApproveAsync(5, APPROVER1_ID);
            
            var resultAfterApproving = await sut.GetRequestsToBeProcessedByApproverAsync(approver1);

            Assert.Equal(1, resultAfterApproving.Count);
            Assert.All(resultBeforeApproving, r => Assert.Equal("Awaiting", r.Status));
        }

        //Will be passed after bug is solved
        [Fact]
        public async Task GetRequestsToBeProcessedByUser_ReturnsOnlyNotProcessedByHim()
        {
            var context = await GetSeededDatabase();
            var sut = await GetSutSetupWithApprovers(context);

            var approver1 = await GetApprover1(context);
            var approver2 = await GetApprover2(context);
            var request4 = await CreateRequestByTypeAndStatus("Unpaid", "Awaiting", context);
            var request5 = await CreateRequestByTypeAndStatus("Paid", "Awaiting", context);

            request4.RequesterId = request5.RequesterId = REQUESTER_ID;
            request4.Approvers.Add(approver1);
            request4.Approvers.Add(approver2);
            request5.Approvers.Add(approver1);

            request4.Id = 5;

            await context.AddRangeAsync(request4, request5);
            await context.SaveChangesAsync();

            var resultBeforeApproving = await sut.GetRequestsToBeProcessedByApproverAsync(approver1);

            Assert.Equal(2, resultBeforeApproving.Count);
            Assert.All(resultBeforeApproving, r => Assert.Equal("Awaiting", r.Status));

            await sut.ApproveAsync(5, APPROVER1_ID);

            var resultAfterApproving = await sut.GetRequestsToBeProcessedByApproverAsync(approver1);

            Assert.Equal(1, resultAfterApproving.Count);
            Assert.All(resultBeforeApproving, r => Assert.Equal("Awaiting", r.Status));
        }
        #endregion

        [Fact]
        public async Task IsApprover_ExistingRequest_Succeeds()
        {
            var context = await GetSeededDatabase();
            var sut = await GetSutSetupWithApprovers(context);

            var approver1 = await GetApprover1(context);

            var request = await CreateRequestByTypeAndStatus("Paid", "Created", context);
            request.Id = 1;
            request.RequesterId = REQUESTER_ID;
            request.Approvers.Add(approver1);
            await context.AddAsync(request);
            await context.SaveChangesAsync();

            var isApprover1 = await sut.IsTheUserRequestApproverAsync(request.Id, APPROVER1_ID);
            var isApprover2 = await sut.IsTheUserRequestApproverAsync(request.Id, APPROVER2_ID);

            Assert.True(isApprover1);
            Assert.False(isApprover2);
        }

        [Fact]
        public async Task IsRequestCreator_ExistingRequest_Succeeds()
        {
            var context = await GetSeededDatabase();
            var sut = await GetSutSetupWithApprovers(context);

            var request = await CreateRequestByTypeAndStatus("Paid", "Created", context);
            request.Id = 1;
            request.RequesterId = REQUESTER_ID;
            request.CreatorId = REQUESTER_ID;
            await context.AddAsync(request);
            await context.SaveChangesAsync();

            var isCreatorRequester = await sut.IsTheUserRequestCreatorAsync(request.Id, REQUESTER_ID);
            var isCreatorApprover2 = await sut.IsTheUserRequestCreatorAsync(request.Id, APPROVER2_ID);

            Assert.True(isCreatorRequester);
            Assert.False(isCreatorApprover2);
        }
        #region Helpers
        private async Task<WorkforceManagerDbContext> GetSeededDatabase()
        {
            var context = DatabaseMock.Instance;
            var created = new TimeOffRequestStatus
            {
                Id = 1,
                State = "Created"
            };
            var awaiting = new TimeOffRequestStatus
            {
                Id = 2,
                State = "Awaiting"
            };
            var approved = new TimeOffRequestStatus
            {
                Id = 3,
                State = "Approved"
            };
            var rejected = new TimeOffRequestStatus
            {
                Id = 4,
                State = "Rejected"
            };
            var paid = new TimeOffRequestType
            {
                Id = 1,
                Type = "Paid"
            };
            var unpaid = new TimeOffRequestType
            {
                Id = 2,
                Type = "Unpaid"
            };
            var sick = new TimeOffRequestType
            {
                Id = 3,
                Type = "Sick"
            };
            await context.Statuses.AddRangeAsync(created, awaiting, approved, rejected);
            await context.RequestTypes.AddRangeAsync(paid, unpaid, sick);
            await context.SaveChangesAsync();
            return context;
        }

        private async Task<RequestService> GetBasicSutSetup(WorkforceManagerDbContext context)
        {
            var mapper = MapperMock.Instance;
            var userManager = new Mock<IUserManager>();
            var emailService = new Mock<IEmailNotificationService>();

            var requester = new User() { Id = REQUESTER_ID, UserName = REQUESTER_USERNAME };
            await context.AddAsync(requester);
            await context.SaveChangesAsync();
            userManager.Setup(getUser => getUser.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(requester);
            var sut = new RequestService(userManager.Object, context, mapper, emailService.Object);

            return sut;
        }

        private async Task<RequestService> GetSutSetupWithApprovers(WorkforceManagerDbContext context)
        {
            var sut = await GetBasicSutSetup(context);

            var approver1 = new User()
            {
                UserName = APPROVER1_USERNAME,
                Id = APPROVER1_ID,
                Email = APPROVER1_EMAIL
            };
            var approver2 = new User()
            {
                UserName = APPROVER2_USERNAME,
                Id = APPROVER2_ID,
                Email = APPROVER2_EMAIL
            };

            var team1 = new Team
            {
                TeamLeaderId = 2
            };
            var team2 = new Team
            {
                TeamLeaderId = 3
            };
            var creatorOfRequest = await GetRequester(context);
            team1.Members.Add(creatorOfRequest);
            team2.Members.Add(creatorOfRequest);
            await context.AddRangeAsync(approver1, approver2);
            await context.AddRangeAsync(team1, team2);
            await context.SaveChangesAsync();

            return sut;
        }

        private async Task<User> GetRequester(WorkforceManagerDbContext context)
        {
            return await context.Users.FirstOrDefaultAsync(u => u.Id == REQUESTER_ID);
        }

        private async Task<User> GetApprover1(WorkforceManagerDbContext context)
        {
            return await context.Users.FirstOrDefaultAsync(u => u.Id == APPROVER1_ID);
        }

        private async Task<User> GetApprover2(WorkforceManagerDbContext context)
        {
            return await context.Users.FirstOrDefaultAsync(u => u.Id == APPROVER2_ID);
        }

        private async Task<TimeOffRequest> CreateRequestByTypeAndStatus(string type, string status, WorkforceManagerDbContext context)
        {
            var result = new TimeOffRequest();
            result.Status = await context
                                    .Statuses
                                    .FirstOrDefaultAsync(s => s.State == status);

            result.Type = await context
                                    .RequestTypes
                                    .FirstOrDefaultAsync(t => t.Type == type);

            return result;
        }

        private async Task<ICollection<TimeOffRequest>> GetRequests(WorkforceManagerDbContext context)
        {
            var request1 = await CreateRequestByTypeAndStatus("Paid", "Created", context);
            var request2 = await CreateRequestByTypeAndStatus("Unpaid", "Created", context);
            var request3 = await CreateRequestByTypeAndStatus("Paid", "Awaiting", context);

            request1.RequesterId = REQUESTER_ID;
            request2.RequesterId = APPROVER1_ID;
            request3.RequesterId = REQUESTER_ID;

            var result = new HashSet<TimeOffRequest>();
            result.Add(request1);
            result.Add(request2);
            result.Add(request3);
            return result;
        }
        #endregion
    }
}
