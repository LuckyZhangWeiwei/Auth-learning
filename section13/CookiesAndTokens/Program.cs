using System.Security.Claims;
using CookiesAndTokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var keyManager_ = new KeyManager();

builder.Services.AddSingleton(keyManager_);

builder.Services.AddDbContext<IdentityDbContext>(c => c.UseInMemoryDatabase("my_db"));

builder
    .Services.AddIdentity<IdentityUser, IdentityRole>(o =>
    {
        o.User.RequireUniqueEmail = false;
        o.Password.RequireDigit = false;
        o.Password.RequiredLength = 4;
        o.Password.RequireUppercase = false;
        o.Password.RequireLowercase = false;
        o.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<IdentityDbContext>()
    .AddDefaultTokenProviders();

builder
    .Services.AddAuthentication()
    .AddJwtBearer(
        "jwt",
        o =>
        {
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
            };

            o.Events = new JwtBearerEvents
            {
                OnMessageReceived = (ctx) =>
                {
                    if (ctx.Request.Query.TryGetValue("t", out var token))
                    {
                        ctx.Token = token;
                    }

                    return Task.CompletedTask;
                }
            };

            o.Configuration = new OpenIdConnectConfiguration()
            {
                SigningKeys = { new RsaSecurityKey(keyManager_.RSAKey) },
            };

            o.MapInboundClaims = false;
        }
    );

builder.Services.AddAuthorization(b =>
{
    b.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .AddAuthenticationSchemes(IdentityConstants.ApplicationScheme, "jwt")
        .Build();

    b.AddPolicy(
        "the_policy",
        pb =>
            pb.RequireAuthenticatedUser()
                .AddAuthenticationSchemes(IdentityConstants.ApplicationScheme, "jwt")
                .RequireClaim("role", "janitor")
    );
    b.AddPolicy(
        "cookie_policy",
        pb =>
            pb.RequireAuthenticatedUser()
                .AddAuthenticationSchemes(IdentityConstants.ApplicationScheme)
                .RequireClaim("role", "janitor")
    );

    b.AddPolicy(
        "token_policy",
        pb =>
            pb.RequireAuthenticatedUser()
                .AddAuthenticationSchemes("jwt")
                .RequireClaim("role", "janitor")
    );
});

var app = await builder.BuildAndSetup();

app.MapGet(
        "/",
        (ClaimsPrincipal user) => user.Claims.Select(x => KeyValuePair.Create(x.Type, x.Value))
    )
    .RequireAuthorization();

app.MapGet("/secret", () => "secret").RequireAuthorization("the_policy");

app.MapGet("/secret-cookie", () => "secret-cookie").RequireAuthorization("cookie_policy");

app.MapGet("/secret-token", () => "secret-token").RequireAuthorization("token_policy");

app.MapGet(
    "/cookie/sign-in",
    async (SignInManager<IdentityUser> signInManager) =>
    {
        var result = await signInManager.PasswordSignInAsync(
            "test@test.com",
            "password",
            false,
            false
        );

        return Results.Ok(result);
    }
);

app.MapGet(
    "/jwt/sign-in",
    async (
        KeyManager keyManager,
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager,
        IUserClaimsPrincipalFactory<IdentityUser> claimsPrincipalFactory
    ) =>
    {
        var user = await userManager.FindByNameAsync("test@test.com");

        var result = await signInManager.CheckPasswordSignInAsync(user!, "password", false);

        var principal = await claimsPrincipalFactory.CreateAsync(user!);

        var identity = principal.Identities.First();

        identity.AddClaim(new Claim("arm", "pwd"));
        
        identity.AddClaim(new Claim("method", "jwt"));

        var handler = new JsonWebTokenHandler();

        var key = new RsaSecurityKey(keyManager.RSAKey);

        var token = handler.CreateToken(
            new SecurityTokenDescriptor()
            {
                Issuer = "https://localhost:7100",
                Subject = identity,
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256),
            }
        );

        return token;
    }
);

app.Run();