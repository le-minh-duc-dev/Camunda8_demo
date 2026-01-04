using System.Text.Json;
using WebApplication4.Services;
using Zeebe.Client;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace WebApplication4.Workers;

public class ReserveInventoryWorker : BackgroundService
{
    private readonly IZeebeClient _client;
    private readonly IReservationService _reservationService;

    public ReserveInventoryWorker(IZeebeClient client, IReservationService reservationService)
    {
        _client = client;
        _reservationService = reservationService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client.NewWorker()
            .JobType("reserve-inventory")
            .Handler(HandleJob)
            .Name("ReserveInventoryWorker")
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

            bool reserved = _reservationService.Reserve(je.GetString()!);

            if (!reserved)
            {
                _ = jobClient.NewFailCommand(job.Key)
                              .Retries(job.Retries - 1)
                              .ErrorMessage("Inventory not reserved")
                              .Send()
                              .GetAwaiter()
                              .GetResult();
                return;
            }

            _ = jobClient.NewCompleteJobCommand(job.Key)
                          .Variables(JsonSerializer.Serialize(new
                          {
                              reserved
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

