namespace WorkforceManager.Services
{
    using AutoMapper;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using WorkforceManager.Data;
    using WorkforceManager.Data.Constants;
    using WorkforceManager.Data.Entities;
    using WorkforceManager.Models.Requests.UserRequestModels;
    using WorkforceManager.Models.Responses.UserResponseModels;
    using WorkforceManager.Services.Interfaces;

    public class UserService : IUserService
    {
        private readonly IUserManager _userManager;
        private readonly WorkforceManagerDbContext _dbContext;
        private readonly IMapper _mapper;

        public UserService(IUserManager userManager, WorkforceManagerDbContext dbContext, IMapper mapper)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<CreatedUserResponseModel> CreateAsync(UserCreateRequestModel userRequestModel, int creatorId)
        {
            if (await _userManager.FindByNameAsync(userRequestModel.UserName) != null)
                throw new ArgumentException(string.Format(UserServiceConstants.UsernameAlreadyExistsErrorMessage, userRequestModel.UserName));

            if (await _userManager.FindByEmailAsync(userRequestModel.Email) != null)
                throw new ArgumentException(string.Format(UserServiceConstants.EmailAlreadyExistsErrorMessage, userRequestModel.Email));

            var user = _mapper.Map<User>(userRequestModel);

            user.CreatorId = creatorId;
            user.LastModifierId = creatorId;

            var role = userRequestModel.Role == UserServiceConstants.AdminRoleNum || 
                userRequestModel.Role.ToLower() == UserServiceConstants.AdminRole.ToLower() ? 
                UserServiceConstants.AdminRole : UserServiceConstants.RegularRole;

            await _userManager.CreateAsync(user, userRequestModel.Password);
            await _userManager.AddToRoleAsync(user, role);


            var isValidTeamId = int.TryParse(userRequestModel.TeamId, out int teamId);
            var team = await _dbContext.Teams.FirstOrDefaultAsync(t => t.Id == teamId);
            var result = string.Empty;
            if (isValidTeamId && team != null)
            {
                team.Members.Add(user);
            }            

            await _dbContext.SaveChangesAsync();
            
            return _mapper.Map<CreatedUserResponseModel>(user);
        }

        public async Task<UserResponseModel> EditAsync(UserEditRequestModel userEditRequestModel, int userId, int modifierId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                throw new KeyNotFoundException($"User with id '{userId}' does not exists!");

            var existingUserWithNewUserName = await _userManager.FindByNameAsync(userEditRequestModel.UserName);
            var existingUserWithNewEmail =  await _userManager.FindByEmailAsync(userEditRequestModel.Email);

            if (existingUserWithNewUserName != null && existingUserWithNewUserName.Id != userId)
                throw new ArgumentException($"User with username '{userEditRequestModel.UserName}' already exists!");
            if (existingUserWithNewEmail != null && existingUserWithNewEmail.Id != userId)
                throw new ArgumentException($"User with email '{userEditRequestModel.Email}' already exists!");

            var hasher = new PasswordHasher<User>();

            user.UserName = userEditRequestModel.UserName;
            user.PasswordHash = hasher.HashPassword(user, userEditRequestModel.Password);
            user.LastModifierId = modifierId;
            user.LastModificationDate = DateTime.Now;
            user.Email = userEditRequestModel.Email;

            await _userManager.UpdateAsync(user);

            var userResponseModel = _mapper.Map<UserResponseModel>(user);
            return userResponseModel;
        }

        public async Task<DeletedUserResponseModel> DeleteAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new KeyNotFoundException(string.Format(UserServiceConstants.UserDoesNotExistErrorMessage, userId));
            if (user.UserName == UserServiceConstants.InitialAdminUsername)
                throw new ArgumentException(string.Format(UserServiceConstants.DeleteInitialUserErrorMessage, 1));


            var approversRequests = await _dbContext.Set<ApproverRequest>().ToListAsync();

            var requestsToBeProcessedByUser = approversRequests
                .Where(r => user.RequestsToApprove.Any(ur => ur.Id == r.RequestId) && (r.ApproverId != user.Id))
                .ToList();

            var requestsToBeAutoApproved = new List<int>();

            foreach (var approverRequest in requestsToBeProcessedByUser)
            {
                var isProcessedByAllOtherApprovers = requestsToBeProcessedByUser.All(r => r.RequestId == approverRequest.RequestId && r.IsProcessed);
                if (isProcessedByAllOtherApprovers)
                    requestsToBeAutoApproved.Add(approverRequest.RequestId);
            }

            var requestsToBeApproved = await _dbContext.TimeOffRequests.Where(r => r.Approvers.Contains(user) || requestsToBeAutoApproved.Contains(r.Id)).ToListAsync();
            var approvedStatus = await _dbContext.Statuses.FirstOrDefaultAsync(s => s.State == "Approved");
            foreach (var request in requestsToBeApproved)
            {
                request.Status = approvedStatus;
            }

            var result = _mapper.Map<DeletedUserResponseModel>(user);
            user.TeamsLed.Clear();
            user.RequestsToApprove.Clear();
            await _userManager.DeleteAsync(user);
            await _dbContext.SaveChangesAsync();

            return result;
        }

        public async Task<ICollection<UserResponseModel>> GetAllUsersAsync()
        {
            var users = await _userManager.GetAllAsync();
            var usersResponceDto = _mapper.Map<ICollection<UserResponseModel>>(users);
            return usersResponceDto.ToList();
        }

        public async Task<User> GetUserByNameAsync(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            return user;
        }

        public async Task<UserResponseModel> GetUserByIdAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new KeyNotFoundException(string.Format(UserServiceConstants.UserDoesNotExistErrorMessage, userId));

            return _mapper.Map<UserResponseModel>(user);
        }

        public async Task<User> GetCurrentUserAsync(ClaimsPrincipal principal)
        {
            return await _userManager.GetUserAsync(principal);
        }

        public async Task<bool> IsUserAdminAsync(User user)
        {
            var userRoles = await _userManager.GetUserRolesAsync(user);

            return userRoles.Contains(UserServiceConstants.AdminRole);
        }

        public async Task<bool> IsTheUserTeamLeadAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            return user.TeamsLed.Any();
        }
    }
}
