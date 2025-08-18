using System.Security.Claims;
using IdentityManagement;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddDefaultTokenProviders();

// Add services to the container.
builder
    .Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);
builder.Services.AddAuthorization(b =>
{
    b.AddPolicy(
        "manager",
        pb =>
        {
            pb.RequireAuthenticatedUser()
                .AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme)
                .RequireClaim("role", "manager");
        }
    );
});
builder.Services.AddSingleton<Database>();
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddDataProtection();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "hello world");

app.MapGet("/protected", () => "sth super secret").RequireAuthorization("manager");

app.MapGet("/test", (SignInManager<IdentityUser> signMgr, UserManager<IdentityUser> userMgr) => {
});

app.MapGet(
    "/register",
    async (
        string username,
        string password,
        IPasswordHasher<User> hasher,
        Database db,
        HttpContext ctx
    ) =>
    {
        var user = new User() { Username = username };
        user.PasswordHash = hasher.HashPassword(user, password);
        await db.PutAsync(user);
        await ctx.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            UserHelper.Convert(user)
        );
        return user;
    }
);

app.MapGet(
    "/login",
    async (
        string username,
        string password,
        IPasswordHasher<User> hasher,
        Database db,
        HttpContext ctx
    ) =>
    {
        var user = await db.GetUserAsync(username);

        if (user == null)
            return "username error";

        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);

        if (result == PasswordVerificationResult.Failed)
        {
            return "bad credentials";
        }

        await ctx.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            UserHelper.Convert(user)
        );
        return "loggin";
    }
);

app.MapGet(
    "/promote",
    async (string username, Database db) =>
    {
        var user = await db.GetUserAsync(username);

        if (user == null)
            return "username error";

        user.Claims.Add(new UserClaim { Type = "role", Value = "manager" });

        if (user == null)
            return "username error";
        await db.PutAsync(user);

        return "promoted";
    }
);

app.MapGet(
    "/start-password-reset",
    async (string username, Database db, IDataProtectionProvider provider) =>
    {
        var protector = provider.CreateProtector("PasswordReset");

        var user = await db.GetUserAsync(username);

        return protector.Protect(user.Username);
    }
);

app.MapGet(
    "/end-password-reset",
    async (string username, string password, string hash, Database db, IPasswordHasher<User> hasher,
        IDataProtectionProvider provider) =>
    {
        var protector = provider.CreateProtector("PasswordReset");
        var hashUsername = protector.Unprotect(hash);

        if (hashUsername != username)
        {
            return "username error";
        }

        var user = await db.GetUserAsync(username);
        user.PasswordHash = hasher.HashPassword(user, password);
        await db.PutAsync(user);

        return "passowrd reset";
    }
);

app.Run();

public class UserHelper
{
    public static ClaimsPrincipal Convert(User user)
    {
        var claims = new List<Claim>() { new Claim("username", user.Username) };

        claims.AddRange(user.Claims.Select(x => new Claim(x.Type, x.Value)));

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        return new ClaimsPrincipal(identity);
    }
}