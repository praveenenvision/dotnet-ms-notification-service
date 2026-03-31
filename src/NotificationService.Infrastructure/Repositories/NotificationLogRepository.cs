using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Interfaces;
using NotificationService.Infrastructure.Persistence;

namespace NotificationService.Infrastructure.Repositories;

public class NotificationLogRepository : INotificationLogRepository
{
    private readonly NotificationDbContext _context;

    public NotificationLogRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(NotificationLog log)
    {
        _context.NotificationLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task<List<NotificationLog>> GetAllAsync()
    {
        return await _context.NotificationLogs
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<NotificationLog>> GetByTraceIdAsync(string traceId)
    {
        return await _context.NotificationLogs
            .Where(n => n.TraceId == traceId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }
}
