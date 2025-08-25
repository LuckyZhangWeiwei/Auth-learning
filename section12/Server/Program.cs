using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

var resKey = RSA.Create();

resKey.ImportRSAPrivateKey(File.ReadAllBytes("key"), out _);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("jwt")
    .AddJwtBearer("jwt", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateAudience = false,
            ValidateIssuer = false,
        };
        options.Events = new JwtBearerEvents()
        {
            OnMessageReceived = ctx =>
            {
                if (ctx.Request.Query.ContainsKey("t"))
                {
                    ctx.Token = ctx.Request.Query["t"];
                }

                return Task.CompletedTask;
            }
        };

        options.Configuration = new OpenIdConnectConfiguration()
        {
            SigningKeys =
            {
                new RsaSecurityKey(resKey)
            }
        };

        options.MapInboundClaims = false;
    });

// Add services to the container.
var app = builder.Build();

app.UseAuthentication();

app.MapGet("/", (HttpContext ctx) => ctx.User.FindFirst("sub")?.Value ?? "empty");

app.MapGet("/jwt", () =>
{
    var key = new RsaSecurityKey(resKey);
    var handler = new JsonWebTokenHandler();
    var token = handler.CreateToken(new SecurityTokenDescriptor()
    {
        Issuer = "https://localhost:5000",
        Subject = new ClaimsIdentity(
            [
                new Claim("sub", Guid.NewGuid().ToString()),
                new Claim("name", "Anton")
            ]
        ),
        SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256)
    });

    return token;
});

app.MapGet(
    "/jwk",
    () =>
    {
        var publicKey = RSA.Create();

        publicKey.ImportRSAPublicKey(resKey.ExportRSAPublicKey(), out _);

        var key = new RsaSecurityKey(publicKey);

        return JsonWebKeyConverter.ConvertFromRSASecurityKey(key);
    }
);

app.MapGet(
    "/jwk-private",
    () =>
    {
        var key = new RsaSecurityKey(resKey);

        return JsonWebKeyConverter.ConvertFromRSASecurityKey(key);
    }
);

// Configure the HTTP request pipeline.

app.Run();