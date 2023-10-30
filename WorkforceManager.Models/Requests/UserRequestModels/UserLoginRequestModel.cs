namespace WorkforceManager.Models.Requests
{
    public class UserLoginRequestModel
    {
        public string UsernameOrEmail { get; set; }
        public string Password { get; set; }
    }
}
