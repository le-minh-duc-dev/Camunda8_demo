using System.Text.Json;
using WebApplication4.Services;
using Zeebe.Client;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace WebApplication4.Workers;

public class ChargePaymentWorker : BackgroundService
{
    private readonly IZeebeClient _client;
    private readonly IPaymentService _paymentService;

    public ChargePaymentWorker(IZeebeClient client, IPaymentService paymentService)
    {
        _client = client;
        _paymentService = paymentService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client.NewWorker()
               .JobType("charge-payment")
               .Handler(HandleJob)
               .Name("ChargePaymentWorker")
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
            var amount = 50;

            bool paid = _paymentService.Charge(je.GetString()!, amount);

            _ = jobClient.NewCompleteJobCommand(job.Key)
                          .Variables(JsonSerializer.Serialize(new
                          {
                              paid
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