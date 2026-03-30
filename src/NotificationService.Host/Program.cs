using NotificationService.Application.Interfaces;
using NotificationService.Application.Services;
using NotificationService.Domain.Interfaces;
using NotificationService.Infrastructure.Messaging;
using NotificationService.Infrastructure.Services;
using DotnetMsPoc.Shared.Telemetry;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IDomainEventHandler, DomainEventHandler>();
builder.Services.AddHostedService<DomainEventConsumer>();
builder.Services.AddCustomOpenTelemetry("NotificationService");

var host = builder.Build();
host.Run();
