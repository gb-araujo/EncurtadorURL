using Carter;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

const string RedisConnectionStringName = "REDIS_CONNECTION_STRING";
var redisConnectionString = builder.Configuration.GetValue<string>(RedisConnectionStringName);

if (string.IsNullOrEmpty(redisConnectionString))
{
    Console.WriteLine($"AVISO: Usando o default localhost:6379, defina {RedisConnectionStringName} para produção.");
    redisConnectionString = "localhost:6379";
}

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddCarter();

// Configure Redis with better settings
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse(redisConnectionString);
    configuration.ConnectTimeout = 10000;
    configuration.SyncTimeout = 10000;
    configuration.AbortOnConnectFail = false;
    configuration.ReconnectRetryPolicy = new LinearRetry(5000);
    return ConnectionMultiplexer.Connect(configuration);
});

// Enhanced CORS configuration
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            "https://encurtador-omega.vercel.app",
            "https://encurtadorurl-c3lm.onrender.com",
            "http://127.0.0.1:5500",
            "http://localhost:3000",
            "http://localhost:8080",
            "http://localhost:5500"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()
        .SetPreflightMaxAge(TimeSpan.FromMinutes(10));

        // For more permissive setup (use carefully):
        // policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// CRITICAL: Correct middleware order
app.UseRouting();

// CORS must come after Routing, before Authentication/Authorization
app.UseCors();

// Handle preflight requests globally
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

// Optional: Add health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }))
   .WithTags("Health");

// Comment out HTTPS redirection temporarily for testing
// app.UseHttpsRedirection();

app.Run();