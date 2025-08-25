using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

var jwkString =
    "{\"e\":\"AQAB\",\"key_ops\":[],\"kty\":\"RSA\",\"n\":\"u4Whe6hsEFALoeV2NVnnW9A1A7eRQx5ZnX5AlYMS0gTyxrkV3XqY64vIiWhuBTumNHmuOhWhVa2Ai71e5e_ipo5fU8AMqnnvwru36mUadhz6NZbJBh90LU_EcA5cWsOleLc_CavIXQ--ovy9uT-a-bA1BtD4PrDi1tCBdpp2hBMMHD7-dqzMozi69_zZcM7lAhQKb_JicIcUHLPL4Yh54FJ7W0Ghn1UW5eYcbbQQDqeuweTvqfU8_6iJd1VQjgFGk6Fo3Gw-a9fikW4RP5Esot4qbTZawdWI1l0oQM222qnlF-Nk_VfZqzQLV3pPAuT4f1tF23e9ZtbrfyiO7292ZQ\",\"oth\":[],\"x5c\":[]}";
var builder = WebApplication.CreateBuilder(args);

builder
    .Services.AddAuthentication("jwt")
    .AddJwtBearer(
        "jwt",
        o =>
        {
            o.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateAudience = false,
                ValidateIssuer = false,
            };

            o.Events = new JwtBearerEvents()
            {
                OnMessageReceived = (ctx) =>
                {
                    if (ctx.Request.Query.ContainsKey("t"))
                    {
                        ctx.Token = ctx.Request.Query["t"];
                    }

                    return Task.CompletedTask;
                }
            };

            o.Configuration = new OpenIdConnectConfiguration()
            {
                SigningKeys = { JsonWebKey.Create(jwkString) }
            };

            o.MapInboundClaims = false;
        }
    );

// Add services to the container.

var app = builder.Build();

app.UseAuthentication();

app.MapGet("/", (HttpContext ctx) => ctx.User.FindFirst("sub")?.Value ?? "empty");

//app.MapGet(
//    "/jwt",
//    () =>
//    {
//        var handler = new JsonWebTokenHandler();

//        var token = handler.CreateToken(
//            new SecurityTokenDescriptor()
//            {
//                Issuer = "https://localhost:5000",
//                Subject = new ClaimsIdentity(
//                    [
//                        new Claim("sub", Guid.NewGuid().ToString()),
//                        new Claim("name", Guid.NewGuid().ToString())
//                    ]
//                ),
//                SigningCredentials = new SigningCredentials(
//                    JsonWebKey.Create(jwkString),
//                    SecurityAlgorithms.RsaSha256
//                )
//            }
//        );

//        return token;
//    }
//);

app.Run();