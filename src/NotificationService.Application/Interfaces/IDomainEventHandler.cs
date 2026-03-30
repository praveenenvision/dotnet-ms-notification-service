namespace NotificationService.Application.Interfaces;

public interface IDomainEventHandler
{
    Task HandleEventAsync(string routingKey, string jsonBody);
}
