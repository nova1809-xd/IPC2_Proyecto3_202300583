using BackendAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// registra los controladores de la API
builder.Services.AddControllers();

// registra el servicio singleton XmlDatabaseService para persistencia
// singleton asegura que solo exista una instancia durante toda la vida de la aplicación
builder.Services.AddSingleton<XmlDatabaseService>(provider => XmlDatabaseService.GetInstance());

// registra los servicios de negocio con ciclo de vida transient
// esto significa que se crea una nueva instancia cada vez que se solicita
builder.Services.AddTransient<TransaccionesConfiguracionService>();
builder.Services.AddTransient<TransaccionesServicio>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// mapea los controladores
app.MapControllers();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
