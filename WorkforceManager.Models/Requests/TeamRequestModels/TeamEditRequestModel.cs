namespace WorkforceManager.Models.Requests.TeamRequestModels
{
    using System.ComponentModel.DataAnnotations;

    public class TeamEditRequestModel
    {
        [Required]
        [MaxLength(25, ErrorMessage = "Team name is required and must be less than 25 symbols!")]
        [MinLength(1, ErrorMessage = "Team name is required and must be more than 1 symbols!")]
        public string Title { get; set; }
        public string Description { get; set; }
        public string TeamLeaderId { get; set; }
    }
}
