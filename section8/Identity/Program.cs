using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder
    .Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

builder.Services.AddAuthorization();

// Add services to the container.
var app = builder.Build();

// Configure the HTTP request pipeline.



app.UseAuthorization();

app.MapGet("/", () => "Hello World! Identity");

app.MapGet(
    "/login",
    async (HttpContext ctx) =>
    {
        await ctx.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(
                new ClaimsIdentity(
                    [new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),],
                    CookieAuthenticationDefaults.AuthenticationScheme
                )
            )
        );

        return "Ok";
    }
);

app.MapGet("/protected", () => "secret").RequireAuthorization();

app.Run();