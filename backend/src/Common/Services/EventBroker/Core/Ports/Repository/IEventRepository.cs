namespace Common.Services.EventBroker.Core.Ports.Repository;

using Common.Services.EventBroker.Core.Entities;
using Common.Data.Repository;

public interface IEventRepository : IBaseRepository<EventEntity>
{
    Task MarkProcessedAsync(EventEntity ev, CancellationToken cancellationToken = default);
    
    Task DeleteProcessedAsync(DateTime olderThan, CancellationToken cancellationToken = default);
}