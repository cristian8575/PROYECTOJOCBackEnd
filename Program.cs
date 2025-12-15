using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
// Si usas Entity Framework, descomenta la siguiente línea y asegúrate de tener el namespace correcto
// using Microsoft.EntityFrameworkCore;
// using TextilesJocApi.Data; 

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------------------------------------------
// 1. CONFIGURACIÓN DE SERVICIOS (Lo que tu app puede hacer)
// -------------------------------------------------------------------------

// A. Controladores (Vital para que no salga error 404)
builder.Services.AddControllers();

// B. Swagger / OpenAPI (Para que veas la pantalla azul con los endpoints)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TextilesJoc API", Version = "v1" });
});

// C. CORS (Para que React pueda conectarse sin bloqueos)
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirReact", policy =>
    {
        // Agrega aquí los puertos donde corre tu React (Frontend)
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// D. Autenticación JWT (Necesario para tu AuthController)
var key = builder.Configuration["Jwt:Key"] ?? "ClaveSecreta_Super_Segura_123456789"; // Clave por defecto si no está en appsettings
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "TextilesJocApi",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "TextilesJocFront",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

// E. Base de Datos (Opcional: Solo si usas Entity Framework. Si usas SqlConnection directo, esto no es estricto)
// var connectionString = builder.Configuration.GetConnectionString("CadenaSQL");
// builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

var app = builder.Build();

// -------------------------------------------------------------------------
// 2. PIPELINE HTTP (El orden aquí es CRÍTICO)
// -------------------------------------------------------------------------


app.UseDefaultFiles(); // Habilita buscar index.html
app.UseStaticFiles();  // Habilita servir archivos JS/CSS

// A. Swagger (Lo ponemos fuera del 'if Development' para que te salga sí o sí ahora)
app.UseSwagger();
app.UseSwaggerUI();

// B. CORS (Antes de seguridad)
app.UseCors("PermitirReact");

// C. Redirección HTTPS (A veces da problemas en local, coméntalo si falla la conexión)
// app.UseHttpsRedirection(); 

// D. Seguridad (Primero Autenticación, luego Autorización)
app.UseAuthentication();
app.UseAuthorization();

// E. Mapeo de rutas (¡ESTO SOLUCIONA EL 404!)
app.MapControllers();

app.Run();