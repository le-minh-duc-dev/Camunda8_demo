namespace WebApplication4.Services;

using Microsoft.Extensions.Logging;


public interface IInventoryService
{
    bool CheckStock(string orderId);
}

public class InventoryService : IInventoryService
{
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(ILogger<InventoryService> logger)
    {
        _logger = logger;
    }

    public bool CheckStock(string orderId)
    {
        // Fake logic, true = available
        bool available = true;
        _logger.LogInformation("Checking stock for Order {OrderId}: {Availability}", orderId, available);
        return available;
    }
}

public interface IReservationService
{
    bool Reserve(string orderId);
}

public class ReservationService : IReservationService
{
    private readonly ILogger<ReservationService> _logger;

    public ReservationService(ILogger<ReservationService> logger)
    {
        _logger = logger;
    }

    public bool Reserve(string orderId)
    {
        bool reserved = true; // Fake reservation
        _logger.LogInformation("Reserving order {OrderId}: {Result}", orderId, reserved);
        return reserved;
    }
}

public interface IReleaseService
{
    bool Release(string orderId);
}

public class ReleaseService : IReleaseService
{
    private readonly ILogger<ReleaseService> _logger;

    public ReleaseService(ILogger<ReleaseService> logger)
    {
        _logger = logger;
    }

    public bool Release(string orderId)
    {
        bool released = true; // Fake release
        _logger.LogInformation("Releasing reservation for order {OrderId}: {Result}", orderId, released);
        return released;
    }
}

public interface IPaymentService
{
    bool Charge(string orderId, decimal amount);
}

public class PaymentService : IPaymentService
{
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(ILogger<PaymentService> logger)
    {
        _logger = logger;
    }

    public bool Charge(string orderId, decimal amount)
    {
        bool charged = amount > 100;
        _logger.LogInformation("Charging order {OrderId} with amount {Amount}: {Result}", orderId, amount, charged);
        return charged;
    }
}

public interface IShippingService
{
    bool Ship(string orderId);
}

public class ShippingService : IShippingService
{
    private readonly ILogger<ShippingService> _logger;

    public ShippingService(ILogger<ShippingService> logger)
    {
        _logger = logger;
    }

    public bool Ship(string orderId)
    {
        bool shipped = true; // Fake ship
        _logger.LogInformation("Shipping order {OrderId}: {Result}", orderId, shipped);
        return shipped;
    }
}
