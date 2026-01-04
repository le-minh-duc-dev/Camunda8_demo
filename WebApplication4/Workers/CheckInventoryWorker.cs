using System.Text.Json;
using WebApplication4.Services;
using Zeebe.Client;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace WebApplication4.Workers;

public class CheckInventoryWorker : BackgroundService
{
    private readonly IZeebeClient _client;
    private readonly IInventoryService _inventoryService;

    public CheckInventoryWorker(IZeebeClient client, IInventoryService inventoryService)
    {
        _client = client;
        _inventoryService = inventoryService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client.NewWorker()
            .JobType("check-inventory")
            .Handler(HandleJob)
            .Name("CheckInventoryWorker")
            .MaxJobsActive(5)
            .Timeout(TimeSpan.FromSeconds(30))
            .PollInterval(TimeSpan.FromSeconds(1))
            .Open();

        return Task.CompletedTask;
    }

    private void HandleJob(IJobClient jobClient, IJob job)
    {
        var variables = JsonSerializer.Deserialize<Dictionary<string, object>>(job.Variables);

        if (variables.TryGetValue("orderId", out var obj) && obj is JsonElement je && je.ValueKind == JsonValueKind.String)
        {

            bool available = _inventoryService.CheckStock(je.GetString()!);

            if (!available)
            {
                _ = jobClient.NewFailCommand(job.Key)
                              .Retries(job.Retries - 1)
                              .ErrorMessage("Inventory unavailable")
                              .Send()
                              .GetAwaiter()
                              .GetResult();
                return;
            }

            _ = jobClient.NewCompleteJobCommand(job.Key)
                          .Variables(JsonSerializer.Serialize(new
                          {
                              available
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