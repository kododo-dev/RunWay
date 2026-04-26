using Kododo.RunWay.Core.Recurrences;
using Kododo.RunWay.Core.Store.Queries;

namespace Kododo.RunWay.Core.Store;

public interface IRecurrenceStore
{
    Task<Recurrence?> FindRecurrenceAsync(RecurrenceId id, CancellationToken stoppingToken);
    Task<Recurrence> CreateRecurrenceWithoutTransactionAsync(Recurrence recurrence, CancellationToken stoppingToken);
    Task<Recurrence> UpdateRecurrenceWithoutTransactionAsync(Recurrence recurrence, CancellationToken stoppingToken);
    Task<PagedResult<Recurrence>> GetRecurrencesAsync(Pagination pagination, CancellationToken stoppingToken);
}