using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace CookiesAndTokens;

public static class ApplicationBuilderExtension
{
    public static async Task<WebApplication> BuildAndSetup(this WebApplicationBuilder builder)
    {
        var app = builder.Build();

        using var scope = app.Services.CreateScope();
        var usrMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        var user = new IdentityUser() { UserName = "test@test.com", Email = "test@test.com" };

        var result1 = await usrMgr.CreateAsync(user, password: "password");

        var result2 = await usrMgr.AddClaimAsync(user, new Claim("role", "janitor"));
        
        return app;
    }
}