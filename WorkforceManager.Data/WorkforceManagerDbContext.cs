namespace WorkforceManager.Data
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using WorkforceManager.Data.Entities;

    public class WorkforceManagerDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public virtual DbSet<Team> Teams { get; set; }
        public virtual DbSet<TimeOffRequest> TimeOffRequests { get; set; }
        public virtual DbSet<TimeOffRequestStatus> Statuses { get; set; }
        public virtual DbSet<TimeOffRequestType> RequestTypes { get; set; }

        public WorkforceManagerDbContext(DbContextOptions<WorkforceManagerDbContext> options)
            : base(options)
        { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseLazyLoadingProxies();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var assemblyWithConfigurations = GetType().Assembly;
            modelBuilder.ApplyConfigurationsFromAssembly(assemblyWithConfigurations);
            base.OnModelCreating(modelBuilder);
        }

    }
}
