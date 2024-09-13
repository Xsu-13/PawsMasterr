using Backend.Models;
using Backend.Requests;
using Backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MongoDB.Bson;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var token = await _userService.Login(request.Email, request.Password);
                Response.Cookies.Append("token", token);
            }
            catch (Exception ex)
            {
                return BadRequest("Creating user error: " + ex);
            }

            return Ok("Login succesful");
        }

        [HttpPost("signup")]
        public async Task<ActionResult> SignUp([FromBody] SignUpRequest request)
        {
            await _userService.SignUp(request.UserName, request.Email, request.Password);
            return Ok();
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            Response.Cookies.Delete("token");

            return Ok();
        }
    }
}
