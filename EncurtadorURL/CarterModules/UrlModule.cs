using Carter;
using System.Security.Cryptography;
using System.Text;
using StackExchange.Redis;
using Microsoft.AspNetCore.Http; // Para StatusCodes
using EncurtadorURL.DTOs; // Presumindo que você criou esta pasta

namespace EncurtadorURL.CarterModules;

public class UrlModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var urls = app.MapGroup("/urls");

        urls.MapPost("/", async (CreateShortUrlRequest req, IConnectionMultiplexer redis) =>
        {
            using var sha256 = SHA256.Create();
            var inputBytes = Encoding.UTF8.GetBytes(req.LongUrl);
            var hashBytes = sha256.ComputeHash(inputBytes);

            string chunk = Convert.ToBase64String(hashBytes).Replace("/", "-").Replace("+", "_").Substring(0, 8);

            IDatabase db = redis.GetDatabase();
            string baseUrl = "https://encurtadorurl-c3lm.onrender.com/";
            string shortUrlResult = $"{baseUrl}/{chunk}";

            RedisValue existingUrl = await db.StringGetAsync(chunk);

            if (existingUrl.IsNullOrEmpty)
            {
                await db.StringSetAsync(chunk, req.LongUrl, TimeSpan.FromDays(30));
            }

            return Results.Ok(new ShortUrlResponse(req.LongUrl, shortUrlResult));
        });

        app.MapGet("/{chunk}", async (string chunk, IConnectionMultiplexer redis) =>
        {
            IDatabase db = redis.GetDatabase();
            RedisValue longUrlValue = await db.StringGetAsync(chunk);

            if (longUrlValue.IsNullOrEmpty)
            {
                return Results.StatusCode(StatusCodes.Status404NotFound);
            }
            else
            {
                return Results.Redirect(longUrlValue.ToString());
            }
        }).ExcludeFromDescription(); 
    }
}

public record CreateShortUrlRequest(string LongUrl);
public record ShortUrlResponse(string LongUrl, string ShortUrl);