namespace WorkforceManager.Services
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoMapper;
    using Microsoft.EntityFrameworkCore;
    using Data;
    using Data.Entities;
    using Models.Requests.RequestRequestModels;
    using Models.Responses.RequestsResponseModels;
    using Services.Exceptions;
    using Services.Interfaces;
    using WorkforceManager.Data.Constants;

    public class RequestService : IRequestService
    {
        private readonly IUserManager _userManager;
        private readonly IEmailNotificationService _emailService;
        private readonly WorkforceManagerDbContext _dbContext;
        private readonly IMapper _mapper;

        public RequestService(IUserManager userManager, WorkforceManagerDbContext dbContext, IMapper mapper, IEmailNotificationService emailService)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _mapper = mapper;
            _emailService = emailService;
        }

        public async Task<RequestResponseModel> CreateAsync(RequestCreateRequestModel createModel, int creatorId)
        {
            return await CreateAsync(createModel, creatorId, creatorId);
        }

        public async Task<RequestResponseModel> CreateAsync(RequestCreateRequestModel createModel, int creatorId, int requesterId)
        {
            if (await _userManager.FindByIdAsync(requesterId.ToString()) == null)
            {
                throw new ArgumentException(string.Format(UserServiceConstants.UserDoesNotExistErrorMessage, requesterId));
            }
            var offRequest = _mapper.Map<TimeOffRequest>(createModel);
            offRequest.RequesterId = requesterId;
            offRequest.CreatorId = creatorId;
            offRequest.LastModifierId = creatorId;

            var overlappingRequest = await OverlappingRequest(offRequest);

            if (overlappingRequest != null)
            {
                throw new ArgumentException(
                    string.Format(RequestServiceConstants.OverlappingRequestErrorMessage,
                                        offRequest.RequesterId,
                                        overlappingRequest.StartDate.ToString("dd/MM/yyyy"),
                                        overlappingRequest.EndDate.ToString("dd/MM/yyyy")));
            }

            offRequest.Status = await _dbContext.Statuses.FirstOrDefaultAsync(s => s.State.ToLower() == "created");
            offRequest.Type = await _dbContext.RequestTypes.FirstOrDefaultAsync(t => t.Type.ToLower() == createModel.Type.ToLower());

            await _dbContext.TimeOffRequests.AddAsync(offRequest);
            await _dbContext.SaveChangesAsync();

            return _mapper.Map<RequestResponseModel>(offRequest);
        }

        public async Task<SubmitRequestResponseModel> SubmitAsync(int requestId)
        {
            var offRequest = await GetRequestByIdAsync(requestId);
            if (offRequest.Status.State != "Created")
                throw new InvalidRequestStatusTransitionException(RequestServiceConstants.SubmitAlreadySubmitedRequestErrorMessage);
                                                                             

            offRequest.Approvers = await GetAvailableApproversAsync(offRequest.RequesterId);

            if (offRequest.Approvers.Count == 0 || offRequest.Type.Type == "Sick")
                return await AutoApproveRequest(offRequest);


            await ChangeStatusAsync(offRequest, "Awaiting");
            var submitRequestResponseModel = _mapper.Map<SubmitRequestResponseModel>(offRequest);
            await _emailService.SendSubmitNotification(submitRequestResponseModel, offRequest);

            return submitRequestResponseModel;
        }

        public async Task<ApprovedRequestResponseModel> ApproveAsync(int requestId, int approverId)
        {
            var offRequest = await GetRequestByIdAsync(requestId);

            if (offRequest.Status.State != "Awaiting")
                throw new InvalidRequestStatusTransitionException(RequestServiceConstants.ApproveNotAwaitingRequestErrorMessage);

            await RequestProcessingAsync(approverId, requestId);
            var isTheRequestProcessedByRestOfTheApproversAsync = await IsTheRequestProcessedByRestOfTheApproversAsync(requestId);

            var approvedRequestModel = _mapper.Map<ApprovedRequestResponseModel>(offRequest);

            if (isTheRequestProcessedByRestOfTheApproversAsync)
            {
                await ChangeStatusAsync(offRequest, "Approved");
                approvedRequestModel.RequestStatusState = "Approved";
                await _emailService.SendApprovedNotification(approvedRequestModel, offRequest);
            }

            return approvedRequestModel;
        }

        public async Task<RejectedRequestResponseModel> RejectAsync(int requestId, int approverId)
        {
            var offRequest = await GetRequestByIdAsync(requestId);
            if (offRequest.Status.State != "Awaiting")
                throw new InvalidRequestStatusTransitionException(RequestServiceConstants.RejectNotAwaitingRequestErrorMessage);

            await RequestProcessingAsync(approverId, requestId);
            await ChangeStatusAsync(offRequest, "Rejected");

            var rejectRequestResponseModel = _mapper.Map<RejectedRequestResponseModel>(offRequest);

            await _emailService.SendRejectedNotification(rejectRequestResponseModel,offRequest, approverId);

            return rejectRequestResponseModel;
        }

        public async Task<RequestResponseModel> EditAsync(RequestCreateRequestModel editModel, int requestId, int modifierId)
        {
            var offRequest = await GetRequestByIdAsync(requestId);

            if (offRequest.Status.State.ToLower() == "approved" || offRequest.Status.State.ToLower() == "rejected")
                throw new ArgumentException(string.Format(RequestServiceConstants.AlreadyProcessedRequestErrorMessage, requestId));

            offRequest.StartDate = Convert.ToDateTime(editModel.StartDate);
            offRequest.EndDate = Convert.ToDateTime(editModel.EndDate);
            
            var overlappingRequest = await OverlappingRequest(offRequest);

            if (overlappingRequest != null && overlappingRequest.Id != requestId)
            {
                throw new ArgumentException(
                    string.Format(RequestServiceConstants.OverlappingRequestErrorMessage,
                                        offRequest.RequesterId,
                                        overlappingRequest.StartDate.ToString("dd/MM/yyyy"),
                                        overlappingRequest.EndDate.ToString("dd/MM/yyyy")));
            }


            offRequest.Description = editModel.Description;
            offRequest.LastModifierId = modifierId;
            offRequest.LastModificationDate = DateTime.Now;
            offRequest.Type = await _dbContext.RequestTypes.FirstOrDefaultAsync(t => t.Type == editModel.Type);

            var approversRequest = await _dbContext.Set<ApproverRequest>().Where(ar => ar.RequestId == requestId).ToListAsync();

            foreach (var request in approversRequest)
            {
                request.IsProcessed = false;
            }
            if (approversRequest.Any())
            {
                await ChangeStatusAsync(offRequest, "Created");
                await SubmitAsync(requestId);
            }

            await _dbContext.SaveChangesAsync();

            return _mapper.Map<RequestResponseModel>(offRequest);
        }

        public async Task<RequestResponseModel> DeleteAsync(int requestId)
        {
            var offRequest = await GetRequestByIdAsync(requestId);

            if (offRequest.Status.State == "Approved" || offRequest.Status.State == "Rejected")
                throw new ArgumentException(string.Format(RequestServiceConstants.DeleteAlreadyProcessedRequestErrorMessage, requestId));

            var result = _mapper.Map<RequestResponseModel>(offRequest);
            _dbContext.TimeOffRequests.Remove(offRequest);
            await _dbContext.SaveChangesAsync();

            result.Status = "DELETED";

            return result;
        }

        public async Task<RequestDetailedResponseModel> GetRequestByIdDetailedAsync(int requestId)
        {
            var offRequest = await GetRequestByIdAsync(requestId);

            return _mapper.Map<RequestDetailedResponseModel>(offRequest);
        }

        public async Task<ICollection<RequestResponseModel>> GetRequestsMadeByUserAsync(int userId)
        {
            if (!(await _dbContext.Users.AnyAsync(u => u.Id == userId)))
                throw new KeyNotFoundException(string.Format(UserServiceConstants.UserDoesNotExistErrorMessage, userId));

            var requests = await _dbContext.TimeOffRequests
                .Where(r => r.RequesterId == userId)
                .ToListAsync();

            var requestResponseDto = _mapper.Map<ICollection<RequestResponseModel>>(requests);
            return requestResponseDto.ToList();
        }

        public async Task<ICollection<RequestResponseModel>> GetRequestsToBeProcessedByApproverAsync(User teamLeader)
        {
            var approverAllRequests = await _dbContext.TimeOffRequests
                .Where(r => r.Approvers.Any(a => a.Id == teamLeader.Id) && r.Status.State == "Awaiting")
                .ToListAsync();

            var requestsToBeProcessed = new List<TimeOffRequest>();

            foreach (var request in approverAllRequests)
            {
                var isProcessed = await IsTheRequestProcessedByCurrentApprover(request.Id, teamLeader.Id);
                if (!isProcessed)
                {
                    requestsToBeProcessed.Add(request);
                }
            }

            var requestResponseDto = _mapper.Map<ICollection<RequestResponseModel>>(requestsToBeProcessed);

            return requestResponseDto;
        }

        public async Task<ICollection<RequestResponseModel>> GetRequestsByStatusAsync(string status)
        {
            var statusFromDb = await _dbContext.Statuses
                .FirstOrDefaultAsync(s => s.State.ToLower() == status.ToLower());

            if (statusFromDb == null)
                throw new ArgumentException(RequestServiceConstants.InvalidStatusChosenErrorMessage);

            var requests = statusFromDb.Requests.ToList();
            return _mapper.Map<ICollection<RequestResponseModel>>(requests);
        }

        #region Policies
        public async Task<bool> IsTheUserRequestApproverAsync(int requestId, int userId)
        {
            var request = await GetRequestByIdAsync(requestId);

            return request.Approvers.Any(a => a.Id == userId);
        }

        public async Task<bool> IsTheUserRequestCreatorAsync(int requestId, int userId)
        {
            var request = await GetRequestByIdAsync(requestId);

            return request.RequesterId == userId;
        }

        #endregion

        #region Helper methods

        private async Task ChangeStatusAsync(TimeOffRequest request, string newStatus)
        {
            request.Status = await _dbContext.Statuses.FirstOrDefaultAsync(s => s.State == newStatus);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<TimeOffRequest> GetRequestByIdAsync(int requestId)
        {
            var offRequest = await _dbContext
                .TimeOffRequests
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (offRequest == null)
                throw new KeyNotFoundException(string.Format(
                                                    RequestServiceConstants.NonExistingRequestErrorMessage,
                                                    requestId));

            return offRequest;
        }

        private async Task<ICollection<User>> GetAvailableApproversAsync(int requesterId)
        {
            var requester = await _userManager.FindByIdAsync(requesterId.ToString());

            var approvers = requester.Teams
                .Where(a => a.TeamLeaderId.HasValue && a.TeamLeaderId != requester.Id)
                .Select(a=> a.TeamLeader)
                .Where(tl => tl.Requests
                .All(
                    r => 
                    r.StartDate > DateTime.Now ||
                    r.EndDate < DateTime.Now ||
                    r.Status.State != "Approved"))
                   .ToList();

            return approvers;
        }

        private async Task<bool> RequestProcessingAsync(int approverId, int requestId)
        {
            var isProcessedByCurrentUser = await IsTheRequestProcessedByCurrentApprover(requestId, approverId);

            if (isProcessedByCurrentUser)
                throw new InvalidRequestStatusTransitionException("The request already has been processed by you!");
            
            var userRequestProcess = await _dbContext
                .Set<ApproverRequest>()
                .FirstOrDefaultAsync(ru => ru.ApproverId == approverId && ru.RequestId == requestId);

            userRequestProcess.IsProcessed = true;
            await _dbContext.SaveChangesAsync();

            return userRequestProcess.IsProcessed;
        }

        private async Task<bool> IsTheRequestProcessedByRestOfTheApproversAsync(int requestId)
        {
            var result = await _dbContext
                .Set<ApproverRequest>()
                .AllAsync(ru => ru.RequestId != requestId || ru.IsProcessed);

            return result;
        }

        private async Task<bool> IsTheRequestProcessedByCurrentApprover(int requestId, int approverId)
        {
            var approverRequest = await _dbContext
                .Set<ApproverRequest>()
                .FirstOrDefaultAsync(r => r.ApproverId == approverId && r.RequestId == requestId);
      
            return approverRequest.IsProcessed;
        }

        private async Task<SubmitRequestResponseModel> AutoApproveRequest(TimeOffRequest request)
        {
            var recipients = new List<string>();

            if (request.Type.Type.ToLower() == "sick")
            {
                recipients = request.Requester.Teams.SelectMany(x => x.Members.Select(m => m.Email)).ToList();
                if (recipients.Count == 0)
                {
                    recipients = new List<string>() { request.Requester.Email };
                }
            }
            else
            {
                recipients = new List<string>() { request.Requester.Email };
            }

            var approversRequest = await _dbContext.Set<ApproverRequest>().Where(ar => ar.RequestId == request.Id).ToListAsync();

            foreach (var approverRequest in approversRequest)
            {
                approverRequest.IsProcessed = true;
            }

            await ChangeStatusAsync(request, "Approved");
            var autoApprovedRequestResponseModel = _mapper.Map<ApprovedRequestResponseModel>(request);
            await _emailService.SendAutoApprovedNotification(autoApprovedRequestResponseModel, request, recipients);

            var submitRequestResponseModel = _mapper.Map<SubmitRequestResponseModel>(request);
            return submitRequestResponseModel;
        }

        private async Task<TimeOffRequest> OverlappingRequest (TimeOffRequest request)
        {
            var overlappingRequest = await _dbContext
                                .TimeOffRequests
                                .FirstOrDefaultAsync(
                                    r => r.RequesterId == request.RequesterId //same requester
                                        && !(r.EndDate <= request.StartDate)  //new is not before old
                                        && !(r.StartDate > request.EndDate)  // new is not after old
                                        && r.Status.State != "Rejected");

            return overlappingRequest;
        }

        #endregion
    }
}
