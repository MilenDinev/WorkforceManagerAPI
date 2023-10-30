namespace WorkforceManager.Web.Extensions
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;
    using System.Reflection;
    using WorkforceManager.Services;
    using WorkforceManager.Services.Interfaces;
    using WorkforceManager.Services.Managers;

    public static class ServicesConfigurationExtensions
    {
        public static void AddServiceConfig(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.Load("WorkforceManager.Services"));
            services.AddSingleton<IEmailNotificationService, EmailNotificationService>();
            services.AddTransient<IUserManager, AppUserManager>();
            services.AddTransient<RoleManager<IdentityRole<int>>>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<ITeamService, TeamService>();
            services.AddTransient<IRequestService, RequestService>();
        }
    }
}
