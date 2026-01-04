namespace WebApplication4.Services;

public interface IInventoryService
{
    bool CheckStock(string orderId);
}

public class InventoryService : IInventoryService
{
    public bool CheckStock(string orderId)
    {
        // Fake logic, true = available
        return true;
    }
}

public interface IPaymentService
{
    bool Charge(string orderId, decimal amount);
}

public class PaymentService : IPaymentService
{
    public bool Charge(string orderId, decimal amount)
    {
        // Fake charge
        return true;
    }
}

public interface IShippingService
{
    bool Ship(string orderId);
}

public class ShippingService : IShippingService
{
    public bool Ship(string orderId)
    {
        // Fake ship
        Console.WriteLine($"Shipping order {orderId}");
        return true;
    }
}
