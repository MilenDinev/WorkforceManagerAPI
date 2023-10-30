namespace WorkforceManager.Data.Constants
{
    public static class UserServiceConstants
    {
        public const string AdminRole = "admin";        
        public const string AdminRoleNum = "1";
        public const string RegularRoleNum = "2";
        public const string RegularRole = "regular";
        public const string InitialAdminUsername = "admin";
        public const string UserDoesNotExistErrorMessage = @"User with ID '{0}' does not exist!";
        public const string DeleteInitialUserErrorMessage = @"User with id '{0}' is  initial user and cannot be deleted!";
        public const string UsernameAlreadyExistsErrorMessage = @"User with username '{0}' already exists!";
        public const string EmailAlreadyExistsErrorMessage = @"Email address '{0}' already exists!";

    }
}
