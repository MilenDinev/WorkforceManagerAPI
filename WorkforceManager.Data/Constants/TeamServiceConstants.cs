

namespace WorkforceManager.Data.Constants
{
    public static class TeamServiceConstants
    {
        public const string TeamTitleAlreadyExistsErrorMessage = @"Team with title '{0}' already exists!";       
        public const string UserAlreadyTeamLeader = @"User with id '{0}' is already Team Leader! Only one team can be led at the same time!"; 
        public const string UserNotMemberOfTeam = @"User with id '{0}' is not part of '{1}' !";        
        public const string UserAlreadyMemberOfTeam = @"User '{0}' is already part of team '{1}' !";        
        public const string TeamDoesNotExistsErrorMessage = @"Team with id '{0}' does not exists!";
    }
}
