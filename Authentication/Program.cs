using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication("cookie").AddCookie("cookie");

var app = builder.Build();

app.UseAuthentication();

app.MapGet(
    "/username",
    (HttpContext ctx) =>
    {
        return ctx.User.FindFirst("usr")!.Value;
    }
);

app.MapGet(
    "/login",
    async (HttpContext ctx) =>
    {
        var claims = new List<Claim>();
        claims.Add(new Claim("usr", "anton"));
        var identity = new ClaimsIdentity(claims, "cookie");
        var user = new ClaimsPrincipal(identity);

        await ctx.SignInAsync("cookie", user);
        return "ok";
    }
);

app.Run();
