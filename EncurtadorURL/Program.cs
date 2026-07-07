using Carter;
using EncurtadorURL;
using Microsoft.AspNetCore.RateLimiting;
using StackExchange.Redis;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

var redisConnectionString = GetRedisConnectionString(builder);
Console.WriteLine($"🔗 Redis: {redisConnectionString}");

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddCarter();

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    try
    {
        Console.WriteLine($"🔄 Conectando ao Redis: {redisConnectionString}");

        var configuration = ConfigurationOptions.Parse(redisConnectionString);
        configuration.ConnectTimeout = 5000;
        configuration.SyncTimeout = 5000;
        configuration.AbortOnConnectFail = false;

        var redis = ConnectionMultiplexer.Connect(configuration);
        Console.WriteLine("✅ Redis conectado!");
        return redis;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ ERRO Redis: {ex.Message}");
        if (builder.Environment.IsDevelopment())
        {
            Console.WriteLine("💡 Dica para desenvolvimento:");
            Console.WriteLine("   - Execute: docker run -d -p 6379:6379 redis:alpine");
            Console.WriteLine("   - Ou configure em appsettings.Development.json:");
            Console.WriteLine("     \"ConnectionStrings\": { \"Redis\": \"localhost:6379\" }");
        }
        throw;
    }
});

// CORS: origens vêm da configuração (AppSettings:AllowedOrigins),
// permitindo ajustar por ambiente sem recompilar.
var allowedOrigins = builder.Configuration
    .GetSection("AppSettings:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Rate limiting: o endpoint de criação é público e de escrita,
// então limita por IP para evitar abuso.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("shorten", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1)
            }));
});

var app = builder.Build();

// Middleware
app.UseRouting();
app.UseCors();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapCarter();

// Health check
app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName
}));

app.Run();

static string GetRedisConnectionString(WebApplicationBuilder builder)
{

    var envConnectionString = builder.Configuration.GetValue<string>("REDIS_CONNECTION_STRING");
    if (!string.IsNullOrEmpty(envConnectionString))
    {
        Console.WriteLine("📦 Usando Redis de variável de ambiente");
        return envConnectionString;
    }

    var configConnectionString = builder.Configuration.GetConnectionString("Redis");
    if (!string.IsNullOrEmpty(configConnectionString))
    {
        Console.WriteLine("📁 Usando Redis de appsettings.json");
        return configConnectionString;
    }

    // Fallback desenvolvimento
    if (builder.Environment.IsDevelopment())
    {
        Console.WriteLine("🔧 Desenvolvimento: usando localhost:6379");
        return "localhost:6379";
    }

    throw new Exception("❌ String de conexão Redis não configurada");
}