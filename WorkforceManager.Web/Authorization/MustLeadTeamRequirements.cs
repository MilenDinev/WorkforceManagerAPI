namespace WorkforceManager.Web.Authorization
{
    using Microsoft.AspNetCore.Authorization;

    public class MustLeadTeamRequirements : IAuthorizationRequirement
    {
        public MustLeadTeamRequirements()
        {

        }
    }
}
