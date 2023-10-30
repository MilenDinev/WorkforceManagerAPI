namespace WorkforceManager.Data.Entities
{
    public class ApproverRequest
    {
        public int ApproverId { get; set; }
        public int RequestId { get; set; }
        public bool IsProcessed { get; set; }

    }
}
