namespace WorkforceManager.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using WorkforceManager.Data.Entities;

    public class RequestTypesSeederConfiguration : IEntityTypeConfiguration<TimeOffRequestType>
    {
        public void Configure(EntityTypeBuilder<TimeOffRequestType> modelBuilder)
        {
            modelBuilder.HasData(
                    new TimeOffRequestType
                    {
                        Id = 1,
                        Type = "Paid"
                    });

            modelBuilder.HasData(
                new TimeOffRequestType
                {
                    Id = 2,
                    Type = "Unpaid"
                });

            modelBuilder.HasData(
                new TimeOffRequestType
                {
                    Id = 3,
                    Type = "Sick"
                });
        }
    }
}
