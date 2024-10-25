using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
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

        [HttpGet("search")]
        public async Task<ActionResult<List<RecipeDto>>> Search([FromQuery] string? subtitle, [FromQuery] string[]? ingredients, [FromQuery] int? ingredientsCount, [FromQuery] int? servingsCount)
        {
            List<RecipeDto> recipes = new();

            if (!string.IsNullOrEmpty(subtitle))
            {
                recipes = await _recipeService.GetRecipesBySubTitleAsync(subtitle);
            }
            else if (ingredients != null && ingredients.Any())
            {
                recipes = await _recipeService.GetRecipesByIngredientsAsync(ingredients);
            }
            else if (ingredientsCount.HasValue)
            {
                recipes = await _recipeService.GetRecipesByIngredientsCountAsync(ingredientsCount.Value);
            }
            else if (servingsCount.HasValue)
            {
                recipes = await _recipeService.GetRecipesByPersonCountAsync(servingsCount.Value);
            }
            else
            {
                recipes = await _recipeService.GetRecipesAsync();
            }

            if (!string.IsNullOrEmpty(subtitle))
                recipes = recipes.Intersect(await _recipeService.GetRecipesBySubTitleAsync(subtitle), new RecipeDtoComparer()).ToList();

            if (ingredients != null && ingredients.Length > 0)
                recipes = recipes.Intersect(await _recipeService.GetRecipesByIngredientsAsync(ingredients), new RecipeDtoComparer()).ToList();

            if (ingredientsCount.HasValue)
                recipes = recipes.Intersect(await _recipeService.GetRecipesByIngredientsCountAsync(ingredientsCount.Value), new RecipeDtoComparer()).ToList();

            if (servingsCount.HasValue)
                recipes = recipes.Intersect(await _recipeService.GetRecipesByPersonCountAsync(servingsCount.Value), new RecipeDtoComparer()).ToList();

            return recipes;
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

        [HttpPost("{recipeId}/upload-image")]
        public async Task<IActionResult> UploadRecipeImage(string recipeId, IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest("Invalid image file.");
            }

            var fileName = $"{Guid.NewGuid()}_{image.FileName}";
            var filePath = Path.Combine("wwwroot", "images", "recipes", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            var imageUrl = $"/images/recipes/{fileName}";

            await _recipeService.UpdateRecipeImageAsync(recipeId, imageUrl);

            return Ok(new { ImageUrl = imageUrl });
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
