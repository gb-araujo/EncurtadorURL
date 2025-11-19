using Carter;
using StackExchange.Redis;


var builder = WebApplication.CreateBuilder(args);

const string RedisConnectionStringName = "REDIS_CONNECTION_STRING";
var redisConnectionString = builder.Configuration.GetValue<string>(RedisConnectionStringName);

if (string.IsNullOrEmpty(redisConnectionString))
{
    // aviso
    Console.WriteLine($"AVISO: Usando o default localhost:6379, defina {RedisConnectionStringName} para produção.");
    redisConnectionString = "localhost:6379";
}

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddCarter();
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisConnectionString));

// configuração do CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontEnd", 
        builder =>
        {
            builder.WithOrigins("https://encurtador-omega.vercel.app", 
                                "https://encurtadorurl-c3lm.onrender.com", // URL do Render
                                "http://127.0.0.1:5500")           // Localhost
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});


var app = builder.Build();
app.UseCors("PermitirFrontEnd");


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}



app.MapCarter();

app.UseHttpsRedirection();


app.Run();