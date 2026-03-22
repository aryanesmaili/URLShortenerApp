namespace URLShortener.Common.SignalR;

public interface ISignalRUserClient
{
    Task ReceiveBalanceUpdate(long balance);
    Task ReceiveURLCountUpdate(int count = 1);
}
