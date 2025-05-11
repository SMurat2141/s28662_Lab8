namespace WarehouseApi.Dtos;

public record WarehouseRequest(
    int      IdProduct,
    int      IdWarehouse,
    int      Amount,
    DateTime CreatedAt);

