using WebApplication4.Services;
using WebApplication4.Workers;
using Zeebe.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton(sp => ZeebeClient.Builder()
                                                .UseGatewayAddress("127.0.0.1:26500")
                                                .UsePlainText()
                                                .Build());


// Domain services
builder.Services.AddSingleton<IInventoryService, InventoryService>();
builder.Services.AddSingleton<IPaymentService, PaymentService>();
builder.Services.AddSingleton<IShippingService, ShippingService>();

// Hosted workers
builder.Services.AddHostedService<CheckInventoryWorker>();
builder.Services.AddHostedService<ChargePaymentWorker>();
builder.Services.AddHostedService<ShipItemsWorker>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "E-Commerce Worker running...");

app.Run();
