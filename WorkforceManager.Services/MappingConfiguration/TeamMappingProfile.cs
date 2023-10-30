namespace WorkforceManager.Services.MappingConfiguration
{
    using System;
    using System.Linq;
    using AutoMapper;
    using Data.Entities;
    using Models.Requests.TeamRequestModels;
    using Models.Responses.TeamResponseModels;

    public class TeamMappingProfile : Profile
    {
        public TeamMappingProfile()
        {

            this.CreateMap<TeamCreateRequestModel, Team>()
                .ForMember(e => e.CreationDate, m => m.MapFrom(d => DateTime.Now))
                .ForMember(e => e.LastModificationDate, m => m.MapFrom(d => DateTime.Now))
                .ForMember(e => e.TeamLeaderId, m => m.Ignore());
            this.CreateMap<TeamEditRequestModel, Team>()
                .ForMember(e => e.LastModificationDate, m => m.MapFrom(d => DateTime.Now));
            this.CreateMap<TeamEditRequestModel, Team>();
            this.CreateMap<Team, TeamResponseModel>()
                .ForMember(m => m.TeamLeader, e => e.MapFrom(t => t.TeamLeaderId == null ? "none" : t.TeamLeader.UserName))
                .ForMember(m => m.Members, e => e.MapFrom(t => t.Members.Count));
            this.CreateMap<Team, TeamMemberResponseModel>()
                .ForMember(m => m.Team, e => e.MapFrom(t => t.Title))
                .ForMember(m => m.Member, e => e.MapFrom(t => t.Members.Select(x => x.UserName).LastOrDefault()));
            this.CreateMap<Team, CreatedTeamResponseModel>()
                .ForMember(m => m.TeamLeader, e => e.MapFrom(t => t.TeamLeaderId == null ? "none" : t.TeamLeader.UserName));
        }
    }
}
