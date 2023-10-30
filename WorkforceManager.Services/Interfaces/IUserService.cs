namespace WorkforceManager.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using WorkforceManager.Data.Entities;
    using WorkforceManager.Models.Requests.UserRequestModels;
    using WorkforceManager.Models.Responses.UserResponseModels;

    public interface IUserService
    {
        Task<CreatedUserResponseModel> CreateAsync(UserCreateRequestModel userRequestModel, int creatorId);
        Task<UserResponseModel> EditAsync(UserEditRequestModel userEditRequestModel, int userId, int modifierId);
        Task<DeletedUserResponseModel> DeleteAsync(int userId);
        Task<User> GetCurrentUserAsync(ClaimsPrincipal principal);
        Task<ICollection<UserResponseModel>> GetAllUsersAsync();
        Task<bool> IsUserAdminAsync(User user);
        Task<bool> IsTheUserTeamLeadAsync(int userId);
        Task<User> GetUserByNameAsync(string userName);
        Task<UserResponseModel> GetUserByIdAsync(int userId);
    }
}
