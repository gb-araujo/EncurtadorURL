using Carter;
using System.Security.Cryptography;
using System.Text;
using StackExchange.Redis;
using Microsoft.Extensions.Options;
using EncurtadorURL.DTOs;

namespace EncurtadorURL.CarterModules;

public class UrlModule : CarterModule
{
    private const int MaxUrlLength = 2048;
    private const int MaxCollisionAttempts = 5;
    private static readonly TimeSpan UrlTtl = TimeSpan.FromDays(30);

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var urls = app.MapGroup("/urls/");

        urls.MapPost("/", async (CreateShortUrlRequest req, IConnectionMultiplexer redis, IOptions<AppSettings> appSettings) =>
        {
            if (!TryNormalizeUrl(req.LongUrl, out string longUrl))
            {
                return Results.BadRequest(new
                {
                    message = $"URL inválida. Apenas URLs http/https com até {MaxUrlLength} caracteres são aceitas."
                });
            }

            IDatabase db = redis.GetDatabase();
            string baseUrl = appSettings.Value.BaseUrl.TrimEnd('/');

            // Colisões são raras com 8 caracteres, mas possíveis: se o código já
            // pertence a outra URL, gera um novo código em vez de devolver o link errado.
            for (int attempt = 0; attempt < MaxCollisionAttempts; attempt++)
            {
                string chunk = GenerateChunk(longUrl, attempt);
                string shortUrl = $"{baseUrl}/{chunk}";

                RedisValue existingUrl = await db.StringGetAsync(chunk);

                if (existingUrl.IsNullOrEmpty)
                {
                    await db.StringSetAsync(chunk, longUrl, UrlTtl);
                    return Results.Created(shortUrl, new ShortUrlResponse(longUrl, shortUrl));
                }

                if (existingUrl == longUrl)
                {
                    return Results.Ok(new ShortUrlResponse(longUrl, shortUrl));
                }
            }

            return Results.Problem(
                detail: "Não foi possível gerar um código único para esta URL. Tente novamente.",
                statusCode: StatusCodes.Status500InternalServerError);
        }).RequireRateLimiting("shorten");

        app.MapGet("/{chunk}", async (string chunk, IConnectionMultiplexer redis) =>
        {
            IDatabase db = redis.GetDatabase();
            RedisValue longUrlValue = await db.StringGetAsync(chunk);

            if (longUrlValue.IsNullOrEmpty)
            {
                return Results.NotFound();
            }

            return Results.Redirect(longUrlValue.ToString());
        }).ExcludeFromDescription();
    }

    // Validação no servidor: o cliente também valida, mas nunca é confiável.
    // Sem isso o serviço aceita qualquer string e vira um open redirect.
    private static bool TryNormalizeUrl(string? input, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(input) || input.Length > MaxUrlLength)
            return false;

        if (!Uri.TryCreate(input.Trim(), UriKind.Absolute, out Uri? uri))
            return false;

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            return false;

        normalized = uri.ToString();
        return true;
    }

    private static string GenerateChunk(string url, int attempt)
    {
        // attempt 0 mantém o formato original (hash puro da URL) para
        // preservar os códigos já emitidos antes desta mudança.
        string input = attempt == 0 ? url : $"{url}:{attempt}";
        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));

        return Convert.ToBase64String(hashBytes)
            .Replace("/", "-")
            .Replace("+", "_")[..8];
    }
}
