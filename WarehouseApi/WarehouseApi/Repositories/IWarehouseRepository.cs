using WarehouseApi.Dtos;

namespace WarehouseApi.Repositories;

public interface IWarehouseRepository
{
    Task<int?> AddProductAsync(WarehouseRequest req, CancellationToken ct);
    Task<int?> AddProductViaProcAsync(WarehouseRequest req, CancellationToken ct);
}