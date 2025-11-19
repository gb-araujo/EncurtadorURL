using Carter;

public class PagesModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (HttpContext ctx) =>
        {
            ctx.Response.ContentType = "text/html";
            ctx.Response.StatusCode = 200;
            await ctx.Response.SendFileAsync("wwwroot/index.html");
        });
    }
}