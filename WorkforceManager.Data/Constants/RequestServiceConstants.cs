namespace WorkforceManager.Data.Constants
{
    public static class RequestServiceConstants
    {

        public const string NonExistingRequestErrorMessage = @"Request with id '{0}' does not exists!";
        
        public const string OverlappingRequestErrorMessage = @"There is a request from user with id '{0}' for the period '{1}' - '{2}'. Requests by same user cannot overlap!";

        public const string SubmitAlreadySubmitedRequestErrorMessage = @"You cannot submit request which has already been submited!";

        public const string ApproveNotAwaitingRequestErrorMessage = @"You can only approve requests that are awaiting!";

        public const string RejectNotAwaitingRequestErrorMessage = @"You can only reject requests that are awaiting!";

        public const string AlreadyProcessedRequestErrorMessage = @"Request with id '{0}' has been already processed and cannot be edited!";

        public const string InvalidStatusChosenErrorMessage = "Status can be only one of \"Created\", \"Awaiting\", \"Approved\" or \"Rejected\".";

        public const string DeleteAlreadyProcessedRequestErrorMessage = @"Request with id '{0}' has been already processed and cannot be deleted!";


     }
}
