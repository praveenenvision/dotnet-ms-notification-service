using NotificationService.Domain.Entities;

namespace NotificationService.Domain.Interfaces;

public interface INotificationLogRepository
{
    Task AddAsync(NotificationLog log);
    Task<List<NotificationLog>> GetAllAsync();
    Task<List<NotificationLog>> GetByTraceIdAsync(string traceId);
}
