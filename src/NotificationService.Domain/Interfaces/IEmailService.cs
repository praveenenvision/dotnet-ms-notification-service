namespace NotificationService.Domain.Interfaces;

public interface IEmailService
{
    Task SendOrderPlacedAsync(string customerEmail, int orderId, decimal totalAmount, string traceId);
    Task SendOrderModifiedAsync(string customerEmail, int orderId, decimal newTotal, string traceId);
    Task SendOrderCancelledAsync(string customerEmail, int orderId, string reason, string traceId);
    Task SendOrderConfirmedAsync(string customerEmail, int orderId, decimal totalAmount, string traceId);
    Task SendInventoryReducedAsync(string productName, int quantityReduced, int newStock, string traceId);
}
