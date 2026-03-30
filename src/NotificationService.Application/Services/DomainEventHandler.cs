using System.Text.Json;
using DotnetMsPoc.Shared.Events;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Interfaces;

namespace NotificationService.Application.Services;

public class DomainEventHandler : IDomainEventHandler
{
    private readonly IEmailService _emailService;
    private readonly ILogger<DomainEventHandler> _logger;

    public DomainEventHandler(IEmailService emailService, ILogger<DomainEventHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task HandleEventAsync(string routingKey, string jsonBody)
    {
        _logger.LogInformation("Handling domain event: {RoutingKey}", routingKey);

        switch (routingKey)
        {
            case "order.placed":
                var placedEvent = JsonSerializer.Deserialize<OrderPlacedEvent>(jsonBody);
                if (placedEvent != null)
                    await _emailService.SendOrderPlacedAsync(placedEvent.CustomerEmail, placedEvent.OrderId, placedEvent.TotalAmount, placedEvent.TraceId);
                break;

            case "order.modified":
                var modifiedEvent = JsonSerializer.Deserialize<OrderModifiedEvent>(jsonBody);
                if (modifiedEvent != null)
                    await _emailService.SendOrderModifiedAsync(modifiedEvent.CustomerEmail, modifiedEvent.OrderId, modifiedEvent.NewTotalAmount, modifiedEvent.TraceId);
                break;

            case "order.cancelled":
                var cancelledEvent = JsonSerializer.Deserialize<OrderCancelledEvent>(jsonBody);
                if (cancelledEvent != null)
                    await _emailService.SendOrderCancelledAsync(cancelledEvent.CustomerEmail, cancelledEvent.OrderId, cancelledEvent.Reason, cancelledEvent.TraceId);
                break;

            case "order.confirmed":
                var confirmedEvent = JsonSerializer.Deserialize<OrderConfirmedEvent>(jsonBody);
                if (confirmedEvent != null)
                    await _emailService.SendOrderConfirmedAsync(confirmedEvent.CustomerEmail, confirmedEvent.OrderId, confirmedEvent.TotalAmount, confirmedEvent.TraceId);
                break;

            case "inventory.reduced":
                var reducedEvent = JsonSerializer.Deserialize<InventoryReducedEvent>(jsonBody);
                if (reducedEvent != null)
                    await _emailService.SendInventoryReducedAsync(reducedEvent.ProductName, reducedEvent.QuantityReduced, reducedEvent.NewStock, reducedEvent.TraceId);
                break;

            default:
                _logger.LogWarning("Unknown event routing key: {RoutingKey}", routingKey);
                break;
        }
    }
}
