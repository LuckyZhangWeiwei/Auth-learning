using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

const string AuthScheme = "cookie";
const string AuthScheme2 = "cookie2";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(AuthScheme).AddCookie(AuthScheme).AddCookie(AuthScheme2);

builder.Services.AddAuthorization(builder =>
{
    builder.AddPolicy(
        "eu passport",
        pb =>
        {
            pb.RequireAuthenticatedUser()
                .AddAuthenticationSchemes([AuthScheme])
                .AddRequirements(new MyRequirement())
                .RequireClaim("passport_type", "eur");
        }
    );
});

builder.Services.AddSingleton<IAuthorizationHandler, MyRequirementHandler>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

//app.Use(
//    (ctx, next) =>
//    {
//        if(ctx.Request.Path.StartsWithSegments("/login"))
//        {
//            return next();
//        }

//        if (!ctx.User.Identities.Any(x => x.AuthenticationType == AuthScheme))
//        {
//            ctx.Response.StatusCode = 401;
//            return Task.CompletedTask;
//        }

//        if (!ctx.User.HasClaim("passport_type", "eur"))
//        {
//            ctx.Response.StatusCode = 403;
//            return Task.CompletedTask;
//        }

//        return next();
//    }
//);

app.MapGet(
        "/unsecure",
        (HttpContext ctx) =>
        {
            return ctx.User.FindFirst("usr")?.Value ?? "empty";
        }
    )
    .RequireAuthorization("eu passport");

app.MapGet(
    "/sweden",
    [Authorize(Policy = "eu passport")]
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
    )
    .AllowAnonymous();

app.Run();

public class MyRequirement : IAuthorizationRequirement { }

public class MyRequirementHandler : AuthorizationHandler<MyRequirement>
{
    public MyRequirementHandler() { }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MyRequirement requirement
    )
    {
        var identity = context.User.Identities.FirstOrDefault(i =>
            i.AuthenticationType == "cookie"
        );

        if (identity != null && identity.HasClaim("passport_type", "eur"))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail(new AuthorizationFailureReason(this, "no access"));
        }

        return Task.CompletedTask;
    }
}
