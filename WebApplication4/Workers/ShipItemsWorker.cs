using System.Text.Json;
using WebApplication4.Services;
using Zeebe.Client;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace WebApplication4.Workers;

public class ShipItemsWorker : BackgroundService
{
    private readonly IZeebeClient _client;
    private readonly IShippingService _shippingService;

    public ShipItemsWorker(IZeebeClient client, IShippingService shippingService)
    {
        _client = client;
        _shippingService = shippingService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client.NewWorker()
               .JobType("ship-items")
               .Handler(HandleJob)
               .Name("ShipItemsWorker")
               .MaxJobsActive(5)
               .Timeout(TimeSpan.FromSeconds(30))
               .PollInterval(TimeSpan.FromSeconds(1))
               .Open();

        return Task.CompletedTask;
    }

    private void HandleJob(IJobClient jobClient, IJob job)
    {
        var variables = JsonSerializer.Deserialize<Dictionary<string, object>>(job.Variables)
                        ?? throw new Exception("Job variables null");
        if (variables.TryGetValue("orderId", out var obj) && obj is JsonElement je && je.ValueKind == JsonValueKind.String)
        {
            bool shipped = _shippingService.Ship(je.GetString()!);

            if (!shipped)
            {
                _ = jobClient.NewFailCommand(job.Key)
                              .Retries(job.Retries - 1)
                              .ErrorMessage("Shipping failed")
                              .Send()
                              .GetAwaiter()
                              .GetResult();
                return;
            }

            _ = jobClient.NewCompleteJobCommand(job.Key)
                          .Variables(JsonSerializer.Serialize(new
                          {
                              shipped
                          }))
                          .Send()
                          .GetAwaiter()
                          .GetResult();
            return;

        }

        _ = jobClient.NewFailCommand(job.Key)
                          .Retries(job.Retries - 1)
                          .ErrorMessage("orderId missing")
                          .Send()
                          .GetAwaiter()
                          .GetResult();
    }
}