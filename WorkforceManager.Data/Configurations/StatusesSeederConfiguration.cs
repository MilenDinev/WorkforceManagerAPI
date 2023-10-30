namespace WorkforceManager.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using WorkforceManager.Data.Entities;

    public class StatusesSeederConfiguration : IEntityTypeConfiguration<TimeOffRequestStatus>
    {
        public void Configure(EntityTypeBuilder<TimeOffRequestStatus> modelBuilder)
        {
            modelBuilder.HasData(
                     new TimeOffRequestStatus
                     {
                         Id = 1,
                         State = "Created"
                     });

            modelBuilder.HasData(
                new TimeOffRequestStatus
                {
                    Id = 2,
                    State = "Awaiting"
                });

            modelBuilder.HasData(
                new TimeOffRequestStatus
                {
                    Id = 3,
                    State = "Approved"
                });

            modelBuilder.HasData(
                new TimeOffRequestStatus
                {
                    Id = 4,
                    State = "Rejected"
                });
        }

    }
}
