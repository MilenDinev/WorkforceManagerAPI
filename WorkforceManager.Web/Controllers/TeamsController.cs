namespace WorkforceManager.Web.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.AspNetCore.Authorization;
    using Services.Interfaces;
    using Models.Requests.TeamRequestModels;
    using Models.Responses.TeamResponseModels;
    using AutoMapper;
    using Microsoft.AspNetCore.Http;
    using WorkforceManager.Models;

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class TeamsController : ApplicationBaseController
    {
        private readonly ITeamService _teamService;
        private readonly IMapper _mapper;

        public TeamsController(IUserService userService, ITeamService teamService, IMapper mapper) : base(userService)
        {
            _teamService = teamService;
            _mapper = mapper;
        }

        /// <summary>
        /// List all teams
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TeamResponseModel>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(object))]
        [HttpGet("List/All")]
        public async Task<ActionResult<IEnumerable<TeamResponseModel>>> GetAll()
        {
            var allTeams = await _teamService.GetAllTeamsAsync();
            return allTeams.ToList();
        }

        /// <summary>
        /// Get team by ID
        /// </summary>
        /// <param name="teamId">ID of the team</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TeamResponseModel))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorMessageResponse))]
        [HttpGet("Get/{teamId}/")]
        public async Task<ActionResult<TeamResponseModel>> GetTeamById(int teamId)
        {
            return _mapper.Map<TeamResponseModel>(await _teamService.GetTeamAsync(teamId));
        }

        /// <summary>
        /// Create team by given title and description. Team leader can me instantly
        /// assigned by providing valid user ID
        /// </summary>
        /// <param name="teamRequestModel"></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CreatedTeamResponseModel))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorMessageResponse))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorMessageResponse))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorMessageResponse))]
        [HttpPost("Create/")]
        public async Task<ActionResult> Post(TeamCreateRequestModel teamRequestModel)
        {
            await GetCurrentUserAsync();
            var result = await _teamService.CreateAsync(teamRequestModel, CurrentUser.Id);
            return CreatedAtAction(nameof(GetAll), "Teams", result);
        }

        /// <summary>
        /// Add member to a team
        /// </summary>
        /// <param name="teamId">ID of the team</param>
        /// <param name="userId">ID of the user</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TeamMemberResponseModel))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorMessageResponse))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorMessageResponse))]
        [HttpPut("AddMember/Team/{teamId}/User/{userId}")]
        public async Task<ActionResult> Put(int teamId, int userId)
        {
            await GetCurrentUserAsync();
            var result = await _teamService.AddMember(teamId, userId, CurrentUser.Id);
            return Ok(result);
        }

        /// <summary>
        /// Remove member from a team
        /// </summary>
        /// <param name="teamId">ID of the team</param>
        /// <param name="userId">ID of the member</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TeamMemberResponseModel))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorMessageResponse))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorMessageResponse))]
        [HttpPut("RemoveMember/Team/{teamId}/User/{userId}")]
        public async Task<ActionResult> Remove(int teamId, int userId)
        {
            await GetCurrentUserAsync();
            var result = await _teamService.RemoveMember(teamId, userId, CurrentUser.Id);
            return Ok(result);
        }

        /// <summary>
        /// Edit exisiting team
        /// </summary>
        /// <param name="teamRequestModel"></param>
        /// <param name="teamId">ID of the team</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TeamResponseModel))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorMessageResponse))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorMessageResponse))]
        [HttpPut("Edit/{teamId}")]
        public async Task<ActionResult<TeamResponseModel>> Put(TeamEditRequestModel teamRequestModel, int teamId)
        {
            await GetCurrentUserAsync();
            var teamGeneralResponseModel = await _teamService.EditAsync(teamRequestModel, teamId, CurrentUser.Id);

            return teamGeneralResponseModel;
        }

        /// <summary>
        /// Delete team
        /// </summary>
        /// <param name="teamId">ID of the team</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TeamResponseModel))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorMessageResponse))]
        [HttpDelete("Delete/{teamId}")]
        public async Task<ActionResult> Delete(int teamId)
        {

            var teamDeletedResult = await _teamService.DeleteAsync(teamId);

            return Ok(teamDeletedResult);
        }

        /// <summary>
        /// Promote team member to team leader
        /// </summary>
        /// <param name="teamId">ID of the team</param>
        /// <param name="userId">ID of the user</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TeamMemberResponseModel))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorMessageResponse))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorMessageResponse))]
        [HttpPut("Promote/Team/{teamId}/User/{userId}")]
        public async Task<ActionResult> Promote(int teamId, int userId)
        {
            await GetCurrentUserAsync();
            var result = await _teamService.PromoteToTeamLeaderAsync(teamId, userId, CurrentUser.Id);
            return Ok(result);
        }
    }
}