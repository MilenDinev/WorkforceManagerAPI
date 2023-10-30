namespace WorkforceManager.Web.Authorization
{
    using Microsoft.AspNetCore.Authorization;

    public class MustBeRequestCreatorOrAdminRequirements : IAuthorizationRequirement
    {
        public MustBeRequestCreatorOrAdminRequirements()
        {

        }
    }
}
