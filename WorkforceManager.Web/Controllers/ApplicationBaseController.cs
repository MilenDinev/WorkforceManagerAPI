namespace WorkforceManager.Web.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Data.Entities;
    using Services.Interfaces;

    public class ApplicationBaseController : ControllerBase
    {
        protected readonly IUserService _userService;

        public ApplicationBaseController(IUserService userService)
        {
            _userService = userService;
        }

        public User CurrentUser { get; set; }

        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult> GetCurrentUserAsync()
        {
            CurrentUser = await _userService.GetCurrentUserAsync(User);

            if (CurrentUser == null)
            {
                throw new UnauthorizedAccessException("Bad credentials, please try again!");
            }
            return Ok();
        }
    }
}
