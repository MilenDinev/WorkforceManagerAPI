namespace WorkforceManager.Models.Requests.UserRequestModels
{
    using System.ComponentModel.DataAnnotations;

    public class UserCreateRequestModel
    {
        [Required(ErrorMessage = "Username is required!")]
        [MaxLength(15, ErrorMessage = "Username must be less than 15 symbols!")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "First name is required!")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Last name is required!")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required!")]
        [EmailAddress(ErrorMessage = "Please provide valid email address!")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required!")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Role is required!")]
        [RegularExpression("(^(?i)admin$|^regular$|^1$|^2$)",
            ErrorMessage = "Typed role is not valid! Please type between \'1\' or \'admin\' for admin role, \'2\' or \'regular\' for regular role.")]
        public string Role { get; set;}
        public string TeamId { get; set; }
    }
}