using Carter;
using EncurtadorURL;
using StackExchange.Redis;

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

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            "http://127.0.0.1:5500",    // Live Server
            "http://localhost:3000",    // React/Vue
            "http://localhost:8080",    // Vite
            "http://localhost:5500",    // Outro Live Server
            "https://encurtador-omega.vercel.app"  // Seu frontend produção
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

var app = builder.Build();

// Middleware
app.UseRouting();
app.UseCors();

app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        context.Response.Headers.Add("Access-Control-Allow-Origin", context.Request.Headers["Origin"]);
        context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
        context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, X-Requested-With, Origin, Accept");
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

    // 3. Fallback desenvolvimento
    if (builder.Environment.IsDevelopment())
    {
        Console.WriteLine("🔧 Desenvolvimento: usando localhost:6379");
        return "localhost:6379";
    }

    throw new Exception("❌ String de conexão Redis não configurada");
}