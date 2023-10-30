namespace WorkforceManager.Services.MappingConfiguration
{
    using System;
    using System.Linq;
    using AutoMapper;
    using WorkforceManager.Data.Entities;
    using WorkforceManager.Models.Requests.UserRequestModels;
    using WorkforceManager.Models.Responses.UserResponseModels;

    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            this.CreateMap<UserCreateRequestModel, User>()
                .ForMember(e => e.CreationDate, m => m.MapFrom(d => DateTime.Now))
                .ForMember(e => e.LastModificationDate, m => m.MapFrom(d => DateTime.Now));
            this.CreateMap<UserEditRequestModel, User>()
                .ForMember(e => e.CreationDate, m => m.MapFrom(d => DateTime.Now))
                .ForMember(e => e.LastModificationDate, m => m.MapFrom(d => DateTime.Now));
            this.CreateMap<User, UserResponseModel>()
                .ForMember(m => m.MemberOf, e => e.MapFrom(u => string.Join(", ", u.Teams.Select(t => t.Title))))
                .ForMember(m => m.Email, e => e.MapFrom(u => u.Email ?? "none"))
                .ForMember(m => m.CreatorId, e => e.MapFrom(u => u.CreatorId == 0 ? "none" : u.CreatorId.ToString()))
                .ForMember(m => m.LastModifierId, e => e.MapFrom(u => u.LastModifierId == 0 ? "none" : u.LastModifierId.ToString()));
            this.CreateMap<User, CreatedUserResponseModel>()
                .ForMember(m => m.AddedToTeam, e => e.MapFrom(u => u.Teams.Any() ? u.Teams.First().Title : "none"))
                .ForMember(m => m.Email, e => e.MapFrom(u => u.Email ?? "none"));
            this.CreateMap<User, DeletedUserResponseModel>()
            .ForMember(m => m.Teams, e => e.MapFrom(u => string.Join(", ", u.Teams.Select(t => t.Title))))
            .ForMember(m => m.Email, e => e.MapFrom(u => u.Email ?? "none"))
            .ForMember(m => m.LeaderOfTeam, e => e.MapFrom(u => string.Join(", ", u.TeamsLed.Select(t => t.Title))));
        }
    }
}
