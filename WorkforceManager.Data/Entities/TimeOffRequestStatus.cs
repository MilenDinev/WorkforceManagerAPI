namespace WorkforceManager.Data.Entities
{
    using System.Collections.Generic;

    public class TimeOffRequestStatus
    {
        public TimeOffRequestStatus()
        {
            this.Requests = new HashSet<TimeOffRequest>();
        }

        public int Id { get; set; }
        public string State { get; set; }
        public virtual ICollection<TimeOffRequest> Requests { get; set; }
    }
}
