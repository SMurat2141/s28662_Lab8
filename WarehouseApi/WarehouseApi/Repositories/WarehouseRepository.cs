using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using WarehouseApi.Dtos;

namespace WarehouseApi.Repositories;

public sealed class WarehouseRepository(IConfiguration cfg) : IWarehouseRepository
{
    private readonly string _conn = cfg.GetConnectionString("DefaultConnection")!;

    public async Task<int?> AddProductAsync(WarehouseRequest r, CancellationToken ct)
    {
        await using var c = new SqlConnection(_conn);
        await c.OpenAsync(ct);
        await using var t = (SqlTransaction)await c.BeginTransactionAsync(IsolationLevel.Serializable, ct);

        try
        {
            if (!await ExistsAsync(c, t, "Product", "IdProduct", r.IdProduct, ct))  return null;
            if (!await ExistsAsync(c, t, "Warehouse", "IdWarehouse", r.IdWarehouse, ct)) return null;

            var (orderId, unitPrice) = await FindOrderAsync(c, t, r, ct);
            if (orderId is null) return null;

            if (await ExistsAsync(c, t,"Product_Warehouse","IdOrder",orderId.Value, ct))
                throw new InvalidOperationException("Order already fulfilled");

            var now = DateTime.UtcNow;

            await ExecAsync(c, t,
                "UPDATE [Order] SET FullfilledAt=@Now WHERE IdOrder=@Id",
                ct, ("@Now", now), ("@Id", orderId));

            var newId = (int)await ExecScalarAsync(c, t, """
                INSERT INTO Product_Warehouse
                    (IdWarehouse,IdProduct,IdOrder,Amount,Price,CreatedAt)
                OUTPUT INSERTED.IdProduct_Warehouse
                VALUES (@Wid,@Pid,@Oid,@Amt,@Price,@Now);
                """,
                ct,
                ("@Wid", r.IdWarehouse),
                ("@Pid", r.IdProduct),
                ("@Oid", orderId),
                ("@Amt", r.Amount),
                ("@Price", unitPrice*r.Amount),
                ("@Now", now));

            await t.CommitAsync(ct);
            return newId;
        }
        catch
        {
            if (t.Connection is not null) await t.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<int?> AddProductViaProcAsync(WarehouseRequest r, CancellationToken ct)
    {
        await using var c = new SqlConnection(_conn);
        await c.OpenAsync(ct);

        await using var cmd = new SqlCommand("dbo.usp_AddProductToWarehouse", c)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@IdProduct",   r.IdProduct);
        cmd.Parameters.AddWithValue("@IdWarehouse", r.IdWarehouse);
        cmd.Parameters.AddWithValue("@Amount",      r.Amount);
        cmd.Parameters.AddWithValue("@CreatedAt",   r.CreatedAt);
        var outId = cmd.Parameters.Add("@NewId", SqlDbType.Int);
        outId.Direction = ParameterDirection.Output;

        await cmd.ExecuteNonQueryAsync(ct);
        return (int?)outId.Value;
    }

    // -------- helpers (unchanged from previous message) --------
    private static async Task<bool> ExistsAsync(SqlConnection c, SqlTransaction t,
        string tbl, string pk, int id, CancellationToken ct)
    {
        var cmd = new SqlCommand($"SELECT 1 FROM {tbl} WHERE {pk}=@id", c, t);
        cmd.Parameters.AddWithValue("@id", id);
        return await cmd.ExecuteScalarAsync(ct) is not null;
    }

    private static async Task<(int? id, decimal price)> FindOrderAsync(
        SqlConnection c, SqlTransaction t, WarehouseRequest r, CancellationToken ct)
    {
        var cmd = new SqlCommand("""
            SELECT TOP 1 IdOrder, Price
            FROM   [Order]
            WHERE  IdProduct=@Pid AND Amount=@Amt AND CreatedAt<@Crt
            """, c, t);
        cmd.Parameters.AddWithValue("@Pid", r.IdProduct);
        cmd.Parameters.AddWithValue("@Amt", r.Amount);
        cmd.Parameters.AddWithValue("@Crt", r.CreatedAt);
        await using var dr = await cmd.ExecuteReaderAsync(ct);
        return await dr.ReadAsync(ct) ? (dr.GetInt32(0), dr.GetDecimal(1))
                                      : ((int?)null, 0m);
    }

    private static async Task<int> ExecAsync(SqlConnection c, SqlTransaction t,
        string sql, CancellationToken ct, params (string, object?)[] p)
    {
        var cmd = new SqlCommand(sql, c, t);
        foreach (var (n,v) in p) cmd.Parameters.AddWithValue(n, v ?? DBNull.Value);
        return await cmd.ExecuteNonQueryAsync(ct);
    }

    private static async Task<object> ExecScalarAsync(SqlConnection c, SqlTransaction t,
        string sql, CancellationToken ct, params (string, object?)[] p)
    {
        var cmd = new SqlCommand(sql, c, t);
        foreach (var (n,v) in p) cmd.Parameters.AddWithValue(n, v ?? DBNull.Value);
        return (await cmd.ExecuteScalarAsync(ct))!;
    }
}
