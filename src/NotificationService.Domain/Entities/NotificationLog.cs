namespace NotificationService.Domain.Entities;

public class NotificationLog
{
    public int Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string RoutingKey { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? TraceId { get; set; }
    public string? EventPayload { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
