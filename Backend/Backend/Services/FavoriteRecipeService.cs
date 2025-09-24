using AutoMapper;
using Backend.Models;
using static Ydb.Sdk.Value.ResultSet;
using System.Text;
using Ydb.Sdk.Value;
using Ydb.Sdk.Services.Table;

namespace Backend.Services
{
    public class FavoriteRecipeService
    {
        private readonly YdbService _ydbService;
        private readonly IMapper _mapper;

        public FavoriteRecipeService(YdbService ydbService, IMapper mapper)
        {
            _ydbService = ydbService;
            _mapper = mapper;
        }

        public async Task AddRecipeToFavoritesAsync(string userId, string recipeId)
        {
            // Проверяем существование рецепта
            var recipe = await _ydbService.GetRecipeAsync(recipeId);
            if (recipe == null)
            {
                throw new ArgumentException($"Recipe with id {recipeId} not found");
            }

            // Проверяем, не добавлен ли уже рецепт в избранное
            var existingFavorite = await GetFavoriteRecipeAsync(userId, recipeId);
            if (existingFavorite != null)
            {
                return; // Уже в избранном
            }

            await _ydbService.SessionExec(async (session) =>
            {
                var query = @"
                    INSERT INTO user_favorite_recipes (user_id, recipe_id, added_at)
                    VALUES ($user_id, $recipe_id, CurrentUtcTimestamp())
                ";

                var parameters = new Dictionary<string, YdbValue>
                {
                    ["$user_id"] = YdbValue.MakeUtf8(userId),
                    ["$recipe_id"] = YdbValue.MakeUtf8(recipeId)
                };

                var response = await session.ExecuteDataQuery(
                    query: query,
                    parameters: parameters,
                    txControl: TxControl.BeginSerializableRW().Commit()
                );

                if (!response.Status.IsSuccess)
                {
                    throw new Exception($"Failed to add recipe to favorites: {response.Status}");
                }

                return response;
            });
        }

        public async Task RemoveRecipeFromFavoritesAsync(string userId, string recipeId)
        {
            await _ydbService.SessionExec(async (session) =>
            {
                var query = @"
                    DELETE FROM user_favorite_recipes 
                    WHERE user_id = $user_id AND recipe_id = $recipe_id
                ";

                var parameters = new Dictionary<string, YdbValue>
                {
                    ["$user_id"] = YdbValue.MakeUtf8(userId),
                    ["$recipe_id"] = YdbValue.MakeUtf8(recipeId)
                };

                var response = await session.ExecuteDataQuery(
                    query: query,
                    parameters: parameters,
                    txControl: TxControl.BeginSerializableRW().Commit()
                );

                if (!response.Status.IsSuccess)
                {
                    throw new Exception($"Failed to remove recipe from favorites: {response.Status}");
                }

                return response;
            });
        }

        public async Task<List<RecipeDto>> GetFavoriteRecipesAsync(string userId)
        {
            var favoriteRecipes = await _ydbService.SessionExec(async (session) =>
            {
                var query = @"
                    SELECT r.*
                    FROM recipes r
                    INNER JOIN user_favorite_recipes ufr ON r.id = ufr.recipe_id
                    WHERE ufr.user_id = $user_id
                    ORDER BY ufr.added_at DESC
                ";

                var parameters = new Dictionary<string, YdbValue>
                {
                    ["$user_id"] = YdbValue.MakeUtf8(userId)
                };

                var response = await session.ExecuteDataQuery(
                    query: query,
                    parameters: parameters,
                    txControl: TxControl.BeginSerializableRW().Commit()
                );

                var recipes = new List<Recipe>();
                if (response.Status.IsSuccess && response.Result.ResultSets.Count > 0)
                {
                    var resultSet = response.Result.ResultSets[0];
                    foreach (var row in resultSet.Rows)
                    {
                        var recipe = MapRowToRecipe(row);
                        // Загружаем связанные данные
                        recipe.Ingredients = await _ydbService.GetRecipeIngredientsPublicAsync(recipe.Id);
                        recipe.Steps = await _ydbService.GetRecipeStepsPublicAsync(recipe.Id);
                        recipes.Add(recipe);
                    }
                }
                return recipes;
            });

            return _mapper.Map<List<RecipeDto>>(favoriteRecipes);
        }

        public async Task<List<RecipeDto>> GetAddedRecipesAsync(string userId)
        {
            var addedRecipes = await _ydbService.SessionExec(async (session) =>
            {
                var query = @"
                    SELECT r.*
                    FROM recipes r
                    WHERE r.user_id = $user_id
                    ORDER BY r.created_at DESC
                ";

                var parameters = new Dictionary<string, YdbValue>
                {
                    ["$user_id"] = YdbValue.MakeUtf8(userId)
                };

                var response = await session.ExecuteDataQuery(
                    query: query,
                    parameters: parameters,
                    txControl: TxControl.BeginSerializableRW().Commit()
                );

                var recipes = new List<Recipe>();
                if (response.Status.IsSuccess && response.Result.ResultSets.Count > 0)
                {
                    var resultSet = response.Result.ResultSets[0];
                    foreach (var row in resultSet.Rows)
                    {
                        var recipe = MapRowToRecipe(row);
                        // Загружаем связанные данные
                        recipe.Ingredients = await _ydbService.GetRecipeIngredientsPublicAsync(recipe.Id);
                        recipe.Steps = await _ydbService.GetRecipeStepsPublicAsync(recipe.Id);
                        recipes.Add(recipe);
                    }
                }
                return recipes;
            });

            return _mapper.Map<List<RecipeDto>>(addedRecipes);
        }

        public async Task<bool> IsRecipeInFavoritesAsync(string userId, string recipeId)
        {
            var favorite = await GetFavoriteRecipeAsync(userId, recipeId);
            return favorite != null;
        }

        private async Task<FavoriteRecipe?> GetFavoriteRecipeAsync(string userId, string recipeId)
        {
            return await _ydbService.SessionExec(async (session) =>
            {
                var query = @"
                    SELECT user_id, recipe_id, added_at
                    FROM user_favorite_recipes
                    WHERE user_id = $user_id AND recipe_id = $recipe_id
                ";

                var parameters = new Dictionary<string, YdbValue>
                {
                    ["$user_id"] = YdbValue.MakeUtf8(userId),
                    ["$recipe_id"] = YdbValue.MakeUtf8(recipeId)
                };

                var response = await session.ExecuteDataQuery(
                    query: query,
                    parameters: parameters,
                    txControl: TxControl.BeginSerializableRW().Commit()
                );

                if (response.Status.IsSuccess && response.Result.ResultSets.Count > 0)
                {
                    var resultSet = response.Result.ResultSets[0];
                    if (resultSet.Rows.Count > 0)
                    {
                        var row = resultSet.Rows[0];
                        return new FavoriteRecipe
                        {
                            UserId = Encoding.UTF8.GetString(row["user_id"].GetString()),
                            RecipeId = Encoding.UTF8.GetString(row["recipe_id"].GetString()),
                            AddedAt = row["added_at"].GetTimestamp()
                        };
                    }
                }
                return null;
            });
        }

        private Recipe MapRowToRecipe(Row row)
        {
            return new Recipe
            {
                Id = Encoding.UTF8.GetString(row["id"].GetString()),
                Title = Encoding.UTF8.GetString(row["title"].GetString()),
                Description = row["description"].GetOptionalString() != null ?
                    Encoding.UTF8.GetString(row["description"].GetOptionalString()) : null,
                Servings = row["servings"].GetInt32(),
                PrepTime = row["prep_time"].GetOptionalString() != null ?
                    Encoding.UTF8.GetString(row["prep_time"].GetOptionalString()) : null,
                CookTime = row["cook_time"].GetOptionalString() != null ?
                    Encoding.UTF8.GetString(row["cook_time"].GetOptionalString()) : null,
                ImageUrl = row["image_url"].GetOptionalString() != null ?
                    Encoding.UTF8.GetString(row["image_url"].GetOptionalString()) : null,
                CreatedAt = row["created_at"].GetTimestamp(),
                UpdatedAt = row["updated_at"].GetTimestamp()
            };
        }
    }

    public class FavoriteRecipe
    {
        public string UserId { get; set; } = string.Empty;
        public string RecipeId { get; set; } = string.Empty;
        public DateTime AddedAt { get; set; }
    }
}