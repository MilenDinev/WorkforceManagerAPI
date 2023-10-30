namespace WorkforceManager.Models.Responses.UserResponseModels
{
    public class DeletedUserResponseModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Teams { get; set; }
        public string LeaderOfTeam { get; set; }
    }
}
