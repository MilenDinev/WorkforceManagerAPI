namespace WorkforceManager.Web
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Data;
    using Data.Seeders;
    using Data.Entities;
    using Models;
    using Middleware;
    using System.Diagnostics.CodeAnalysis;
    using WorkforceManager.Data.Constants;
    using WorkforceManager.Web.Extensions;

    [ExcludeFromCodeCoverage]
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<WorkforceManagerDbContext>(options => options.UseSqlServer(Configuration["ConnectionStrings:MDinevConnectionString"]));    
            services.Configure<SmtpSettings>(Configuration.GetSection("SmtpSettings"));
            services.AddSwaggerConfig();

            services.AddIdentityConfig();

            services.AddServiceConfig();

            services.AddAuthorizationConfig();
            
            services.AddControllers();

            var builder = services.IdentityServerBuilderConfig();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            DatabaseSeeder.SeedAsync(app.ApplicationServices).GetAwaiter().GetResult();

            app.UseIdentityServer();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint(SwaggerConstants.EndpointUrl, SwaggerConstants.EndpointName));
            }

            app.UseMiddleware<ErrorHandler>();

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
