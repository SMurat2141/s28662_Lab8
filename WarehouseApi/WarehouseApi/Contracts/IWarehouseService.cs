using WarehouseApi.Dtos;

namespace WarehouseApi.Contracts;
public interface IWarehouseService
{
    Task<WarehouseResponse?> AddAsync (WarehouseRequest req, CancellationToken ct);
    Task<WarehouseResponse?> AddViaProcAsync(WarehouseRequest req, CancellationToken ct);
}