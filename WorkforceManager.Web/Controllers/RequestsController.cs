namespace WorkforceManager.Web.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.AspNetCore.Authorization;
    using Services.Interfaces;
    using Models.Requests.RequestRequestModels;
    using Models.Responses.RequestsResponseModels;
    using Microsoft.AspNetCore.Http;
    using WorkforceManager.Models;

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RequestsController : ApplicationBaseController
    {
        private readonly IRequestService _requestService;

        public RequestsController(IUserService userService, IRequestService requestService) : base(userService)
        {
            _requestService = requestService;
        }

        /// <summary>
        /// List requests of the current user
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RequestResponseModel>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(object))]
        [HttpGet("List/Requested")]
        public async Task<ActionResult<IEnumerable<RequestResponseModel>>> Get()
        {
            await GetCurrentUserAsync();
            var requests = await _requestService.GetRequestsMadeByUserAsync(CurrentUser.Id);
            return requests.ToList();
        }

        /// <summary>
        /// List requests that await to be processed by the current user - TEAM LEADER
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RequestResponseModel>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(object))]
        [Authorize(Policy = "MustBeTeamLead")]
        [HttpGet("List/Awaiting")]
        public async Task<ActionResult<IEnumerable<RequestResponseModel>>> GetRequestsToBeProcessed()
        {
            await GetCurrentUserAsync();
            var requests = await _requestService.GetRequestsToBeProcessedByApproverAsync(CurrentUser);
            return requests.ToList();
        }

        /// <summary>
        /// List all requests of a given user - ADMIN
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RequestResponseModel>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(object))]
        [Authorize(Roles = "admin")]
        [HttpGet("List/ByUser/{userId}")]
        public async Task<ActionResult<IEnumerable<RequestResponseModel>>> GetByUser(int userId)
        {
            await GetCurrentUserAsync();
            var requests = await _requestService.GetRequestsMadeByUserAsync(userId);
            return requests.ToList();
        }

        /// <summary>
        /// List all requests with a given status - ADMIN
        /// </summary>
        /// <param name="status">Created/Awaiting/Rejected/Approved</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RequestResponseModel>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(object))]
        [Authorize(Roles = "admin")]
        [HttpGet("List/ByStatus/{status}")]
        public async Task<ActionResult<IEnumerable<RequestResponseModel>>> GetByStatus(string status)
        {
            await GetCurrentUserAsync();
            var requests = await _requestService.GetRequestsByStatusAsync(status);
            return requests.ToList();
        }

        /// <summary>
        /// Get request - Requester or Admin
        /// </summary>
        /// <param name="requestId">ID of the request</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RequestDetailedResponseModel))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(object))]
        [Authorize(Policy = "MustBeRequestCreatorOrAdmin")]
        [HttpGet("Get/{requestId}")]
        public async Task<ActionResult<RequestDetailedResponseModel>> GetRequest(int requestId)
        {
            await GetCurrentUserAsync();
            var request = await _requestService.GetRequestByIdDetailedAsync(requestId);
            return request;
        }

        /// <summary>
        /// Create request by given start and end dates, description and a type
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(RequestResponseModel))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorMessageResponse))]
        [HttpPost("Create/")]
        public async Task<ActionResult> Post(RequestCreateRequestModel request)
        {
            if (ModelState.IsValid)
            {
                await GetCurrentUserAsync();
                var createResult = await _requestService.CreateAsync(request, CurrentUser.Id);
                return CreatedAtAction(nameof(GetRequest), "Requests", new { requestId = createResult.Id }, createResult);
            }
            return BadRequest();
        }

        /// <summary>
        /// Create request on behalf of another user - ADMIN
        /// </summary>
        /// <param name="request"></param>
        /// <param name="userId">ID of the user</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(RequestResponseModel))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorMessageResponse))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorMessageResponse))]
        [Authorize(Roles = "admin")]
        [HttpPost("Create/OnBehalf/{userId}")]
        public async Task<ActionResult> CreateForUser(RequestCreateRequestModel request, int userId)
        {
            await GetCurrentUserAsync();
            var createResult = await _requestService.CreateAsync(request, CurrentUser.Id, userId);
            return CreatedAtAction("GetRequest", "Requests", new { requestId = createResult.Id }, createResult);
        }

        /// <summary>
        /// Submit request for processing
        /// </summary>
        /// <param name="requestId">ID of the request</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SubmitRequestResponseModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorMessageResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorMessageResponse))]
        [Authorize(Policy = "MustBeRequestCreatorOrAdmin")]
        [HttpPut("Submit/{requestId}")]
        public async Task<ActionResult<RequestResponseModel>> Submit(int requestId)
        {
            var submitionResult = await _requestService.SubmitAsync(requestId);

            return Ok(submitionResult);
        }

        /// <summary>
        /// Approve request - Approver of the request
        /// </summary>
        /// <param name="requestId">ID of the request</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApprovedRequestResponseModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorMessageResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorMessageResponse))]
        [Authorize(Policy = "MustBeRequestApprover")]
        [HttpPut("Approve/{requestId}")]
        public async Task<ActionResult<ApprovedRequestResponseModel>> Approve(int requestId)
        {
            await GetCurrentUserAsync();
            var approvementResult = await _requestService.ApproveAsync(requestId, CurrentUser.Id);

            return Ok(approvementResult);
        }

        /// <summary>
        /// Reject request - Approver of the request
        /// </summary>
        /// <param name="requestId">ID of the request</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RejectedRequestResponseModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorMessageResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorMessageResponse))]
        [Authorize(Policy = "MustBeRequestApprover")]
        [HttpPut("Reject/{requestId}")]
        public async Task<ActionResult<RejectedRequestResponseModel>> Reject(int requestId)
        {
            await GetCurrentUserAsync();
            var rejectionResult = await _requestService.RejectAsync(requestId, CurrentUser.Id);

            return Ok(rejectionResult);
        }

        /// <summary>
        /// Edit a request. It is forbidden to edit request after it is approved/rejected.
        /// Admin or requester.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="requestId">ID of the request</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RequestResponseModel))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorMessageResponse))]
        [Authorize(Policy = "MustBeRequestCreatorOrAdmin")]
        [HttpPut("Edit/{requestId}")]
        public async Task<ActionResult<RequestResponseModel>> Put(RequestCreateRequestModel request, int requestId)
        {
            await GetCurrentUserAsync();
            var requestGeneralResponseModel = await _requestService.EditAsync(request, requestId, CurrentUser.Id);

            return requestGeneralResponseModel;
        }

        /// <summary>
        /// Delete request - Admin or requester. Forbidden to delete after it is approved/rejected
        /// </summary>
        /// <param name="requestId">ID of the request</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RequestResponseModel))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorMessageResponse))]
        [Authorize(Policy = "MustBeRequestCreatorOrAdmin")]
        [HttpDelete("Delete/{requestId}")]
        public async Task<ActionResult> Delete(int requestId)
        {
            var requestDeleteResult = await _requestService.DeleteAsync(requestId);
            return Ok(requestDeleteResult);
        }
    }
}
