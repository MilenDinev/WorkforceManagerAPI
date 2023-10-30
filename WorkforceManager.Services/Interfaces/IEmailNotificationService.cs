namespace WorkforceManager.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Data.Entities;
    using WorkforceManager.Models.Responses.RequestsResponseModels;

    public interface IEmailNotificationService
    {
        Task SendNotification(List<string> recipients, string subject, string messageText);
        Task SendSubmitNotification(SubmitRequestResponseModel submitRequestResponseModel, TimeOffRequest request);
        Task SendApprovedNotification(ApprovedRequestResponseModel approveRequestModel, TimeOffRequest request);
        Task SendAutoApprovedNotification(ApprovedRequestResponseModel autoApprovedRequestResponseModel, TimeOffRequest request, List<string> recipients);
        Task SendRejectedNotification(RejectedRequestResponseModel rejectResponseModel, TimeOffRequest request, int approverId);
        Task SendMemberAddedToTeamNotification(Team team, User newMember, int requestsCount);
        Task SendMemberRemovedFromTeamNotification(Team team, User member);
    }
}
