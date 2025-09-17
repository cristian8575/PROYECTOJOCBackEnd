//var builder = WebApplication.CreateBuilder(args);



//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll", policy =>
//    {
//        policy.AllowAnyOrigin()
//              .AllowAnyHeader()
//              .AllowAnyMethod();

//    });
//});

//// Add services to the container.
//builder.Services.AddControllers();    // <- necesario
//builder.Services.AddOpenApi();

//var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//    app.MapOpenApi();
//}

//app.UseHttpsRedirection();

//app.UseCors("AllowAll");

//app.UseAuthorization();              // <- opcional pero típico

//app.MapControllers();                // <- necesario

//app.Run();
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
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
app.UseHttpsRedirection();
app.UseCors("AllowAll"); // La ubicación de esta línea es crítica
app.UseAuthorization();
app.MapControllers();
app.Run();