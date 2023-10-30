namespace WorkforceManager.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using WorkforceManager.Data.Entities;

    public class UserRequestsConfiguration : IEntityTypeConfiguration<TimeOffRequest>
    {
        public void Configure(EntityTypeBuilder<TimeOffRequest> builder)
        {
           builder.HasMany(tr => tr.Approvers)
            .WithMany(u => u.RequestsToApprove)
            .UsingEntity<ApproverRequest>
            (tu => tu.HasOne<User>().WithMany().HasForeignKey("ApproverId")
            .OnDelete(DeleteBehavior.Restrict),
            tu => tu.HasOne<TimeOffRequest>().WithMany().HasForeignKey("RequestId")
            .OnDelete(DeleteBehavior.Cascade));

            builder.HasOne(r => r.Requester)
            .WithMany(u => u.Requests)
            .HasForeignKey(r => r.RequesterId);

            builder.Ignore(tr => tr.Creator);
        }
    }
}
