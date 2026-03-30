using Microsoft.Extensions.Logging;
using NotificationService.Domain.Interfaces;

namespace NotificationService.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendOrderPlacedAsync(string customerEmail, int orderId, decimal totalAmount, string traceId)
    {
        _logger.LogInformation(
            "[TraceId: {TraceId}] EMAIL SENT to {Email} - Your order #{OrderId} has been placed. Total: ${Total:F2}",
            traceId, customerEmail, orderId, totalAmount);
        return Task.CompletedTask;
    }

    public Task SendOrderModifiedAsync(string customerEmail, int orderId, decimal newTotal, string traceId)
    {
        _logger.LogInformation(
            "[TraceId: {TraceId}] EMAIL SENT to {Email} - Your order #{OrderId} has been updated. New Total: ${Total:F2}",
            traceId, customerEmail, orderId, newTotal);
        return Task.CompletedTask;
    }

    public Task SendOrderCancelledAsync(string customerEmail, int orderId, string reason, string traceId)
    {
        _logger.LogInformation(
            "[TraceId: {TraceId}] EMAIL SENT to {Email} - Your order #{OrderId} has been cancelled. Reason: {Reason}",
            traceId, customerEmail, orderId, reason);
        return Task.CompletedTask;
    }

    public Task SendOrderConfirmedAsync(string customerEmail, int orderId, decimal totalAmount, string traceId)
    {
        _logger.LogInformation(
            "[TraceId: {TraceId}] EMAIL SENT to {Email} - Your order #{OrderId} is confirmed. Sales Invoice generated. Total: ${Total:F2}",
            traceId, customerEmail, orderId, totalAmount);
        return Task.CompletedTask;
    }

    public Task SendInventoryReducedAsync(string productName, int quantityReduced, int newStock, string traceId)
    {
        _logger.LogInformation(
            "[TraceId: {TraceId}] NOTIFICATION - Stock updated for {Product}: reduced by {Reduced}, {Remaining} remaining",
            traceId, productName, quantityReduced, newStock);
        return Task.CompletedTask;
    }
}
