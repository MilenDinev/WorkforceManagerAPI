namespace WorkforceManager.Services
{
    using AutoMapper;
    using System;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Data;
    using Data.Entities;
    using Models.Requests.TeamRequestModels;
    using Services.Interfaces;
    using WorkforceManager.Models.Responses.TeamResponseModels;
    using System.Collections.Generic;
    using System.Linq;
    using WorkforceManager.Data.Constants;

    public class TeamService : ITeamService
    {
        private readonly WorkforceManagerDbContext _dbContext;
        private readonly IEmailNotificationService _emailNotificationService;
        private readonly IMapper _mapper;

        public TeamService(WorkforceManagerDbContext dbContext, IMapper mapper, IEmailNotificationService emailNotificationService)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _emailNotificationService = emailNotificationService;
        }

        public async Task<CreatedTeamResponseModel> CreateAsync(TeamCreateRequestModel teamRequestModel, int creatorId)
        {
            var isValidTeamLeadId = int.TryParse(teamRequestModel.TeamLeaderId, out int teamLeaderId);

            var isTheTeamNameAlreadyTaken = await _dbContext.Teams.AnyAsync(t => t.Title.ToLower() == teamRequestModel.Title.ToLower());
            if (isTheTeamNameAlreadyTaken)
                throw new ArgumentException(string.Format(TeamServiceConstants.TeamTitleAlreadyExistsErrorMessage,
                                                            teamRequestModel.Title));

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == teamLeaderId);

            if (user == null && isValidTeamLeadId)
                throw new KeyNotFoundException(string.Format(UserServiceConstants.UserDoesNotExistErrorMessage ,teamRequestModel.TeamLeaderId));

            if (user != null && user.TeamsLed.Any())
                throw new ArgumentException(string.Format(TeamServiceConstants.UserAlreadyTeamLeader,user.Id));

            var team = _mapper.Map<Team>(teamRequestModel);
            team.CreatorId = creatorId;
            team.LastModifierId = creatorId;
            team.TeamLeader = user;
            if (user != null)
            {
                team.Members.Add(user);
            }
            await _dbContext.Teams.AddAsync(team);

            await _dbContext.SaveChangesAsync();
            return _mapper.Map<CreatedTeamResponseModel>(team);
        }

        public async Task<TeamResponseModel> EditAsync(TeamEditRequestModel teamRequestModel, int teamId, int modifierId)
        {
            var isValidTeamLeadId = int.TryParse(teamRequestModel.TeamLeaderId, out int teamLeaderId);

            var teams = await _dbContext.Teams.ToListAsync();
            var team = await GetTeamAsync(teamId);

            var isNewNameTaken = teams.Any(t => t.Title.ToLower() == teamRequestModel.Title.ToLower() && t.Id != team.Id);
            if (isNewNameTaken)
                throw new ArgumentException(string.Format(TeamServiceConstants.TeamTitleAlreadyExistsErrorMessage,
                                                            teamRequestModel.Title));

            if (isValidTeamLeadId)
            {
                var isNewTeamLeadPartOfTheTeam = team.Members.Any(x => x.Id == teamLeaderId);
                if (!isNewTeamLeadPartOfTheTeam )
                    throw new KeyNotFoundException(string.Format(TeamServiceConstants.UserNotMemberOfTeam,teamRequestModel.TeamLeaderId, team.Title));
            }



            team.Title = teamRequestModel.Title;
            team.Description = teamRequestModel.Description;
            team.TeamLeaderId = teamLeaderId != 0 ? teamLeaderId : null;
            team.LastModifierId = modifierId;
            team.LastModificationDate = DateTime.Now; 
            
            await _dbContext.SaveChangesAsync();

            var teamResponse = _mapper.Map<TeamResponseModel>(team);

            return teamResponse;
        }

        public async Task<TeamResponseModel> DeleteAsync(int teamId)
        {
            var team = await GetTeamAsync(teamId);
            team.Members.Clear();

            if (team.TeamLeader != null)
            {
                team.TeamLeader.RequestsToApprove.Clear();
            }

            var result = _mapper.Map<TeamResponseModel>(team);
            _dbContext.Remove(team);

            await _dbContext.SaveChangesAsync();
            return result;
        }

        public async Task<TeamMemberResponseModel> AddMember(int teamId, int memberId, int modifierId)
        {
            var team = await GetTeamAsync(teamId);
            var member = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == memberId);

            if (member == null)
                throw new KeyNotFoundException(string.Format(
                                                    UserServiceConstants.UserDoesNotExistErrorMessage,
                                                    memberId));

            if (team.Members.Any(m => m.UserName == member.UserName))
                throw new ArgumentException(string.Format(TeamServiceConstants.UserAlreadyMemberOfTeam,
                                                            member.UserName,
                                                            team.Title));

            team.Members.Add(member);
            var requests = member.Requests.Where(r => r.Status.State == "Awaiting");

            foreach (var request in requests)
            {
                request.Approvers.Add(team.TeamLeader);
            }

            team.LastModifierId = modifierId;
            team.LastModificationDate = DateTime.Now;

            await _dbContext.SaveChangesAsync();

            await _emailNotificationService.SendMemberAddedToTeamNotification(team, member, requests.Count());
            var resultModel = _mapper.Map<TeamMemberResponseModel>(team);

            return resultModel;
        }

        public async Task<TeamMemberResponseModel> RemoveMember(int teamId, int memberId, int modifierId)
        {
            var member = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == memberId);
            if (member == null)
                throw new KeyNotFoundException(string.Format(UserServiceConstants.UserDoesNotExistErrorMessage,memberId));

            var team = await GetTeamAsync(teamId);
            var isUserTeamMember = team.Members.Any(m => m.Id == memberId);
            if (!isUserTeamMember)
                throw new ArgumentException(string.Format(TeamServiceConstants.UserNotMemberOfTeam,memberId,team.Id));

            if (team.TeamLeaderId == memberId)
            {
                team.TeamLeader.RequestsToApprove.Clear();
                team.TeamLeader = null;
            }

            var requests = member.Requests.Where(r => r.Status.State == "Awaiting");

            foreach (var request in requests.Where(x => x.Approvers.Any(a => a.Id == team.TeamLeaderId)))
            {
                request.Approvers.Remove(team.TeamLeader);
                team.TeamLeader.RequestsToApprove.Remove(request);
            }

            team.Members.Remove(member);
            team.LastModifierId = modifierId;
            team.LastModificationDate = DateTime.Now;

            await _dbContext.SaveChangesAsync();

            await _emailNotificationService.SendMemberRemovedFromTeamNotification(team, member);

            var resultModel = new TeamMemberResponseModel
            {
                Team = team.Title,
                Member = member.UserName,
            };

            return resultModel;
        }

        public async Task<Team> GetTeamAsync(int teamId)
        {
            var team = await _dbContext.Teams.FirstOrDefaultAsync(t => t.Id == teamId);
            if (team == null)
                throw new KeyNotFoundException(string.Format(TeamServiceConstants.TeamDoesNotExistsErrorMessage,teamId));

            return team;
        }

        public async Task<IEnumerable<TeamResponseModel>> GetAllTeamsAsync()
        {
            var teams = await _dbContext.Teams.ToListAsync();
            var teamsResponseModels = _mapper.Map<ICollection<TeamResponseModel>>(teams);

            return teamsResponseModels;
        }

        public async Task<TeamMemberResponseModel> PromoteToTeamLeaderAsync(int teamId, int userId, int modifierId)
        {
            var team = await GetTeamAsync(teamId);

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new KeyNotFoundException(string.Format(
                                                        UserServiceConstants.UserDoesNotExistErrorMessage,
                                                        userId));

            if (user.TeamsLed.Any())
                throw new ArgumentException(string.Format(TeamServiceConstants.UserAlreadyTeamLeader, userId));

            var isUserTeamMember = team.Members.Any(m => m.Id == userId);
            if (!isUserTeamMember)
                throw new ArgumentException(string.Format(TeamServiceConstants.UserNotMemberOfTeam, userId, team.Id));

            var awaitingRequestsByTeamMembers = team.Members.SelectMany(m => m.Requests)
                                                .Where(r => r.Status.State == "Awaiting");
            
            foreach (var request in awaitingRequestsByTeamMembers)
            {
                request.Approvers.Remove(team.TeamLeader);
                request.Approvers.Add(user);
            }

            team.TeamLeader = user;
            await _dbContext.SaveChangesAsync();

            var resultModel = new TeamMemberResponseModel
            {
                Team = team.Title,
                Member = user.UserName,
            };

            return resultModel;
        }
    }
}
