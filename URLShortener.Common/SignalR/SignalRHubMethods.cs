namespace URLShortener.Common.SignalR;

public static class SignalRHubMethods
{
    public static class User
    {
        public const string ReceiveBalanceUpdate = "ReceiveBalanceUpdate";
        public const string ReceiveUrlUpdate = "ReceiveUrlUpdate";
    }
}
