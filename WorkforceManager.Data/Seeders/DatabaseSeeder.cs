﻿namespace WorkforceManager.Data.Seeders
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using WorkforceManager.Data.Entities;

    [ExcludeFromCodeCoverage]
    public class DatabaseSeeder
    {
        public async static Task SeedAsync(IServiceProvider applicationServices)
        {
            using (IServiceScope serviceScope = applicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<WorkforceManagerDbContext>();
                var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
                var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<User>>();

                await context.Database.MigrateAsync();

                if (!context.Users.Any())
                {
                    await RolesSeeder.SeedRoles(roleManager);
                    await UsersSeeder.SeedUsers(userManager);
                    await SamplesSeeder.SeedSample(context, userManager);
                }
            }
        }
    }
}
