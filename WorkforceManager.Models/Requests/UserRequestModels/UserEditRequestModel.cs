namespace WorkforceManager.Models.Requests.UserRequestModels
{
    using System.ComponentModel.DataAnnotations;

    public class UserEditRequestModel
    {
        [Required]
        [MaxLength(15, ErrorMessage = "Username is required and must be less than 15 symbols!")]
        public string UserName { get; set; }

        [EmailAddress(ErrorMessage = "Please provide valid email address!")]
        public string Email { get; set; }
        [Required]
        [MinLength(6, ErrorMessage = "Password is required and must be at least 6 symbols long!")]
        public string Password { get; set; }
         
    }
}
