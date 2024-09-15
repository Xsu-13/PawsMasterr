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
        private readonly FavoriteRecipeService _favoriteRecipeService;

        public UserController(
            UserService userService, 
            FavoriteRecipeService favoriteRecipeService)
        {
            _userService = userService;
            _favoriteRecipeService = favoriteRecipeService;
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

        [HttpPost("{recipeId}")]
        public async Task<IActionResult> AddToFavorites(string userId, string recipeId)
        {
            await _favoriteRecipeService.AddRecipeToFavoritesAsync(userId, recipeId);
            return Ok();
        }

        [HttpDelete("{recipeId}")]
        public async Task<IActionResult> RemoveFromFavorites(string userId, string recipeId)
        {
            await _favoriteRecipeService.RemoveRecipeFromFavoritesAsync(userId, recipeId);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetFavorites(string userId)
        {
            var favoriteRecipes = await _favoriteRecipeService.GetFavoriteRecipesAsync(userId);
            return Ok(favoriteRecipes);
        }
    }
}
