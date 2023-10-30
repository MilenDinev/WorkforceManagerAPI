


namespace WorkforceManager.Web.Extensions
{
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.DependencyInjection;
    using WorkforceManager.Web.Authorization;

    public static class AuthorizationConfigurationExtension
    {
        public static void AddAuthorizationConfig(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddTransient<IAuthorizationHandler, MustBeRequestCreatorOrAdminHandler>();
            services.AddTransient<IAuthorizationHandler, MustLeadTeamHandler>();
            services.AddTransient<IAuthorizationHandler, MustBeRequestApproverHandler>();


            services.AddAuthorization(authorizationOptions =>
            {
                authorizationOptions.AddPolicy(
                    "MustBeRequestCreatorOrAdmin",
                    policyBuilder =>
                    {
                        policyBuilder.RequireAuthenticatedUser();
                        policyBuilder.AddRequirements(new MustBeRequestCreatorOrAdminRequirements());
                    });

                authorizationOptions.AddPolicy(
                    "MustBeTeamLead",
                    policyBuilder =>
                    {
                        policyBuilder.RequireAuthenticatedUser();
                        policyBuilder.AddRequirements(new MustLeadTeamRequirements());
                    });

                authorizationOptions.AddPolicy(
                    "MustBeRequestApprover",
                    policyBuilder =>
                    {
                        policyBuilder.RequireAuthenticatedUser();
                        policyBuilder.AddRequirements(new MustBeRequestApproverRequirements());
                    });
            })
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.Authority = "https://localhost:5002";
                    options.Audience = "https://localhost:5002/resources";
                });
        }
    }
}
