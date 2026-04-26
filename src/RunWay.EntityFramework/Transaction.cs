using Kododo.RunWay.Core.Store;
using Microsoft.EntityFrameworkCore.Storage;

namespace Kododo.RunWay.EntityFramework;

internal class Transaction(IDbContextTransaction transaction) : ITransaction
{
    public ValueTask DisposeAsync()
    {
        return transaction.DisposeAsync();
    }

    public Task CommitAsync(CancellationToken stoppingToken)
    {
        return transaction.CommitAsync(stoppingToken);
    }
}