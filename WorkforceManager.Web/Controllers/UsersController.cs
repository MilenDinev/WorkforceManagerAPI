namespace WorkforceManager.Web.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.AspNetCore.Authorization;
    using Services.Interfaces;
    using Models.Responses.UserResponseModels;
    using Models.Requests.UserRequestModels;
    using Microsoft.AspNetCore.Http;
    using WorkforceManager.Models;

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class UsersController : ApplicationBaseController
    {
        public UsersController(IUserService userService) : base(userService)
        {
        }

        /// <summary>
        /// List all users
        /// </summary>
        /// <returns>List of all users</returns>
        /// <response code = "200">Returns list of all users</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponseModel))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(object))]
        [HttpGet("List/")]
        public async Task<ActionResult<IEnumerable<UserResponseModel>>> Get()
        {
            var allUsers = await _userService.GetAllUsersAsync();
            return allUsers.ToList();
        }

        /// <summary>
        /// Create a user, providing username, first name, last name, password, email address,
        /// role. Can be instantly added to a team by providing a valid team ID.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CreatedUserResponseModel))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorMessageResponse))]
        [HttpPost("Create/")]
        public async Task<ActionResult> Post(UserCreateRequestModel user)
        {
            if (ModelState.IsValid)
            {
                await GetCurrentUserAsync();
                var createdUser = await _userService.CreateAsync(user, CurrentUser.Id);
                return CreatedAtAction(nameof(Get), "Users", new { id = createdUser.Id }, createdUser);
            }

            return BadRequest();
        }

        /// <summary>
        /// Edit already existing user
        /// </summary>
        /// <param name="user">New data</param>
        /// <param name="userId">ID of the user</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponseModel))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorMessageResponse))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorMessageResponse))]
        [HttpPut("Edit/{userId}")]
        public async Task<ActionResult<UserResponseModel>> Put(UserEditRequestModel user, int userId)
        {
            await GetCurrentUserAsync();
            var userGeneralResponseModel = await _userService.EditAsync(user, userId, CurrentUser.Id);

            return userGeneralResponseModel;
        }

        /// <summary>
        /// Delete an user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns></returns>
        /// <response code = "200"></response>
        /// <response code = "404"></response>
        /// <response code = "409"></response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DeletedUserResponseModel))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorMessageResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorMessageResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorMessageResponse))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorMessageResponse))]
        [HttpDelete("Delete/{userId}")]
        public async Task<ActionResult> Delete(int userId)
        {
            var userDeleteResult = await _userService.DeleteAsync(userId);
            return Ok(userDeleteResult);
        }

        /// <summary>
        /// Get an user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns></returns>
        /// <response code = "200"></response>
        /// <response code = "404"></response>
        /// <response code = "409"></response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponseModel))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorMessageResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorMessageResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorMessageResponse))]
        [HttpGet("Get/{userId}")]
        public async Task<ActionResult> GetById(int userId)
        {
            var userResult = await _userService.GetUserByIdAsync(userId);
            return Ok(userResult);
        }
    }
}
