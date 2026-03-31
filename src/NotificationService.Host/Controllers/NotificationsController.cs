using Microsoft.AspNetCore.Mvc;
using NotificationService.Domain.Interfaces;

namespace NotificationService.Host.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationLogRepository _repository;

    public NotificationsController(INotificationLogRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Get all notification logs, optionally filtered by traceId.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? traceId)
    {
        if (!string.IsNullOrWhiteSpace(traceId))
        {
            var filtered = await _repository.GetByTraceIdAsync(traceId);
            return Ok(filtered);
        }

        var all = await _repository.GetAllAsync();
        return Ok(all);
    }
}
