using AutoMapper;
using Backend.Models;

namespace Backend.Services
{
    public class RecipeService
    {
        private readonly YdbService _ydbService;
        private readonly IMapper _mapper;

        public RecipeService(YdbService ydbService, IMapper mapper)
        {
            _ydbService = ydbService;
            _mapper = mapper;
        }

        public async Task<List<RecipeDto>> GetRecipesAsync()
        {
            var recipes = await _ydbService.SearchRecipesAsync();
            return _mapper.Map<List<RecipeDto>>(recipes);
        }

        public async Task<RecipeDto?> GetRecipeAsync(string id)
        {
            var recipe = await _ydbService.GetRecipeAsync(id);
            return _mapper.Map<RecipeDto?>(recipe);
        }

        public async Task<List<RecipeDto>> GetRecipesBySubTitleAsync(string subtitle)
        {
            // YDB Document API doesn't support complex text search like ILIKE
            // We'll need to fetch all and filter in memory or use YQL functions
            var recipes = await _ydbService.SearchRecipesAsync();
            var filtered = recipes.Where(r => r.Title?.Contains(subtitle, StringComparison.OrdinalIgnoreCase) == true);
            return _mapper.Map<List<RecipeDto>>(filtered);
        }

        public async Task<List<RecipeDto>> GetRecipesByIngredientsAsync(string[] ingredients)
        {
            var recipes = await _ydbService.SearchRecipesAsync();
            var filtered = recipes.Where(r => 
                r.Ingredients.Any(i => ingredients.Contains(i.Name, StringComparer.OrdinalIgnoreCase)));
            return _mapper.Map<List<RecipeDto>>(filtered);
        }

        public async Task<List<RecipeDto>> GetRecipesByIngredientsCountAsync(int ingredientsCount)
        {
            var recipes = await _ydbService.SearchRecipesAsync();
            var filtered = recipes.Where(r => r.Ingredients.Count <= ingredientsCount);
            return _mapper.Map<List<RecipeDto>>(filtered);
        }

        public async Task<List<RecipeDto>> GetRecipesByPersonCountAsync(int personCount)
        {
            var recipes = await _ydbService.SearchRecipesAsync();
            var filtered = recipes.Where(r => r.Servings == personCount);
            return _mapper.Map<List<RecipeDto>>(filtered);
        }

        public async Task CreateRecipeAsync(RecipeDto recipeDto)
        {
            var recipe = _mapper.Map<Recipe>(recipeDto);
            recipe.Id = await _ydbService.CreateRecipeAsync(recipe);
        }

        public async Task CreateRecipeFromUserAsync(string userId, RecipeDto recipeDto)
        {
            await CreateRecipeAsync(recipeDto);
            
            // Add to user's added recipes
            var user = await _ydbService.GetDocumentAsync<User>("users", userId);
            if (user != null)
            {
                user.AddedRecipes.Add(recipeDto.Id!);
                await _ydbService.UpdateDocumentAsync("users", userId, user);
            }
        }

        public async Task UpdateRecipeAsync(string id, RecipeDto recipeDto)
        {
            var recipe = _mapper.Map<Recipe>(recipeDto);
            recipe.Id = id;
            await _ydbService.UpdateRecipeAsync(id, recipe);
        }

        public async Task DeleteRecipeAsync(string id)
        {
            await _ydbService.DeleteRecipeAsync(id);
        }

        public async Task DeleteRecipeFromUserAsync(string userId, string recipeId)
        {
            var user = await _ydbService.GetDocumentAsync<User>("users", userId);
            if (user != null)
            {
                user.AddedRecipes.Remove(recipeId);
                await _ydbService.UpdateUserAsync(userId, user);
            }
            await _ydbService.DeleteRecipeAsync(recipeId);
        }

        public async Task UpdateRecipeImageAsync(string recipeId, string imageUrl)
        {
            var recipe = await _ydbService.GetRecipeAsync(recipeId);
            if (recipe != null)
            {
                recipe.ImageUrl = imageUrl;
                await _ydbService.UpdateRecipeAsync(recipeId, recipe);
            }
        }

        public async Task ImportRecipesAsync(List<RecipeDto> recipeDtos)
        {
            foreach (var dto in recipeDtos)
            {
                await CreateRecipeAsync(dto);
            }
        }
    }
}

