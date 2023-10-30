namespace WorkforceManager.Data.Entities
{
    using Microsoft.AspNetCore.Identity;
    using System;
    using System.Collections.Generic;

    public class User : IdentityUser<int>
    {
        public User()
        {
            this.TeamsLed = new HashSet<Team>();
            this.Teams = new HashSet<Team>();
            this.RequestsToApprove = new HashSet<TimeOffRequest>();
            this.Requests = new HashSet<TimeOffRequest>();
        }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int CreatorId { get; set; }
        public DateTime CreationDate { get; set; }
        public int LastModifierId { get; set; }
        public DateTime LastModificationDate { get; set; }
        public virtual ICollection<Team> Teams { get; set; }
        public virtual ICollection<Team> TeamsLed { get; set; }
        public virtual ICollection<TimeOffRequest> RequestsToApprove { get; set; }
        public virtual ICollection<TimeOffRequest> Requests { get; set; }
    }
}
