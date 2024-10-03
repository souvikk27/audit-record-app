using AuditingRecordApp.Entity;
using AuditingRecordApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuditingRecordApp.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtTokenProvider _jwtTokenProvider;

        public UserController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtTokenProvider jwtTokenProvider)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenProvider = jwtTokenProvider;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterParameter parameter)
        {
            var user = new ApplicationUser
            {
                FirstName = parameter.FirstName,
                LastName = parameter.LastName,
                Email = parameter.Email,
                UserName = parameter.UserName,
                EmailConfirmed = true,
            };
            var result = await _userManager.CreateAsync(user, parameter.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok();
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginParameter parameter)
        {
            var user = await _userManager.FindByNameAsync(parameter.UserName);

            if (user == null)
            {
                return BadRequest("User not found");
            }

            var result = await _signInManager.PasswordSignInAsync(
                user,
                parameter.Password,
                false,
                false);

            if (!result.Succeeded)
            {
                return BadRequest("Invalid password");
            }

            var token = _jwtTokenProvider.GetToken(user);

            return Ok(token);
        }
    }

    public record LoginParameter(
        string UserName, 
        string Password);


    public record RegisterParameter(
        string FirstName,
        string LastName,
        string UserName,
        string Email,
        string Password);
}