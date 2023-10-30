namespace WorkforceManager.Models.Responses.UserResponseModels
{
    public class UserResponseModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string MemberOf { get; set; }
        public string CreatorId { get; set; }
        public string CreationDate { get; set; }
        public string LastModifierId { get; set; }
        public string LastModificationDate { get; set; }
    }
}
 