namespace Kododo.RunWay.Core.Store;

public interface ITransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken stoppingToken);
}