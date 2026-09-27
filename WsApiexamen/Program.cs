using Microsoft.EntityFrameworkCore;
using WsApiexamen.Data;
using WsApiexamen.Services;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Entity Framework + SQL Server
builder.Services.AddDbContext<BdiExamenContext>(opciones =>
 opciones.UseSqlServer(builder.Configuration.GetConnectionString("BdiExamen")));
builder.Services.AddScoped<ExamenService>();
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
