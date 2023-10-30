namespace WorkforceManager.Web.Authorization
{
    using Microsoft.AspNetCore.Authorization;
    using System;
    using System.Threading.Tasks;
    using WorkforceManager.Services.Interfaces;

    public class MustLeadTeamHandler : AuthorizationHandler<MustLeadTeamRequirements>
    {
        private readonly IUserService _userService;

        public MustLeadTeamHandler(IUserService userService)
        {
            _userService = userService;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MustLeadTeamRequirements requirement)
        {
            try
            {
                var currentUser = _userService.GetCurrentUserAsync(context.User).GetAwaiter().GetResult();
                var currentUserId = currentUser.Id;

                var isTheUserTeamLead = _userService.IsTheUserTeamLeadAsync(currentUserId).GetAwaiter().GetResult();

                if (!isTheUserTeamLead)
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
