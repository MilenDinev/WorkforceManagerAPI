using System.Collections.Generic;

namespace WorkforceManager.Models.Responses.RequestsResponseModels
{
    public class RequestDetailedResponseModel
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string CreatorId { get; set; }
        public string CreationDate { get; set; }
        public string LastModifierId { get; set; }
        public string LastModificationDate { get; set; }
        public HashSet<string> Approvers {get;set;}
    }
}
