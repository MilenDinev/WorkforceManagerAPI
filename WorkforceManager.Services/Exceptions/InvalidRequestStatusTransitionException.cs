using System;

namespace WorkforceManager.Services.Exceptions
{

    public class InvalidRequestStatusTransitionException : Exception
    {
        public InvalidRequestStatusTransitionException()
        {
        }

        public InvalidRequestStatusTransitionException(string message)
            : base(message)
        {
        }

        public InvalidRequestStatusTransitionException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
