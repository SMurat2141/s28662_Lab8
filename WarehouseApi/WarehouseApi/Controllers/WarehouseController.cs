using Microsoft.AspNetCore.Mvc;
using WarehouseApi.Contracts;
using WarehouseApi.Dtos;

namespace WarehouseApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class WarehouseController(IWarehouseService svc, ILogger<WarehouseController> log)
    : ControllerBase
{
    [HttpPost]                         // Task 1
    public async Task<IActionResult> Post(
        [FromBody] WarehouseRequest body, CancellationToken ct)
    {
        var result = await svc.AddAsync(body, ct);
        return result is null ? NotFound() : CreatedAtAction(nameof(Get), new { id = result.IdProductWarehouse }, result);
    }

    [HttpPost("proc")]                 // Task 2
    public async Task<IActionResult> PostProc(
        [FromBody] WarehouseRequest body, CancellationToken ct)
    {
        var result = await svc.AddViaProcAsync(body, ct);
        return result is null ? NotFound() : CreatedAtAction(nameof(Get), new { id = result.IdProductWarehouse }, result);
    }

    // demo GET just to satisfy CreatedAtAction (not in tutorial spec)
    [HttpGet("{id:int}")]
    public IActionResult Get(int id) => Ok(new { id });
}