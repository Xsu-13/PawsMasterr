using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace Cooking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecipesController : ControllerBase
    {
        private readonly RecipeService _recipeService;

        public RecipesController(RecipeService recipeService)
        {
            _recipeService = recipeService;
        }

        [HttpGet]
        public async Task<ActionResult<List<RecipeDto>>> Get() => await _recipeService.GetRecipesAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<RecipeDto>> Get(string id)
        {
            var recipe = await _recipeService.GetRecipeAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }
            return recipe;
        }

        [HttpPost]
        public async Task<ActionResult> Create(RecipeDto recipe)
        {
            recipe.Id = ObjectId.GenerateNewId().ToString();
            await _recipeService.CreateRecipeAsync(recipe);
            return CreatedAtAction(nameof(Get), new { id = recipe.Id }, recipe);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, RecipeDto recipe)
        {
            var existingRecipe = await _recipeService.GetRecipeAsync(id);
            if (existingRecipe == null)
            {
                return NotFound();
            }
            await _recipeService.UpdateRecipeAsync(id, recipe);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var recipe = await _recipeService.GetRecipeAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }
            await _recipeService.DeleteRecipeAsync(id);
            return NoContent();
        }

        [HttpPost("import")]
        public async Task<IActionResult> Import([FromBody]List<RecipeDto> recipes)
        {
            foreach (var recipe in recipes)
            {
                recipe.Id = ObjectId.GenerateNewId().ToString();
            }

            if (recipes != null)
            {
                await _recipeService.ImportRecipesAsync(recipes);
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
