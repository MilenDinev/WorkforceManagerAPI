namespace WorkforceManager.Data.Entities
{
    using System.Collections.Generic;

    public class TimeOffRequestType
    {
        public TimeOffRequestType()
        {
            this.Requests = new HashSet<TimeOffRequest>();
        }
        public int Id { get; set; }
        public string Type { get; set; }
        public virtual ICollection<TimeOffRequest> Requests { get; set; }

        public override string ToString()
        {
            return new string(Type); 
        }
    }
}
