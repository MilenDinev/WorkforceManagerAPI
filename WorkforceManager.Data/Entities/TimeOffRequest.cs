namespace WorkforceManager.Data.Entities
{
    using System;
    using System.Collections.Generic;

    public class TimeOffRequest : Entity
    {
        public TimeOffRequest()
        {
            this.Approvers = new HashSet<User>();
        }

        public int RequesterId { get; set; }
        public virtual User Requester { get; set; }

        public int TypeId { get; set; }
        public virtual TimeOffRequestType Type { get; set; }  
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int StatusId { get; set; }
        public virtual TimeOffRequestStatus Status { get; set; }
        public virtual ICollection<User> Approvers { get; set; }
    }

}
