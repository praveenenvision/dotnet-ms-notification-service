using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.Infrastructure.Messaging;

public class DomainEventConsumer : BackgroundService
{
    private readonly ILogger<DomainEventConsumer> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private IConnection? _connection;
    private IModel? _channel;

    public DomainEventConsumer(
        ILogger<DomainEventConsumer> logger,
        IConfiguration configuration,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        var rabbitHost = _configuration["RabbitMQ:Host"] ?? "localhost";
        var rabbitPort = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672");

        for (int i = 0; i < 10; i++)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = rabbitHost,
                    Port = rabbitPort,
                    UserName = _configuration["RabbitMQ:Username"] ?? "guest",
                    Password = _configuration["RabbitMQ:Password"] ?? "guest"
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.ExchangeDeclare("domain_events", ExchangeType.Topic, durable: true);
                _channel.QueueDeclare("notification_events", durable: true, exclusive: false, autoDelete: false);
                _channel.QueueBind("notification_events", "domain_events", "order.placed");
                _channel.QueueBind("notification_events", "domain_events", "order.modified");
                _channel.QueueBind("notification_events", "domain_events", "order.cancelled");
                _channel.QueueBind("notification_events", "domain_events", "order.confirmed");
                _channel.QueueBind("notification_events", "domain_events", "inventory.reduced");

                _logger.LogInformation("DomainEventConsumer connected to RabbitMQ - listening for all domain events");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to connect to RabbitMQ (attempt {Attempt}): {Message}", i + 1, ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)), stoppingToken);
            }
        }

        if (_channel == null)
        {
            _logger.LogError("Could not connect to RabbitMQ after retries");
            return;
        }

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                var routingKey = ea.RoutingKey;

                using var scope = _serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<IDomainEventHandler>();
                await handler.HandleEventAsync(routingKey, body);

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing domain event");
                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        _channel.BasicConsume("notification_events", autoAck: false, consumer: consumer);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
