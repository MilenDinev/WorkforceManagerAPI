namespace WorkforceManager.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models.Requests.TeamRequestModels;
    using Models.Responses.TeamResponseModels;
    using WorkforceManager.Data.Entities;

    public interface ITeamService
    {
        Task<CreatedTeamResponseModel> CreateAsync(TeamCreateRequestModel teamRequestModel, int creatorId);

        Task<TeamResponseModel> EditAsync(TeamEditRequestModel teamRequestModel, int teamId, int modifierId);

        Task<TeamMemberResponseModel> AddMember(int teamId, int memberId, int modifierId);

        Task<TeamMemberResponseModel> RemoveMember(int teamId, int memberId, int modifierId);

        Task<TeamResponseModel> DeleteAsync(int teamId);

        Task<IEnumerable<TeamResponseModel>> GetAllTeamsAsync();

        Task<Team> GetTeamAsync(int teamId);

        Task<TeamMemberResponseModel> PromoteToTeamLeaderAsync(int teamId, int userId, int modifierId);

    }
}
