using AutoMapper;
using Backend.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Backend.Services
{
    public class RecipeService
    {
        private readonly IMongoCollection<Recipe> _recipes;
        private readonly IMapper _mapper;

        public RecipeService(IOptions<MongoDBSettings> settings, IMapper mapper)
        {
            MongoClient client = new MongoClient(settings.Value.ConnectionURI);
            IMongoDatabase database = client.GetDatabase(settings.Value.DatabaseName);
            _recipes = database.GetCollection<Recipe>(settings.Value.RecipesCollection);
            _mapper = mapper;
        }

        public async Task<List<RecipeDto>> GetRecipesAsync()
        {
            return _mapper.Map<List<RecipeDto>>(await _recipes.Find(recipe => true).ToListAsync());
        }
        public async Task<RecipeDto?> GetRecipeAsync(string id)
        {
            var recipe = await _recipes.Find(recipe => recipe.id.ToString() == id).FirstOrDefaultAsync();
            return _mapper.Map<RecipeDto>(recipe);
        }
        public async Task<List<RecipeDto>> GetRecipesBySubTitleAsync(string subtitle)
        {
            var filter = Builders<Recipe>.Filter.Regex(r => r.title, new MongoDB.Bson.BsonRegularExpression(subtitle, "i"));
            var recipes = await _recipes.Find(filter).ToListAsync();

            return _mapper.Map<List<RecipeDto>>(recipes);
        }

        public async Task<List<RecipeDto>> GetRecipesByIngredientsAsync(string[] ingredients)
        {
            var filters = ingredients.Select(ingredient =>
                Builders<Recipe>.Filter.ElemMatch(r => r.ingredients, i => i.name.ToLower() == ingredient.ToLower())
            );

            var filter = Builders<Recipe>.Filter.Or(filters);

            return _mapper.Map<List<RecipeDto>>(await _recipes.Find(filter).ToListAsync());
        }

        public async Task<List<RecipeDto>> GetRecipesByIngredientsCountAsync(int ingredientsCount)
        {
            var recipes = await _recipes.Find(recipe => recipe.ingredients.Count() <= ingredientsCount).ToListAsync();

            return _mapper.Map<List<RecipeDto>>(recipes);
        }

        public async Task<List<RecipeDto>> GetRecipesByPersonCountAsync(int personCount)
        {
            var recipes = await _recipes.Find(recipe => recipe.servings == personCount).ToListAsync();

            return _mapper.Map<List<RecipeDto>>(recipes);
        }

        public async Task CreateRecipeAsync(RecipeDto recipeDto)
        {
            var recipe = _mapper.Map<Recipe>(recipeDto);
            await _recipes.InsertOneAsync(recipe);
        }

        public async Task UpdateRecipeAsync(string id, RecipeDto recipeDto)
        {
            var recipe = _mapper.Map<Recipe>(recipeDto);
            await _recipes.ReplaceOneAsync(r => r.id.ToString() == id, recipe);
        }

        public async Task DeleteRecipeAsync(string id)
        {
            await _recipes.DeleteOneAsync(r => r.id.ToString() == id);
        }

        public async Task UpdateRecipeImageAsync(string recipeId, string imageUrl)
        {
            var update = Builders<Recipe>.Update.Set(r => r.ImageUrl, imageUrl);
            await _recipes.UpdateOneAsync(r => r.id == ObjectId.Parse(recipeId), update);
        }

        public async Task ImportRecipesAsync(List<RecipeDto> recipeDtos)
        {
            var recipes = _mapper.Map<List<Recipe>>(recipeDtos);
            await _recipes.InsertManyAsync(recipes);
        }
    }
}

