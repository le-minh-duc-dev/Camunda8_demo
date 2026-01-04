using System.Text.Json;
using WebApplication4.Services;
using Zeebe.Client;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace WebApplication4.Workers;

public class ReleaseInventoryWorker : BackgroundService
{
    private readonly IZeebeClient _client;
    private readonly IReleaseService _releaseInventoryService;

    public ReleaseInventoryWorker(IZeebeClient client, IReleaseService releaseInventoryService)
    {
        _client = client;
        _releaseInventoryService = releaseInventoryService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client.NewWorker()
            .JobType("release-inventory")
            .Handler(HandleJob)
            .Name("ReleaseInventory")
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

            bool isReleased = _releaseInventoryService.Release(je.GetString()!);

            _ = jobClient.NewCompleteJobCommand(job.Key)
                          .Variables(JsonSerializer.Serialize(new
                          {
                              isReleased
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
