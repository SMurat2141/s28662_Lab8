using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace WarehouseApi.Middleware;
// … rest unchanged …


public sealed class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> log)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (SqlException ex)
        {
            ctx.Response.StatusCode = ex.Number switch
            {
                50000 or 50001 => (int)HttpStatusCode.NotFound,
                50002 or 50003 => (int)HttpStatusCode.BadRequest,
                50004         => (int)HttpStatusCode.Conflict,
                _             => (int)HttpStatusCode.InternalServerError
            };
            await ctx.Response.WriteAsync(ex.Message);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await ctx.Response.WriteAsync(ex.Message);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Unhandled exception");
            ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await ctx.Response.WriteAsync("Unexpected error");
        }
    }
}