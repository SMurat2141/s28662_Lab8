using WarehouseApi.Contracts;
using WarehouseApi.Dtos;
using WarehouseApi.Repositories;

namespace WarehouseApi.Services;

public sealed class WarehouseService(IWarehouseRepository repo) : IWarehouseService
{
    public async Task<WarehouseResponse?> AddAsync(WarehouseRequest req, CancellationToken ct)
    {
        if (req.Amount <= 0) throw new ArgumentOutOfRangeException(nameof(req.Amount));
        var newId = await repo.AddProductAsync(req, ct);
        return newId is null ? null : new WarehouseResponse(newId.Value);
    }

    public async Task<WarehouseResponse?> AddViaProcAsync(WarehouseRequest req, CancellationToken ct)
    {
        if (req.Amount <= 0) throw new ArgumentOutOfRangeException(nameof(req.Amount));
        var newId = await repo.AddProductViaProcAsync(req, ct);
        return newId is null ? null : new WarehouseResponse(newId.Value);
    }
}