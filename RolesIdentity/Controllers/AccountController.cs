using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace RolesIdentity.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        [HttpGet("/login")]
        public async Task<IActionResult> Login(SignInManager<IdentityUser> signInManager)
        {
            await signInManager.PasswordSignInAsync("test@test.com", "password", false, false);

            return Ok();
        }
    }
}
