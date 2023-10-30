namespace WorkforceManager.Data.Seeders
{
    using Microsoft.AspNetCore.Identity;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using WorkforceManager.Data.Entities;

    [ExcludeFromCodeCoverage]
    public class SamplesSeeder
    {
        public static async Task SeedSample(WorkforceManagerDbContext context, UserManager<User> userManager)
        {

            var team1Leader = new User
            {
                UserName = "leader1",
                FirstName = "Leader1",
                LastName = "Team 1",
                Email = "teamleader1@test.com",
                NormalizedEmail = "teamleader1@test.com".ToUpper(),
                EmailConfirmed = true,
                NormalizedUserName = "leader1".ToUpper(),
                SecurityStamp = Guid.NewGuid().ToString("D"),
                CreatorId = 2,
                CreationDate = DateTime.Now,
                LastModifierId = 2,
                LastModificationDate = DateTime.Now
            };

            await userManager.CreateAsync(team1Leader, "teamleadpass");
            await userManager.AddToRoleAsync(team1Leader, "regular");

            var team2Leader = new User
            {
                UserName = "leader2",
                FirstName = "Leader2",
                LastName = "Team 2",
                Email = "teamleader2@test.com",
                NormalizedEmail = "teamleader2@test.com".ToUpper(),
                EmailConfirmed = true,
                NormalizedUserName = "leader2".ToUpper(),
                SecurityStamp = Guid.NewGuid().ToString("D"),
                CreatorId = 2,
                CreationDate = DateTime.Now,
                LastModifierId = 2,
                LastModificationDate = DateTime.Now
            };

            await userManager.CreateAsync(team2Leader, "teamleadpass");
            await userManager.AddToRoleAsync(team2Leader, "regular");


            var Leader3 = new User
            {
                UserName = "leader3",
                FirstName = "Leader3",
                LastName = "Team 1",
                Email = "teamleader3@test.com",
                NormalizedEmail = "teamleader3@test.com".ToUpper(),
                EmailConfirmed = true,
                NormalizedUserName = "leader3".ToUpper(),
                SecurityStamp = Guid.NewGuid().ToString("D"),
                CreatorId = 1,
                CreationDate = DateTime.Now,
                LastModifierId = 1,
                LastModificationDate = DateTime.Now
            };

            await userManager.CreateAsync(Leader3, "teamleadpass");
            await userManager.AddToRoleAsync(Leader3, "regular");


            var user1= new User
            {
                UserName = "user",
                FirstName = "user 1",
                LastName = "No team 1",
                Email = "noteamuser@test.com",
                NormalizedEmail = "noteamuser@test.com".ToUpper(),
                EmailConfirmed = true,
                NormalizedUserName = "team1Regular2".ToUpper(),
                SecurityStamp = Guid.NewGuid().ToString("D"),
                CreatorId = 1,
                CreationDate = DateTime.Now,
                LastModifierId = 1,
                LastModificationDate = DateTime.Now
            };

            await userManager.CreateAsync(user1, "123456");
            await userManager.AddToRoleAsync(user1, "regular");


            var team1 = new Team()
            {
                Title = "Team1",
                Description = "This is testteam 1",
                TeamLeaderId = team1Leader.Id
            };

            var team2 = new Team()
            {
                Title = "Team2",
                Description = "This is testteam 2",
                TeamLeaderId = team2Leader.Id
            };


            await context.Teams.AddRangeAsync(team1, team2);

            team1Leader.TeamsLed.Add(team1);
            team1.Members.Add(team1Leader);
            team1.Members.Add(user1);

            team2Leader.TeamsLed.Add(team2);
            team2.Members.Add(team2Leader);
            team2.Members.Add(user1);

            await context.SaveChangesAsync();
        }
    }
}

