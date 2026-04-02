using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Services;
using NotificationService.Domain.Interfaces;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Repositories;
using NotificationService.Infrastructure.Services;
using DotnetMsPoc.Shared.Messaging;
using DotnetMsPoc.Shared.Middleware;
using DotnetMsPoc.Shared.Telemetry;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Notification Service API",
        Version = "v1",
        Description = "Microservice for notifications - consumes domain events and provides notification log history."
    });
});

builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IDomainEventHandler, DomainEventHandler>();
builder.Services.AddScoped<INotificationLogRepository, NotificationLogRepository>();
builder.Services.AddEventConsumer(
    builder.Configuration,
    queueName: "notification_events",
    routingKeys: ["order.placed", "order.modified", "order.cancelled", "order.confirmed", "inventory.reduced"],
    handler: async (sp, routingKey, jsonBody) =>
    {
        var handler = sp.GetRequiredService<IDomainEventHandler>();
        await handler.HandleEventAsync(routingKey, jsonBody);
    });
builder.Services.AddCustomOpenTelemetry("NotificationService");

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification Service API v1"));

app.UseCors();
app.UseTraceIdMiddleware();
app.UseCustomOpenTelemetry();
app.MapControllers();

app.Run();
