namespace URLShortener.Common.SignalR;

public interface ISignalRUserClient
{
    Task ReceiveBalanceUpdate(double balance);
    Task ReceiveURLCountUpdate(int count = 1);
}
