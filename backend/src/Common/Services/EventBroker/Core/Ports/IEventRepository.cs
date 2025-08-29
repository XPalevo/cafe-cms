namespace Common.Services.EventBroker.Core.Ports;

using Common.Data.Repository;
using Common.Services.EventBroker.Core.Entities;

public interface IEventRepository : IBaseRepository<EventEntity>
{
    Task<IEnumerable<EventEntity>> GetByStatusAsync(EventStatus status);
    
    Task<IEnumerable<EventEntity>> GetByTopicAsync(string topic);
    
    Task<IEnumerable<EventEntity>> GetByNameAsync(string name);
    
    Task MarkProcessedAsync(EventEntity evt);
    
    Task MarkFailedAsync(EventEntity evt);
    
    Task AddRangeAsync(IEnumerable<EventEntity> events);

    Task<IEnumerable<EventEntity>> GetByCreatedAtRangeAsync(DateTime from, DateTime to);
}