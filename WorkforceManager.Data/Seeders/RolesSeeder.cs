namespace WorkforceManager.Data.Seeders
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Identity;

    public class RolesSeeder
    {
        public static async Task SeedRoles(RoleManager<IdentityRole<int>> roleManager)
        {
            var adminRole = new IdentityRole<int>()
            {
                Name = "admin",
                NormalizedName = "admin".ToUpper(),
                ConcurrencyStamp = Guid.NewGuid().ToString("D")
            };
            await roleManager.CreateAsync(adminRole);

            var regularRole = new IdentityRole<int>()
            {
                Name = "regular",
                NormalizedName = "regular".ToUpper(),
                ConcurrencyStamp = Guid.NewGuid().ToString("D")
            };
            await roleManager.CreateAsync(regularRole);
        }
    }
    
}
