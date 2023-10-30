namespace WorkforceManager.Data.Entities
{
    using System.Collections.Generic;

    public class Team : Entity
    {
        public Team()
        {
            this.Members = new HashSet<User>(); 
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public int? TeamLeaderId { get; set; }
        public virtual User TeamLeader { get; set; }
        public virtual ICollection<User> Members { get; set; }
    }
}
