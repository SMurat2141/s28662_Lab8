WarehouseApi – what each piece does(briefly or oversimplified)

WarehouseController.cs
• Exposes two POST endpoints:
  - /api/warehouse       → uses inline SQL via the service/repository stack
  - /api/warehouse/proc  → calls a stored procedure
• Returns 201 / 404 / 400 / 409 so the client sees identical behaviour no
  matter which path is used.

WarehouseService.cs
• Thin business‑layer façade.
• Checks that Amount > 0, then delegates to the repository.
• Maps “new‑id” results to the response DTO.

WarehouseRepository.cs
• **All** direct database work lives here.
• Inline path:
    - opens a serialisable transaction
    - performs the six tutorial steps with SqlCommand
    - returns the generated IdProduct_Warehouse
• Proc path:
    - executes dbo.usp_AddProductToWarehouse and captures @NewId.

ErrorHandlingMiddleware.cs
• Global try/catch that converts SqlException error numbers (50000‑50004) and
  validation exceptions into the correct HTTP status codes.

DTOs (WarehouseRequest / WarehouseResponse)
• Simple records that carry data in and out of the API; keeps controllers clean.

Sql\create.sql
• Drops & recreates the four tables (`Product`, `Warehouse`, `[Order]`,
  `Product_Warehouse`) and seeds a little data — safe to run repeatedly.

Sql\proc.sql
• dbo.usp_AddProductToWarehouse — stored‑proc version of the six‑step logic.
• Returns 0 on success, 50000‑50004 on the same error conditions the tutorial
  specifies.

Program.cs
• Registers everything with the DI container (service, repository, middleware).
• Adds Swagger and minimal hosting code.
