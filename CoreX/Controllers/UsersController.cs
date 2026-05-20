using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreX.UI.Controllers
{
    [ApiController]
    [Route("users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register-user")]
        public async Task<IActionResult> RegisterUser(
            [FromBody] UserRegisterRequest userRegisterRequest)
        {
            await _userService.UserRegisterAsync(userRegisterRequest);

            return Ok("User registered successfully");
        }

        [HttpPost("register-admin")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> RegisterAdmin(
            [FromBody] UserRegisterRequest userRegisterRequest)
        {
            await _userService.AdminRegisterAsync(userRegisterRequest);

            return Ok("Admin registered successfully");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] UserSignInRequest userSignInRequest)
        {
            await _userService.SignInAsync(userSignInRequest);

            return Ok();
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _userService.SignOutAsync();

            return Ok();
        }
    }
}
