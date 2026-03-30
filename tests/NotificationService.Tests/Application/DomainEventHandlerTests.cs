using System.Text.Json;
using DotnetMsPoc.Shared.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NotificationService.Application.Services;
using NotificationService.Domain.Interfaces;
using Xunit;

namespace NotificationService.Tests.Application;

public class DomainEventHandlerTests
{
    private readonly Mock<IEmailService> _emailMock = new();
    private readonly Mock<ILogger<DomainEventHandler>> _loggerMock = new();
    private readonly DomainEventHandler _sut;

    public DomainEventHandlerTests()
    {
        _sut = new DomainEventHandler(_emailMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleOrderPlaced_CallsEmailService()
    {
        var evt = new OrderPlacedEvent
        {
            OrderId = 1,
            CustomerEmail = "test@example.com",
            TotalAmount = 99.99m,
            TraceId = "trace-1",
            Items = new List<OrderEventItem>()
        };
        var json = JsonSerializer.Serialize(evt);

        await _sut.HandleEventAsync("order.placed", json);

        _emailMock.Verify(e => e.SendOrderPlacedAsync("test@example.com", 1, 99.99m, "trace-1"), Times.Once);
    }

    [Fact]
    public async Task HandleOrderModified_CallsEmailService()
    {
        var evt = new OrderModifiedEvent
        {
            OrderId = 2,
            CustomerEmail = "user@example.com",
            NewTotalAmount = 150.00m,
            TraceId = "trace-2",
            Items = new List<OrderEventItem>()
        };
        var json = JsonSerializer.Serialize(evt);

        await _sut.HandleEventAsync("order.modified", json);

        _emailMock.Verify(e => e.SendOrderModifiedAsync("user@example.com", 2, 150.00m, "trace-2"), Times.Once);
    }

    [Fact]
    public async Task HandleOrderCancelled_CallsEmailService()
    {
        var evt = new OrderCancelledEvent
        {
            OrderId = 3,
            CustomerEmail = "cancel@example.com",
            Reason = "Changed mind",
            TraceId = "trace-3"
        };
        var json = JsonSerializer.Serialize(evt);

        await _sut.HandleEventAsync("order.cancelled", json);

        _emailMock.Verify(e => e.SendOrderCancelledAsync("cancel@example.com", 3, "Changed mind", "trace-3"), Times.Once);
    }

    [Fact]
    public async Task HandleOrderConfirmed_CallsEmailService()
    {
        var evt = new OrderConfirmedEvent
        {
            OrderId = 4,
            CustomerEmail = "confirm@example.com",
            TotalAmount = 200.00m,
            TraceId = "trace-4",
            Items = new List<OrderEventItem>()
        };
        var json = JsonSerializer.Serialize(evt);

        await _sut.HandleEventAsync("order.confirmed", json);

        _emailMock.Verify(e => e.SendOrderConfirmedAsync("confirm@example.com", 4, 200.00m, "trace-4"), Times.Once);
    }

    [Fact]
    public async Task HandleInventoryReduced_CallsEmailService()
    {
        var evt = new InventoryReducedEvent
        {
            ProductId = 10,
            ProductName = "Widget",
            QuantityReduced = 5,
            NewStock = 45,
            TraceId = "trace-5"
        };
        var json = JsonSerializer.Serialize(evt);

        await _sut.HandleEventAsync("inventory.reduced", json);

        _emailMock.Verify(e => e.SendInventoryReducedAsync("Widget", 5, 45, "trace-5"), Times.Once);
    }

    [Fact]
    public async Task HandleUnknown_DoesNotThrow()
    {
        var act = () => _sut.HandleEventAsync("unknown.event", "{}");

        await act.Should().NotThrowAsync();
        _emailMock.VerifyNoOtherCalls();
    }
}
