namespace WorkforceManager.Web
{
    using IdentityServer4.Models;
    using IdentityServer4.Validation;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using WorkforceManager.Data.Constants;
    using WorkforceManager.Data.Entities;
    using WorkforceManager.Services.Interfaces;

    public class PasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly IUserManager _userManager;

        public PasswordValidator(IUserManager userManager)
        {
            _userManager = userManager;
        }

        //This method validates the user credentials and if successful then IdentiryServer will build a token from the context.Result object
        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var user = await _userManager.FindByNameAsync(context.UserName);

            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(context.UserName);
            }

            if (user != null)
            {
                var authResult = await _userManager.ValidateUserCredentials(context.UserName, context.Password);

                if (authResult)
                {
                    var roles = await _userManager.GetUserRolesAsync(user);

                    var claims = new List<Claim>();

                    claims.Add(new Claim(ClaimTypes.Name, user.UserName));
                    claims.Add(new Claim(ClaimTypes.Email, user.Email));

                    foreach (var role in roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }

                    context.Result = new GrantValidationResult(subject: user.Id.ToString(), authenticationMethod: "password", claims: claims);
                }
                else
                {
                    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, 
                                                                PasswordValidatorConstants.InvalidCredentialsErrorDescription);
                }

                return;
            }

            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant,
                                                        PasswordValidatorConstants.InvalidCredentialsErrorDescription);
        }
    }
}

