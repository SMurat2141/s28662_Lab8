-- WarehouseApi/Sql/proc.sql
IF OBJECT_ID('dbo.usp_AddProductToWarehouse', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_AddProductToWarehouse;
GO

CREATE PROCEDURE dbo.usp_AddProductToWarehouse
    @IdProduct     INT,
    @IdWarehouse   INT,
    @Amount        INT,
    @CreatedAt     DATETIME,
    @NewId         INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRAN;

        /* 1 ─ validations */
        IF NOT EXISTS (SELECT 1 FROM Product WHERE IdProduct = @IdProduct)
            THROW 50000, 'Product not found', 1;
        IF NOT EXISTS (SELECT 1 FROM Warehouse WHERE IdWarehouse = @IdWarehouse)
            THROW 50001, 'Warehouse not found', 1;
        IF @Amount <= 0
            THROW 50002, 'Amount must be greater than zero', 1;

        /* 2 ─ matching purchase order */
        DECLARE @OrderId   INT,
            @UnitPrice DECIMAL(18,2);

        SELECT TOP 1
            o.IdOrder,
            p.Price      -- from Product
        FROM   [Order]  o
                   JOIN   Product p ON p.IdProduct = o.IdProduct
        WHERE  o.IdProduct = @IdProduct
          AND  o.Amount    = @Amount
          AND  o.CreatedAt < @CreatedAt;

        IF @OrderId IS NULL
            THROW 50003, 'Matching order not found', 1;

        /* 3 ─ already fulfilled? */
        IF EXISTS (SELECT 1 FROM Product_Warehouse WHERE IdOrder = @OrderId)
            THROW 50004, 'Order already fulfilled', 1;

        /* 4 ─ fulfil order */
        UPDATE [Order]
        SET    FulfilledAt = SYSUTCDATETIME()
        WHERE  IdOrder     = @OrderId;

        /* 5 ─ insert row */
        INSERT Product_Warehouse
        (IdWarehouse, IdProduct, IdOrder,
         Amount, Price, CreatedAt)
        VALUES(@IdWarehouse, @IdProduct, @OrderId,
               @Amount, @UnitPrice * @Amount, SYSUTCDATETIME());

        /* 6 ─ return PK */
        SET @NewId = SCOPE_IDENTITY();

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;
        THROW;
    END CATCH
END;
GO
