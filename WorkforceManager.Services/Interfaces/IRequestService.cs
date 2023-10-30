namespace WorkforceManager.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using WorkforceManager.Data.Entities;
    using WorkforceManager.Models.Requests.RequestRequestModels;
    using WorkforceManager.Models.Responses.RequestsResponseModels;

    public interface IRequestService
    {
        Task<RequestResponseModel> CreateAsync(RequestCreateRequestModel createModel, int creatorId);
        Task<RequestResponseModel> CreateAsync(RequestCreateRequestModel createModel, int creatorId, int requesterId);
        Task<RequestResponseModel> EditAsync(RequestCreateRequestModel editModel, int requestId, int modifierId);
        Task<RequestResponseModel> DeleteAsync(int requestId);
        Task<SubmitRequestResponseModel> SubmitAsync(int requestId);
        Task<ApprovedRequestResponseModel> ApproveAsync(int requestId, int approverId);
        Task<RejectedRequestResponseModel> RejectAsync(int requestId, int approverId);
        Task<RequestDetailedResponseModel> GetRequestByIdDetailedAsync(int requestId);
        Task<ICollection<RequestResponseModel>> GetRequestsMadeByUserAsync(int userId);
        Task<ICollection<RequestResponseModel>> GetRequestsToBeProcessedByApproverAsync(User teamLeader);
        Task<ICollection<RequestResponseModel>> GetRequestsByStatusAsync(string status);
        Task<bool> IsTheUserRequestCreatorAsync(int requestId, int userId);
        Task<bool> IsTheUserRequestApproverAsync(int requestId, int userId);
    }
}
