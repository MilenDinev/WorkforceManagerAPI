namespace WorkforceManager.Data.Seeders
{
    using Microsoft.AspNetCore.Identity;
    using System;
    using System.Threading.Tasks;
    using WorkforceManager.Data.Entities;

    public class UsersSeeder
    {
        public static async Task SeedUsers(UserManager<User> userManager)
        {
            var adminUser = new User()
            {
                UserName = "admin",
                FirstName = "Initial admin user",
                LastName = "Initial admin user",
                Email = "TheMostImportanAdminEmail@yahoo.com",
                NormalizedEmail = "TheMostImportanAdminEmail@yahoo.com".ToUpper(),
                EmailConfirmed = true,
                NormalizedUserName = "admin".ToUpper(),
                SecurityStamp = Guid.NewGuid().ToString("D"),
                CreatorId = 1,
                CreationDate = DateTime.Now,
                LastModifierId = 1,
                LastModificationDate = DateTime.Now
            };

            await userManager.CreateAsync(adminUser, "adminpass");
            await userManager.AddToRoleAsync(adminUser, "admin");
        }
    }
}
