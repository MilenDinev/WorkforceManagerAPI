
namespace WorkforceManager.Web.Extensions
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;
    using WorkforceManager.Data;
    using WorkforceManager.Data.Entities;

    public static class IdentityConfigurationExtension
    {
        public static void AddIdentityConfig(this IServiceCollection services)
        {
            services.AddIdentityCore<User>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            })
            .AddRoles<IdentityRole<int>>()
            .AddEntityFrameworkStores<WorkforceManagerDbContext>();
        }

        public static IIdentityServerBuilder IdentityServerBuilderConfig(this IServiceCollection services)
        {
            var builder = services.AddIdentityServer((options) =>
                            {
                                options.EmitStaticAudienceClaim = true;
                            })
                                                  .AddInMemoryApiScopes(IdentityConfig.ApiScopes)
                                                  .AddInMemoryClients(IdentityConfig.Clients);
                            builder.AddDeveloperSigningCredential();
                            builder.AddResourceOwnerValidator<PasswordValidator>();

            return builder;
        }

    }
}
