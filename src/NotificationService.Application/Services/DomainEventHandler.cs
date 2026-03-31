using System.Text.Json;
using DotnetMsPoc.Shared.Events;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Interfaces;

namespace NotificationService.Application.Services;

public class DomainEventHandler : IDomainEventHandler
{
    private readonly IEmailService _emailService;
    private readonly INotificationLogRepository _notificationLogRepository;
    private readonly ILogger<DomainEventHandler> _logger;

    public DomainEventHandler(
        IEmailService emailService,
        INotificationLogRepository notificationLogRepository,
        ILogger<DomainEventHandler> logger)
    {
        _emailService = emailService;
        _notificationLogRepository = notificationLogRepository;
        _logger = logger;
    }

    public async Task HandleEventAsync(string routingKey, string jsonBody)
    {
        _logger.LogInformation("Handling domain event: {RoutingKey}", routingKey);

        string message;
        string? traceId = null;

        switch (routingKey)
        {
            case "order.placed":
                var placedEvent = JsonSerializer.Deserialize<OrderPlacedEvent>(jsonBody);
                if (placedEvent != null)
                {
                    traceId = placedEvent.TraceId;
                    message = $"Order #{placedEvent.OrderId} placed by {placedEvent.CustomerEmail}. Total: ${placedEvent.TotalAmount:F2}";
                    await _emailService.SendOrderPlacedAsync(placedEvent.CustomerEmail, placedEvent.OrderId, placedEvent.TotalAmount, placedEvent.TraceId);
                    await PersistLogAsync(routingKey, "order.placed", message, traceId, jsonBody);
                }
                break;

            case "order.modified":
                var modifiedEvent = JsonSerializer.Deserialize<OrderModifiedEvent>(jsonBody);
                if (modifiedEvent != null)
                {
                    traceId = modifiedEvent.TraceId;
                    message = $"Order #{modifiedEvent.OrderId} modified. New Total: ${modifiedEvent.NewTotalAmount:F2}";
                    await _emailService.SendOrderModifiedAsync(modifiedEvent.CustomerEmail, modifiedEvent.OrderId, modifiedEvent.NewTotalAmount, modifiedEvent.TraceId);
                    await PersistLogAsync(routingKey, "order.modified", message, traceId, jsonBody);
                }
                break;

            case "order.cancelled":
                var cancelledEvent = JsonSerializer.Deserialize<OrderCancelledEvent>(jsonBody);
                if (cancelledEvent != null)
                {
                    traceId = cancelledEvent.TraceId;
                    message = $"Order #{cancelledEvent.OrderId} cancelled. Reason: {cancelledEvent.Reason}";
                    await _emailService.SendOrderCancelledAsync(cancelledEvent.CustomerEmail, cancelledEvent.OrderId, cancelledEvent.Reason, cancelledEvent.TraceId);
                    await PersistLogAsync(routingKey, "order.cancelled", message, traceId, jsonBody);
                }
                break;

            case "order.confirmed":
                var confirmedEvent = JsonSerializer.Deserialize<OrderConfirmedEvent>(jsonBody);
                if (confirmedEvent != null)
                {
                    traceId = confirmedEvent.TraceId;
                    message = $"Order #{confirmedEvent.OrderId} confirmed. Total: ${confirmedEvent.TotalAmount:F2}";
                    await _emailService.SendOrderConfirmedAsync(confirmedEvent.CustomerEmail, confirmedEvent.OrderId, confirmedEvent.TotalAmount, confirmedEvent.TraceId);
                    await PersistLogAsync(routingKey, "order.confirmed", message, traceId, jsonBody);
                }
                break;

            case "inventory.reduced":
                var reducedEvent = JsonSerializer.Deserialize<InventoryReducedEvent>(jsonBody);
                if (reducedEvent != null)
                {
                    traceId = reducedEvent.TraceId;
                    message = $"Stock updated for {reducedEvent.ProductName}: reduced by {reducedEvent.QuantityReduced}, {reducedEvent.NewStock} remaining";
                    await _emailService.SendInventoryReducedAsync(reducedEvent.ProductName, reducedEvent.QuantityReduced, reducedEvent.NewStock, reducedEvent.TraceId);
                    await PersistLogAsync(routingKey, "inventory.reduced", message, traceId, jsonBody);
                }
                break;

            default:
                _logger.LogWarning("Unknown event routing key: {RoutingKey}", routingKey);
                break;
        }
    }

    private async Task PersistLogAsync(string routingKey, string eventType, string message, string? traceId, string jsonBody)
    {
        try
        {
            var log = new NotificationLog
            {
                EventType = eventType,
                RoutingKey = routingKey,
                Message = message,
                TraceId = traceId,
                EventPayload = jsonBody,
                CreatedAt = DateTime.UtcNow
            };
            await _notificationLogRepository.AddAsync(log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist notification log for {RoutingKey}", routingKey);
        }
    }
}
