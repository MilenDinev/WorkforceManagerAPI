namespace WorkforceManager.Web.Authorization
{
    using Microsoft.AspNetCore.Authorization;

    public class MustBeRequestApproverRequirements : IAuthorizationRequirement
    {
        public MustBeRequestApproverRequirements()
        {

        }
    }
}