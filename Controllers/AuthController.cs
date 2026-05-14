using MedicineInventory.Models;
using MedicineInventory.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MedicineInventory.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private IConfiguration _config;
        private readonly IAuthService _authService;

        public AuthController(IConfiguration config, IInventoryStore store, IAuthService authService)
        {
            _config = config;
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest userLogin)
        {
            if (string.IsNullOrEmpty(userLogin.Name) || string.IsNullOrEmpty(userLogin.Password))
            {
                return BadRequest("Username and password are required.");
            }

            IActionResult response = Unauthorized();

            var loggedinUser = await _authService.AuthenticateUser(userLogin);

            if (loggedinUser != null)
            {
                response = Ok(new { token = loggedinUser.Token });
            }

            return response;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] User userRegister)
        {
            if (string.IsNullOrEmpty(userRegister.Name) || string.IsNullOrEmpty(userRegister.Password))
            {
                return BadRequest("Username and password are required.");
            }
            var data = await _authService.RegisterUser(userRegister);
            if (data == null)
            {
                return BadRequest("User already exists.");
            }
            return Ok(new { message = "User registered successfully." });


        }
    }
}
