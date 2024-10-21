namespace SharedDataModels.Utility.Exceptions
{
    public class RefreshTokenExpiredException : Exception
    {
        public RefreshTokenExpiredException()
        {

        }
        public RefreshTokenExpiredException(string message) : base(message)
        {

        }
        public RefreshTokenExpiredException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
