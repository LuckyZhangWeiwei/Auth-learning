using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

const string AuthScheme = "cookie";
const string AuthScheme2 = "cookie2";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(AuthScheme).AddCookie(AuthScheme).AddCookie(AuthScheme2);

var app = builder.Build();

app.UseAuthentication();

app.Use(
    (ctx, next) =>
    {
        if(ctx.Request.Path.StartsWithSegments("/login"))
        {
            return next();
        }

        if (!ctx.User.Identities.Any(x => x.AuthenticationType == AuthScheme))
        {
            ctx.Response.StatusCode = 401;
            return Task.CompletedTask;
        }

        if (!ctx.User.HasClaim("passport_type", "eur"))
        {
            ctx.Response.StatusCode = 403;
            return Task.CompletedTask;
        }

        return next();
    }
);

app.MapGet(
    "/unsecure",
    (HttpContext ctx) =>
    {
        return ctx.User.FindFirst("usr")?.Value ?? "empty";
    }
);

app.MapGet(
    "/sweden",
    (HttpContext ctx) =>
    {
        //if (!ctx.User.Identities.Any(x => x.AuthenticationType == AuthScheme))
        //{
        //    ctx.Response.StatusCode = 401;
        //    return "";
        //}
        //if (!ctx.User.HasClaim("passport_type", "eur"))
        //{
        //    ctx.Response.StatusCode = 403;
        //    return "";
        //}
        return "allowed eur";
    }
);

app.MapGet(
    "/norway",
    (HttpContext ctx) =>
    {
        //if (!ctx.User.Identities.Any(x => x.AuthenticationType == AuthScheme))
        //{
        //    ctx.Response.StatusCode = 401;
        //    return "";
        //}
        //if (!ctx.User.HasClaim("passport_type", "NOR"))
        //{
        //    ctx.Response.StatusCode = 403;
        //    return "";
        //}
        return "allowed eur";
    }
);

app.MapGet(
    "/demark",
    (HttpContext ctx) =>
    {
        //if (!ctx.User.Identities.Any(x => x.AuthenticationType == AuthScheme2))
        //{
        //    ctx.Response.StatusCode = 401;
        //    return "";
        //}
        //if (!ctx.User.HasClaim("passport_type", "eur"))
        //{
        //    ctx.Response.StatusCode = 403;
        //    return "";
        //}
        return "allowed eur";
    }
);

app.MapGet(
    "/login",
    async (HttpContext ctx) =>
    {
        var claims = new List<Claim>();
        claims.Add(new Claim("usr", "anton"));
        claims.Add(new Claim("passport_type", "eur"));
        var identity = new ClaimsIdentity(claims, AuthScheme);

        var user = new ClaimsPrincipal(identity);
        await ctx.SignInAsync(AuthScheme, user);
    }
);

app.Run();
