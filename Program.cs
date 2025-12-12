using Microsoft.EntityFrameworkCore;
using TEXTILJOC_ConcarWeb.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Registro del DbContext para SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddControllers();

// Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseCors("AllowAll"); // La ubicación de esta línea es crítica

app.UseAuthorization();

app.MapControllers();

app.Run();
