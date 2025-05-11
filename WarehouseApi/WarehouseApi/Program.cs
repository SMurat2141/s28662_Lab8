using WarehouseApi.Repositories;
using WarehouseApi.Services;
using WarehouseApi.Middleware;
using WarehouseApi.Contracts;

var b = WebApplication.CreateBuilder(args);
b.Services.AddControllers();
b.Services.AddEndpointsApiExplorer();
b.Services.AddSwaggerGen();
b.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
b.Services.AddScoped<IWarehouseService, WarehouseService>();

var app = b.Build();
app.UseMiddleware<ErrorHandlingMiddleware>();
if (app.Environment.IsDevelopment()) app.UseSwagger().UseSwaggerUI();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();