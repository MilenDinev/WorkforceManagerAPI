namespace WorkforceManager.Services.MappingConfiguration
{
    using System;
    using System.Linq;
    using AutoMapper;
    using WorkforceManager.Data.Entities;
    using WorkforceManager.Models.Requests.RequestRequestModels;
    using WorkforceManager.Models.Responses.RequestsResponseModels;

    public class RequestMappingProfile : Profile
    {
        public RequestMappingProfile()
        {
            this.CreateMap<RequestCreateRequestModel, TimeOffRequest>()
                .ForMember(e => e.Type, opt => opt.Ignore())
                .ForMember(e => e.CreationDate, m => m.MapFrom(d => DateTime.Now))
                .ForMember(e => e.LastModificationDate, m => m.MapFrom(d => DateTime.Now));

            this.CreateMap<TimeOffRequest, RequestResponseModel>()
                .ForMember(m => m.Status, e => e.MapFrom(t => t.Status.State))
                .ForMember(m => m.Type, e => e.MapFrom(t => t.Type.Type))
                .ForMember(m => m.CreatorId, e => e.MapFrom(t => t.CreatorId == 0 ? "none" : t.CreatorId.ToString()))
                .ForMember(m => m.CreatorId, e => e.MapFrom(t => t.RequesterId == 0 ? "none" : t.RequesterId.ToString()))
                .ForMember(m => m.LastModifierId, e => e.MapFrom(t => t.LastModifierId == 0 ? "none" : t.LastModifierId.ToString()))
                .ForMember(m => m.StartDate, e => e.MapFrom(t => t.StartDate.ToString("D")))
                .ForMember(m => m.EndDate, e => e.MapFrom(t => t.EndDate.ToString("D")));

            this.CreateMap<TimeOffRequest, SubmitRequestResponseModel>()
                .ForMember(m => m.RequestId, e => e.MapFrom(t => t.Id))
                .ForMember(m => m.RequesterFirstName, e => e.MapFrom(t => t.Requester.FirstName))
                .ForMember(m => m.RequesterLastName, e => e.MapFrom(t => t.Requester.LastName))
                .ForMember(m => m.RequestType, e => e.MapFrom(t => t.Type.Type))
                .ForMember(m => m.RequestStatusState, e => e.MapFrom(t => t.Status.State))
                .ForMember(m => m.TotalDaysRequested, e => e.MapFrom(t => (t.EndDate.Date - t.StartDate.Date).Days))
                .ForMember(m => m.StartDate, e => e.MapFrom(t => t.StartDate.ToString("D")))
                .ForMember(m => m.EndDate, e => e.MapFrom(t => t.EndDate.ToString("D")))
                .ForMember(m => m.WorkingDays, e => e.MapFrom( t => HolidaysService.GetWorkingDays(t.StartDate, t.EndDate).GetAwaiter().GetResult()));

            this.CreateMap<TimeOffRequest, ApprovedRequestResponseModel>()
                .ForMember(m => m.RequestId, e => e.MapFrom(t => t.Id))
                .ForMember(m => m.RequesterFirstName, e => e.MapFrom(t => t.Requester.FirstName))
                .ForMember(m => m.RequesterLastName, e => e.MapFrom(t => t.Requester.LastName))
                .ForMember(m => m.RequestType, e => e.MapFrom(t => t.Type.Type))
                .ForMember(m => m.RequestStatusState, e => e.MapFrom(t => t.Status.State))
                .ForMember(m => m.StartDate, m => m.MapFrom(t => t.StartDate.ToString("D")))
                .ForMember(m => m.EndDate, m => m.MapFrom(t => t.EndDate.ToString("D")))
                .ForMember(m => m.WorkingDays, e => e.MapFrom(t => HolidaysService.GetWorkingDays(t.StartDate, t.EndDate).GetAwaiter().GetResult()));

            this.CreateMap<TimeOffRequest, RejectedRequestResponseModel>()
                .ForMember(m => m.RequestId, e => e.MapFrom(t => t.Id))
                .ForMember(m => m.RequesterFirstName, e => e.MapFrom(t => t.Requester.FirstName))
                .ForMember(m => m.RequesterLastName, e => e.MapFrom(t => t.Requester.LastName))
                .ForMember(m => m.RequestType, e => e.MapFrom(t => t.Type.Type))
                .ForMember(m => m.RequestStatusState, e => e.MapFrom(t => t.Status.State));

            this.CreateMap<TimeOffRequest, RequestDetailedResponseModel>()
                .ForMember(m => m.Status, e => e.MapFrom(t => t.Status.State))
                .ForMember(m => m.Type, e => e.MapFrom(t => t.Type.Type))
                .ForMember(m => m.CreatorId, e => e.MapFrom(t => t.CreatorId == 0 ? "none" : t.CreatorId.ToString()))
                .ForMember(m => m.LastModifierId, e => e.MapFrom(t => t.LastModifierId == 0 ? "none" : t.LastModifierId.ToString()))
                .ForMember(m => m.StartDate, e => e.MapFrom(t => t.StartDate.ToString("D")))
                .ForMember(m => m.EndDate, e => e.MapFrom(t => t.EndDate.ToString("D")))
                .ForMember(m => m.Approvers, e => e.MapFrom(t => t.Approvers.Select(t => t.UserName)));
        }

    }
}
