namespace WorkforceManager.Web.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Models.Requests;
    using Models.Responses;
    using Services.Interfaces;
    using Web.Authentication;

    [Route("api/[controller]")]
    [ApiController]
    public class GetTokenController : ApplicationBaseController
    {
        public GetTokenController(IUserService userService) : base(userService)
        {
        }

        /// <summary>
        /// Generate token given valid credentials
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<TokenResponseModel>> Login(UserLoginRequestModel user)
        {
            using var httpClientHandler = new HttpClientHandler();
            using var client = new HttpClient(httpClientHandler);
            var token = await _userService.GetElibilityTokenAsync(client, user.UsernameOrEmail, user.Password);
            return token;
        }
    }
}
