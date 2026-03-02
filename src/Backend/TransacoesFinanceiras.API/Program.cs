using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using TransacoesFinanceiras.Application.Injections;
using TransacoesFinanceiras.Infrastructure.Database;
using TransacoesFinanceiras.Infrastructure.Injections;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console(new JsonFormatter())
    .WriteTo.File(
        new JsonFormatter(),
        path: "logs/pagueveloz-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "PagueVeloz - Transações Financeiras API",
        Version = "v1",
        Description = "API para processamento de transações financeiras",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "PagueVeloz",
            Email = "support@pagueveloz.com"
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Health Checks
builder.Services.AddHealthChecksUI(setup =>
{
    setup.SetEvaluationTimeInSeconds(10);
    setup.MaximumHistoryEntriesPerEndpoint(50);
}).AddInMemoryStorage();

// Métricas Prometheus

var app = builder.Build();

// Configurar pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PagueVeloz - Transações Financeiras API v1");
        c.RoutePrefix = "swagger"; // Swagger UI estará em /swagger
    });
}

//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Health Checks
app.MapHealthChecks("/health");
app.MapHealthChecksUI(options =>
{
    options.UIPath = "/health-ui";
});

// Métricas Prometheus
app.UseMetricServer();
app.UseHttpMetrics();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var context = services.GetRequiredService<AppDbContext>();

var retries = 5;
var delay = TimeSpan.FromSeconds(5);

while (retries > 0)
{
    try
    {
        Log.Information("Tentando aplicar migrações...");
        context.Database.Migrate();
        Log.Information("Migrações aplicadas com sucesso");
        break;
    }
    catch (Exception ex)
    {
        retries--;
        Log.Warning(ex, "Falha ao aplicar migrações. Tentativas restantes: {Retries}", retries);

        if (retries == 0)
        {
            Log.Warning("Não foi possível aplicar migrações após múltiplas tentativas.");
            break;
        }

        Thread.Sleep(delay);
    }
}
app.Run();
