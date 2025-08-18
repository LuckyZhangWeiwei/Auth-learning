using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder
    .Services.AddAuthentication().AddCookie("default",
        o =>
        {
            o.Cookie.Name = "mycookie";
            // o.Cookie.Domain = "mydomain.com";
            // o.Cookie.Path = "/test";
            // o.Cookie.HttpOnly = false;
            //o.Cookie.SameSite = SameSiteMode.Lax;
            o.ExpireTimeSpan = TimeSpan.FromSeconds(10);
        });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Hello World!");

app.MapGet("test", ()=> "Hello World!").RequireAuthorization();

app.MapPost(
    "/login",
    async (HttpContext ctx) =>
    {
        await ctx.SignInAsync(
            "default",
            new ClaimsPrincipal(
                new ClaimsIdentity(
                    [new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),],
                    "default"
                )
            ),
            new AuthenticationProperties() { IsPersistent = true }
        );

        return "Ok";
    }
);

app.MapDefaultControllerRoute();

app.Run();