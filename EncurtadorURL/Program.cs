using Carter;
using EncurtadorURL;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Configuração
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

// Redis - PRIORIDADE para variável de ambiente
var redisConnectionString = builder.Configuration.GetValue<string>("REDIS_CONNECTION_STRING")
                         ?? builder.Configuration.GetConnectionString("Redis")
                         ?? "localhost:6379";

if (string.IsNullOrEmpty(redisConnectionString) || redisConnectionString == "localhost:6379")
{
    Console.WriteLine($"⚠️  AVISO: Redis usando default localhost:6379 - Configure REDIS_CONNECTION_STRING para produção");
}

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddCarter();

// Configure Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse(redisConnectionString);
    configuration.ConnectTimeout = 10000;
    configuration.SyncTimeout = 10000;
    configuration.AbortOnConnectFail = false;
    configuration.ReconnectRetryPolicy = new LinearRetry(5000);

    Console.WriteLine($"🔗 Conectando ao Redis: {redisConnectionString}");
    return ConnectionMultiplexer.Connect(configuration);
});

// CORS configuration - MANTENHA como está
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("AppSettings:AllowedOrigins").Get<string[]>()
                          ?? new[] { "http://localhost:3000", "https://localhost:5000" };

        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});

var app = builder.Build();

// Middleware - MANTENHA como está
app.UseRouting();
app.UseCors();

// Handle preflight requests
app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        context.Response.Headers.Add("Access-Control-Allow-Origin",
            context.Request.Headers["Origin"]);
        context.Response.Headers.Add("Access-Control-Allow-Methods",
            "GET, POST, PUT, DELETE, OPTIONS");
        context.Response.Headers.Add("Access-Control-Allow-Headers",
            "Content-Type, Authorization, X-Requested-With, Origin, Accept");
        context.Response.StatusCode = 200;
        await context.Response.CompleteAsync();
        return;
    }
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapCarter();

app.MapGet("/health", () =>
{
    var redisStatus = "Unknown";
    try
    {
        var redis = app.Services.GetService<IConnectionMultiplexer>();
        redisStatus = redis?.IsConnected == true ? "Connected" : "Disconnected";
    }
    catch
    {
        redisStatus = "Error";
    }

    return Results.Ok(new
    {
        status = "Healthy",
        timestamp = DateTime.UtcNow,
        redis = redisStatus
    });
}).WithTags("Health");

app.Run();