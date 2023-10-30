using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkforceManager.Data.Constants
{
    public static class EmailServiceConstants
    {
        public const string SendSubmitNotificationMessage = @"{0} leave request '{1}' has been submitted!";
        
        public const string ApprovedRequestTopicMessage = @"Request has been APPROVED!";

        public const string AutoApprovedTopicMessage = @"{0} leave request '{1}' has been submitted!";

        public const string AutoApprovedBodyMessage = @"{0}The request has been automatically approved!";

        public const string RejectedRequestTopicMessage = @"Request with id '{0}' has been rejected!";

        public const string MemberAddedToTeamUserTopicMessage = "Team membership update!";

        public const string MemberAddedToTeamUserBodyMessage = @"You have been added to team '{0}'";

        public const string MemberAddedToTeamOtherMembersTopicMessage = "Team update: new member.";

        public const string MemberAddedToTeamOtherMembersBodyMessage = @"User '{0}' has been assigned to your team!";

        public const string MemberAddedToTeamLeaderTopicMessage = "Team management update!";

        public const string MemberAddedToTeamLeaderBodyMessage = @"User '{0}' has been assigned to your team. The user has {1} requests to be processed by you!";

        public const string MemberRemovedFromTeamUserTopicMessage = "Team membership update!";

        public const string MemberRemovedFromTeamUserBodyMessage = @"You have been removed from team '{0}'";

        public const string MemberRemovedFromTeamOtherMembersTopicMessage = "Team update: member removed.";

        public const string MemberRemovedFromTeamOtherMembersBodyMessage = @"User '{0}' has been removed from your team {1}!";
    }
}
