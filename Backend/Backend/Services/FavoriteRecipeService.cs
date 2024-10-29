using AutoMapper;
using Backend.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Backend.Services
{
    public class FavoriteRecipeService
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Recipe> _recipes;
        private readonly IMapper _mapper;

        public FavoriteRecipeService(IOptions<MongoDBSettings> settings, IMapper mapper)
        {
            MongoClient client = new MongoClient(settings.Value.ConnectionURI);
            IMongoDatabase database = client.GetDatabase(settings.Value.DatabaseName);
            _users = database.GetCollection<User>(settings.Value.UsersCollection);
            _recipes = database.GetCollection<Recipe>(settings.Value.RecipesCollection);
            _mapper = mapper;
        }

        public async Task AddRecipeToFavoritesAsync(string userId, string recipeId)
        {
            var recipeObjectId = ObjectId.Parse(recipeId);

            var update = Builders<User>.Update.AddToSet(u => u.FavoriteRecipes, recipeObjectId);
            await _users.UpdateOneAsync(u => u.Id == userId, update);
        }

        public async Task RemoveRecipeFromFavoritesAsync(string userId, string recipeId)
        {
            var recipeObjectId = ObjectId.Parse(recipeId);

            var update = Builders<User>.Update.Pull(u => u.FavoriteRecipes, recipeObjectId);
            await _users.UpdateOneAsync(u => u.Id == userId, update);
        }

        public async Task<List<RecipeDto>> GetFavoriteRecipesAsync(string userId)
        {
            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();

            if (user == null || user.FavoriteRecipes.Count == 0)
            {
                return new List<RecipeDto>();
            }

            var favoriteRecipes = await _recipes.Find(r => user.FavoriteRecipes.Contains(r.id)).ToListAsync();

            return _mapper.Map<List<RecipeDto>>(favoriteRecipes);
        }
    }
}
