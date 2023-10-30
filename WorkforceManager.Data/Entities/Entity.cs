namespace WorkforceManager.Data.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Entity
    {
        public int Id { get; set; }

        public int CreatorId { get; set; }
        public User Creator { get; set; }
        public DateTime CreationDate { get; set; }

        public int LastModifierId { get; set; }
        public DateTime LastModificationDate { get; set; }
    }
}
