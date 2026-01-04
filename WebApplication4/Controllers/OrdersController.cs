using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebApplication4.Models;
using Zeebe.Client;

namespace WebApplication4.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly IZeebeClient _client;

    public OrdersController(IZeebeClient client)
    {
        _client = client;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] Order order)
    {
        var json = JsonSerializer.Serialize(
                    order,
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }
                );
        var processInstance = await _client.NewCreateProcessInstanceCommand()
            .BpmnProcessId("process1")
            .LatestVersion()
            .Variables(json)
            .Send();
        return Ok(new
        {
            Message = "Order workflow started",
            processInstance.ProcessInstanceKey
        });
    }
}
