namespace SharedDataModels.Utility.Exceptions
{
    public class NotAuthorizedException : Exception
    {
        public NotAuthorizedException()
        {

        }
        public NotAuthorizedException(string Message) : base(Message)
        {

        }
        public NotAuthorizedException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
