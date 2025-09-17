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
        private readonly SelectionService _selectionService;

        public RecipesController(RecipeService recipeService,
            SelectionService selectionService)
        {
            _recipeService = recipeService;
            _selectionService = selectionService;
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
        public async Task<ActionResult<List<RecipeDto>>> Search([FromQuery] string? subtitle, [FromQuery(Name = "ingredients")] string[]? ingredients, [FromQuery] int? ingredientsCount, [FromQuery] int? servingsCount)
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

        [HttpPost("user/{userId}")]
        public async Task<ActionResult> CreateByUser(string userId, RecipeDto recipe)
        {
            recipe.Id = ObjectId.GenerateNewId().ToString();
            await _recipeService.CreateRecipeFromUserAsync(userId, recipe);
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

        [HttpDelete("user/{userId}/recipe/{id}")]
        public async Task<IActionResult> DeleteByUser(string userId, string id)
        {
            var recipe = await _recipeService.GetRecipeAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }
            await _recipeService.DeleteRecipeFromUserAsync(userId, id);
            return NoContent();
        }

        [HttpPost("{recipeId}/upload-image")]
        public async Task<IActionResult> UploadRecipeImage(string recipeId, IFormFile image,
            [FromServices] Backend.Services.IImageStorageService imageStorage)
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest("Invalid image file.");
            }

            using var stream = image.OpenReadStream();
            var imageUrl = await imageStorage.UploadAsync(stream, image.ContentType, image.FileName, "recipes");

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

        [HttpGet("selection/{id}")]
        public async Task<ActionResult<SelectionDto>> GetBySelectionId(string id)
        {
            var selection = await _selectionService.GetRecipes(id);
            if (selection == null)
            {
                return NotFound();
            }
            return selection;
        }
    }
}
