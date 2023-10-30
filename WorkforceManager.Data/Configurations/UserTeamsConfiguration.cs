namespace WorkforceManager.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using System.Collections.Generic;
    using WorkforceManager.Data.Entities;

    public class UserTeamsConfiguration : IEntityTypeConfiguration<Team>
    {

        public void Configure(EntityTypeBuilder<Team> builder)
        {
            builder.HasOne(t => t.TeamLeader)
            .WithMany(m => m.TeamsLed)
            .HasForeignKey(t => t.TeamLeaderId)
            .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(t => t.Members)
                .WithMany(m => m.Teams)
                .UsingEntity<Dictionary<string, object>>("UserTeams",
                x => x.HasOne<User>().WithMany().HasForeignKey("UserId")
                      .OnDelete(DeleteBehavior.Cascade),
                x => x.HasOne<Team>().WithMany().HasForeignKey("TeamId")
                      .OnDelete(DeleteBehavior.Restrict));

            builder.Ignore(tr => tr.Creator);
        }
    }
}
