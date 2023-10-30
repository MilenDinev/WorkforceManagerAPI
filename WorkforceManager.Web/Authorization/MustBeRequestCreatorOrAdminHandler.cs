namespace WorkforceManager.Web.Authorization
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using System;
    using System.Threading.Tasks;
    using WorkforceManager.Services.Interfaces;

    public class MustBeRequestCreatorOrAdminHandler : AuthorizationHandler<MustBeRequestCreatorOrAdminRequirements>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRequestService _requestService;
        private readonly IUserService _userService;
        public MustBeRequestCreatorOrAdminHandler(IHttpContextAccessor httpContextAccessor, IRequestService requestService, IUserService userService)
        {
            _httpContextAccessor = httpContextAccessor;
            _requestService = requestService;
            _userService = userService;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MustBeRequestCreatorOrAdminRequirements requirement)
        {
            try
            {
                var requestId = Convert.ToInt32(_httpContextAccessor.HttpContext.GetRouteValue("requestId"));

                if (requestId == 0)
                {
                    context.Fail();
                    return Task.CompletedTask;
                }

                var currentUser = _userService.GetCurrentUserAsync(context.User).GetAwaiter().GetResult();
                var currentUserId = currentUser.Id;

                var isTheUserAdmin =  _userService.IsUserAdminAsync(currentUser).GetAwaiter().GetResult();
                var isTheUserRequestCreator = _requestService.IsTheUserRequestCreatorAsync(requestId, currentUserId).GetAwaiter().GetResult();

                if (!isTheUserRequestCreator && !isTheUserAdmin)
                {
                    context.Fail();
                    return Task.CompletedTask;
                }

                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            catch (NullReferenceException)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            catch (UnauthorizedAccessException)
            {
                context.Fail();
                return Task.CompletedTask;
            }
        }
        
    }
}
