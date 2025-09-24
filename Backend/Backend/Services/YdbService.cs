using Ydb.Sdk;
using Ydb.Sdk.Services.Table;
using Ydb.Sdk.Value;
using Backend.Models;
using Microsoft.Extensions.Options;
using System.Text;
using static Ydb.Sdk.Value.ResultSet;
using Ydb.Sdk.Yc;
using Session = Ydb.Sdk.Services.Table.Session;
using Ydb.Sdk.Client;

namespace Backend.Services
{
    public class YdbService : IDisposable
    {
        public TableClient TableClient => _tableClient;
        public async Task UpdateUserAsync(string id, User user)
        {
            var response = (ExecuteDataQueryResponse)await _tableClient.SessionExec(async session =>
            {
                var query = @"
                    UPDATE users 
                    SET 
                        username = $username,
                        email = $email,
                        password_hash = $password_hash,
                        image_url = $image_url,
                        updated_at = $updated_at
                    WHERE id = $id
                ";

                var parameters = new Dictionary<string, YdbValue>
                {
                    ["$id"] = YdbValue.MakeUtf8(id),
                    ["$username"] = YdbValue.MakeUtf8(user.Username ?? ""),
                    ["$email"] = YdbValue.MakeUtf8(user.Email ?? ""),
                    ["$password_hash"] = YdbValue.MakeUtf8(user.PasswordHash ?? ""),
                    ["$image_url"] = string.IsNullOrEmpty(user.ImageUrl) ? YdbValue.MakeOptionalUtf8(null) : YdbValue.MakeOptionalUtf8(user.ImageUrl),
                    ["$updated_at"] = YdbValue.MakeTimestamp(DateTime.UtcNow)
                };

                return await session.ExecuteDataQuery(
                    query: query,
                    parameters: parameters,
                    txControl: TxControl.BeginSerializableRW().Commit()
                );
            });

            if (!response.Status.IsSuccess)
            {
                throw new Exception($"Failed to update user: {response.Status}");
            }
        }
        private readonly Driver _driver;
        private readonly TableClient _tableClient;
        private bool _disposed = false;

        public YdbService(IOptions<YdbSettings> settings)
        {
            var keyPath = "xsu-yandex-key.json";
            var cfg = settings.Value;

            var config = new DriverConfig(
                endpoint: cfg.Endpoint,
                database: cfg.Database,
                credentials: new ServiceAccountProvider(keyPath)
            );

            _driver = Driver.CreateInitialized(config).GetAwaiter().GetResult();
            _tableClient = new TableClient(_driver, new TableClientConfig());
        }

        // Recipe operations
        public async Task<Recipe?> GetRecipeAsync(string id)
        {
            var response = (ExecuteDataQueryResponse)await _tableClient.SessionExec(async session =>
            {
                var query = @"
                    SELECT 
                        id, title, description, servings, prep_time, cook_time, image_url,
                        created_at, updated_at
                    FROM recipes 
                    WHERE id = $id
                ";

                var parameters = new Dictionary<string, YdbValue>
                {
                    ["$id"] = YdbValue.MakeUtf8(id)
                };

                return await session.ExecuteDataQuery(
                    query: query,
                    parameters: parameters,
                    txControl: TxControl.BeginSerializableRW().Commit()
                );
            });

            if (response.Status.IsSuccess && response.Result.ResultSets.Count > 0)
            {
                var resultSet = response.Result.ResultSets[0];
                if (resultSet.Rows.Count > 0)
                {
                    var row = resultSet.Rows[0];
                    return MapRowToRecipe(row, resultSet.Columns.ToArray());
                }
            }
            return null;
        }

        public async Task<string> CreateRecipeAsync(Recipe recipe)
        {
            var id = Guid.NewGuid().ToString();
            var now = DateTime.UtcNow;

            var response = (ExecuteDataQueryResponse)await _tableClient.SessionExec(async session =>
            {
                var query = @"
                    INSERT INTO recipes (
                        id, title, description, servings, prep_time, cook_time, image_url,
                        created_at, updated_at
                    ) VALUES (
                        $id, $title, $description, $servings, $prep_time, $cook_time, $image_url,
                        $created_at, $updated_at
                    )
                ";

                var parameters = new Dictionary<string, YdbValue>
                {
                    ["$id"] = YdbValue.MakeUtf8(id),
                    ["$title"] = YdbValue.MakeUtf8(recipe.Title),
                    ["$description"] = string.IsNullOrEmpty(recipe.Description) ? YdbValue.MakeOptionalUtf8(null) : YdbValue.MakeOptionalUtf8(recipe.Description),
                    ["$servings"] = YdbValue.MakeOptionalInt32(recipe.Servings),
                    ["$prep_time"] = string.IsNullOrEmpty(recipe.PrepTime) ? YdbValue.MakeOptionalUtf8(null) : YdbValue.MakeOptionalUtf8(recipe.PrepTime),
                    ["$cook_time"] = string.IsNullOrEmpty(recipe.CookTime) ? YdbValue.MakeOptionalUtf8(null) : YdbValue.MakeOptionalUtf8(recipe.CookTime),
                    ["$image_url"] = string.IsNullOrEmpty(recipe.ImageUrl) ? YdbValue.MakeOptionalUtf8(null) : YdbValue.MakeOptionalUtf8(recipe.ImageUrl),
                    ["$created_at"] = YdbValue.MakeOptionalTimestamp(now),
                    ["$updated_at"] = YdbValue.MakeOptionalTimestamp(now)
                };

                return await session.ExecuteDataQuery(
                    query: query,
                    parameters: parameters,
                    txControl: TxControl.BeginSerializableRW().Commit()
                );
            });

            if (!response.Status.IsSuccess)
            {
                throw new Exception($"Failed to create recipe: {response.Status}");
            }

            // ��������� �����������
            if (recipe.Ingredients?.Count > 0)
            {
                await CreateRecipeIngredientsAsync(id, recipe.Ingredients);
            }

            // ��������� ���� �������������
            if (recipe.Steps?.Count > 0)
            {
                await CreateRecipeStepsAsync(id, recipe.Steps);
            }

            return id;
        }

        public async Task UpdateRecipeAsync(string id, Recipe recipe)
        {
            var now = DateTime.UtcNow;

            var response = (ExecuteDataQueryResponse)await _tableClient.SessionExec(async session =>
            {
                var query = @"
                    UPDATE recipes 
                    SET 
                        title = $title,
                        description = $description,
                        servings = $servings,
                        prep_time = $prep_time,
                        cook_time = $cook_time,
                        image_url = $image_url,
                        updated_at = $updated_at
                    WHERE id = $id
                ";

                var parameters = new Dictionary<string, YdbValue>
                {
                    ["$id"] = YdbValue.MakeUtf8(id),
                    ["$title"] = YdbValue.MakeUtf8(recipe.Title),
                    ["$description"] = string.IsNullOrEmpty(recipe.Description) ? YdbValue.MakeOptionalUtf8(null) : YdbValue.MakeOptionalUtf8(recipe.Description),
                    ["$servings"] = YdbValue.MakeOptionalInt32(recipe.Servings),
                    ["$prep_time"] = string.IsNullOrEmpty(recipe.PrepTime) ? YdbValue.MakeOptionalUtf8(null) : YdbValue.MakeOptionalUtf8(recipe.PrepTime),
                    ["$cook_time"] = string.IsNullOrEmpty(recipe.CookTime) ? YdbValue.MakeOptionalUtf8(null) : YdbValue.MakeOptionalUtf8(recipe.CookTime),
                    ["$image_url"] = string.IsNullOrEmpty(recipe.ImageUrl) ? YdbValue.MakeOptionalUtf8(null) : YdbValue.MakeOptionalUtf8(recipe.ImageUrl),
                    ["$updated_at"] = YdbValue.MakeOptionalTimestamp(now)
                };

                return await session.ExecuteDataQuery(
                    query: query,
                    parameters: parameters,
                    txControl: TxControl.BeginSerializableRW().Commit()
                );
            });

            if (!response.Status.IsSuccess)
            {
                throw new Exception($"Failed to update recipe: {response.Status}");
            }

            // ��������� ����������� (������� ������, ��������� �����)
            await DeleteRecipeIngredientsAsync(id);
            if (recipe.Ingredients?.Count > 0)
            {
                await CreateRecipeIngredientsAsync(id, recipe.Ingredients);
            }

            // ��������� ���� �������������
            await DeleteRecipeStepsAsync(id);
            if (recipe.Steps?.Count > 0)
            {
                await CreateRecipeStepsAsync(id, recipe.Steps);
            }
        }

        public async Task DeleteRecipeAsync(string id)
        {
            // ������� ��������� ������ ������� (��-�� foreign key constraints)
            await DeleteRecipeIngredientsAsync(id);
            await DeleteRecipeStepsAsync(id);

            var response = (ExecuteDataQueryResponse)await _tableClient.SessionExec(async session =>
            {
                var query = "DELETE FROM recipes WHERE id = $id";

                var parameters = new Dictionary<string, YdbValue>
                {
                    ["$id"] = YdbValue.MakeUtf8(id)
                };

                return await session.ExecuteDataQuery(
                    query: query,
                    parameters: parameters,
                    txControl: TxControl.BeginSerializableRW().Commit()
                );
            });

            if (!response.Status.IsSuccess)
            {
                throw new Exception($"Failed to delete recipe: {response.Status}");
            }
        }

        public async Task<List<Recipe>> SearchRecipesAsync(string searchTerm = "", int limit = 50)
        {
            var query = @"
                SELECT 
                    r.id, r.title, r.description, r.servings, r.prep_time, r.cook_time, r.image_url,
                    r.created_at, r.updated_at
                FROM recipes r
            ";

            var parameters = new Dictionary<string, YdbValue>();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query += " WHERE r.title LIKE $search_term OR r.description LIKE $search_term";
                parameters["$search_term"] = YdbValue.MakeUtf8($"%{searchTerm}%");
            }

            query += " ORDER BY r.created_at DESC LIMIT $limit";
            parameters["$limit"] = YdbValue.MakeUint32((uint)limit);

            var response = (ExecuteDataQueryResponse)await _tableClient.SessionExec(async session =>
            {
                return await session.ExecuteDataQuery(
                    query: query,
                    parameters: parameters,
                    txControl: TxControl.BeginSerializableRW().Commit()
                );
            });

            var results = new List<Recipe>();

            if (response.Status.IsSuccess && response.Result.ResultSets.Count > 0)
            {
                var resultSet = response.Result.ResultSets[0];
                foreach (var row in resultSet.Rows)
                {
                    var recipe = MapRowToRecipe(row, resultSet.Columns.ToArray());
                    if (recipe != null)
                    {
                        // ��������� ��������� ������
                        recipe.Ingredients = await GetRecipeIngredientsAsync(recipe.Id);
                        recipe.Steps = await GetRecipeStepsAsync(recipe.Id);
                        results.Add(recipe);
                    }
                }
            }

            return results;
        }

        // Ingredient operations
        private async Task CreateRecipeIngredientsAsync(string recipeId, List<Ingredient> ingredients)
        {
            for (int i = 0; i < ingredients.Count; i++)
            {
                var ingredient = ingredients[i];
                var response = (ExecuteDataQueryResponse)await _tableClient.SessionExec(async session =>
                {
                    var query = @"
                        INSERT INTO recipe_ingredients (recipe_id, ingredient_index, name, quantity)
                        VALUES ($recipe_id, $ingredient_index, $name, $quantity)
                    ";

                    var parameters = new Dictionary<string, YdbValue>
                    {
                        ["$recipe_id"] = YdbValue.MakeUtf8(recipeId),
                        ["$ingredient_index"] = YdbValue.MakeUint32((uint)i),
                        ["$name"] = YdbValue.MakeUtf8(ingredient.Name),
                        ["$quantity"] = YdbValue.MakeUtf8(ingredient.Quantity)
                    };

                    return await session.ExecuteDataQuery(
                        query: query,
                        parameters: parameters,
                        txControl: TxControl.BeginSerializableRW().Commit()
                    );
                });

                if (!response.Status.IsSuccess)
                {
                    throw new Exception($"Failed to create ingredient: {response.Status}");
                }
            }
        }

        private async Task<List<Ingredient>> GetRecipeIngredientsAsync(string recipeId)
        {
            var response = (ExecuteDataQueryResponse)await _tableClient.SessionExec(async session =>
            {
                var query = @"
                    SELECT name, quantity 
                    FROM recipe_ingredients 
                    WHERE recipe_id = $recipe_id 
                    ORDER BY ingredient_index
                ";

                var parameters = new Dictionary<string, YdbValue>
                {
                    ["$recipe_id"] = YdbValue.MakeUtf8(recipeId)
                };

                return await session.ExecuteDataQuery(
                    query: query,
                    parameters: parameters,
                    txControl: TxControl.BeginSerializableRW().Commit()
                );
            });

            var ingredients = new List<Ingredient>();

            if (response.Status.IsSuccess && response.Result.ResultSets.Count > 0)
            {
                var resultSet = response.Result.ResultSets[0];
                foreach (var row in resultSet.Rows)
                {
                    ingredients.Add(new Ingredient
                    {
                        Name = row["name"].GetOptionalUtf8() ?? "",
                        Quantity = row["quantity"].GetOptionalUtf8() ?? ""
                    });
                }
            }

            return ingredients;
        }

        private async Task DeleteRecipeIngredientsAsync(string recipeId)
        {
            var response = (ExecuteDataQueryResponse)await _tableClient.SessionExec(async session =>
            {
                var query = "DELETE FROM recipe_ingredients WHERE recipe_id = $recipe_id";

                var parameters = new Dictionary<string, YdbValue>
                {
                    ["$recipe_id"] = YdbValue.MakeUtf8(recipeId)
                };

                return await session.ExecuteDataQuery(
                    query: query,
                    parameters: parameters,
                    txControl: TxControl.BeginSerializableRW().Commit()
                );
            });

            if (!response.Status.IsSuccess)
            {
                throw new Exception($"Failed to delete ingredients: {response.Status}");
            }
        }

        // Step operations
        private async Task CreateRecipeStepsAsync(string recipeId, List<string> steps)
        {
            for (int i = 0; i < steps.Count; i++)
            {
                var response = (ExecuteDataQueryResponse)await _tableClient.SessionExec(async session =>
                {
                    var query = @"
                        INSERT INTO recipe_steps (recipe_id, step_index, instruction)
                        VALUES ($recipe_id, $step_index, $instruction)
                    ";

                    var parameters = new Dictionary<string, YdbValue>
                    {
                        ["$recipe_id"] = YdbValue.MakeUtf8(recipeId),
                        ["$step_index"] = YdbValue.MakeUint32((uint)i),
                        ["$instruction"] = YdbValue.MakeUtf8(steps[i])
                    };

                    return await session.ExecuteDataQuery(
                        query: query,
                        parameters: parameters,
                        txControl: TxControl.BeginSerializableRW().Commit()
                    );
                });

                if (!response.Status.IsSuccess)
                {
                    throw new Exception($"Failed to create step: {response.Status}");
                }
            }
        }

        private async Task<List<string>> GetRecipeStepsAsync(string recipeId)
        {
            var response = (ExecuteDataQueryResponse)await _tableClient.SessionExec(async session =>
            {
                var query = @"
                    SELECT instruction 
                    FROM recipe_steps 
                    WHERE recipe_id = $recipe_id 
                    ORDER BY step_index
                ";

                var parameters = new Dictionary<string, YdbValue>
                {
                    ["$recipe_id"] = YdbValue.MakeUtf8(recipeId)
                };

                return await session.ExecuteDataQuery(
                    query: query,
                    parameters: parameters,
                    txControl: TxControl.BeginSerializableRW().Commit()
                );
            });

            var steps = new List<string>();

            if (response.Status.IsSuccess && response.Result.ResultSets.Count > 0)
            {
                var resultSet = response.Result.ResultSets[0];
                foreach (var row in resultSet.Rows)
                {
                    steps.Add(row["instruction"].GetOptionalUtf8() ?? "");
                }
            }

            return steps;
        }

        private async Task DeleteRecipeStepsAsync(string recipeId)
        {
            var response = (ExecuteDataQueryResponse)await _tableClient.SessionExec(async session =>
            {
                var query = "DELETE FROM recipe_steps WHERE recipe_id = $recipe_id";

                var parameters = new Dictionary<string, YdbValue>
                {
                    ["$recipe_id"] = YdbValue.MakeUtf8(recipeId)
                };

                return await session.ExecuteDataQuery(
                    query: query,
                    parameters: parameters,
                    txControl: TxControl.BeginSerializableRW().Commit()
                );
            });

            if (!response.Status.IsSuccess)
            {
                throw new Exception($"Failed to delete steps: {response.Status}");
            }
        }

        // Helper methods
        private Recipe MapRowToRecipe(Row row, Column[] columns)
        {
            return new Recipe
            {
                Id = row["id"].GetOptionalUtf8() ?? "",
                Title = row["title"].GetOptionalUtf8() ?? "",
                Description = row["description"].GetOptionalUtf8(),
                Servings = row["servings"].GetOptionalInt32() ?? 0,
                PrepTime = row["prep_time"].GetOptionalUtf8(),
                CookTime = row["cook_time"].GetOptionalUtf8(),
                ImageUrl = row["image_url"].GetOptionalUtf8(),
                CreatedAt = row["created_at"].GetOptionalTimestamp() ?? DateTime.UtcNow,
                UpdatedAt = row["updated_at"].GetOptionalTimestamp() ?? DateTime.UtcNow
            };
        }

        // Generic methods for Document API compatibility
        public async Task<T?> GetDocumentAsync<T>(string collection, string id) where T : class
        {
            if (typeof(T) == typeof(User))
            {
                var user = await GetUserByIdAsync(id);
                return user as T;
            }
            else if (typeof(T) == typeof(Selection))
            {
                var selection = await GetSelectionByIdAsync(id);
                return selection as T;
            }
            
            // Add other entity types as needed
            throw new NotSupportedException($"GetDocumentAsync is not supported for type {typeof(T).Name}");
        }

        public async Task UpdateDocumentAsync<T>(string collection, string id, T document) where T : class
        {
            if (typeof(T) == typeof(User) && document is User user)
            {
                await UpdateUserAsync(id, user);
                return;
            }
            
            // Add other entity types as needed
            throw new NotSupportedException($"UpdateDocumentAsync is not supported for type {typeof(T).Name}");
        }

        public async Task<TResult> SessionExec<TResult>(Func<Session, Task<TResult>> action)
        {
            var result = default(TResult);
            var response = await _tableClient.SessionExec(async session =>
            {
                result = await action(session);
                // Возвращаем пустой успешный ответ
                return await session.ExecuteDataQuery(
                    query: "SELECT 1", 
                    parameters: new Dictionary<string, YdbValue>(),
                    txControl: TxControl.BeginSerializableRW().Commit()
                );
            });
            return result!;
        }

        // Public methods to access private methods from other services
        public async Task<List<Ingredient>> GetRecipeIngredientsPublicAsync(string recipeId)
        {
            return await GetRecipeIngredientsAsync(recipeId);
        }

        public async Task<List<string>> GetRecipeStepsPublicAsync(string recipeId)
        {
            return await GetRecipeStepsAsync(recipeId);
        }

        private async Task<User?> GetUserByIdAsync(string userId)
        {
            var response = (ExecuteDataQueryResponse)await _tableClient.SessionExec(async session =>
            {
                var query = @"
                    SELECT id, username, email, password_hash, image_url, 
                           created_at, updated_at
                    FROM users 
                    WHERE id = $id
                ";

                var parameters = new Dictionary<string, YdbValue>
                {
                    ["$id"] = YdbValue.MakeUtf8(userId)
                };

                return await session.ExecuteDataQuery(
                    query: query,
                    parameters: parameters,
                    txControl: TxControl.BeginSerializableRW().Commit()
                );
            });

            if (response.Status.IsSuccess && response.Result.ResultSets.Count > 0)
            {
                var resultSet = response.Result.ResultSets[0];
                if (resultSet.Rows.Count > 0)
                {
                    var row = resultSet.Rows[0];
                    return MapRowToUser(row);
                }
            }
            return null;
        }

        private async Task<Selection?> GetSelectionByIdAsync(string selectionId)
        {
            var response = (ExecuteDataQueryResponse)await _tableClient.SessionExec(async session =>
            {
                var query = @"
                    SELECT id, title
                    FROM selections 
                    WHERE id = $id
                ";

                var parameters = new Dictionary<string, YdbValue>
                {
                    ["$id"] = YdbValue.MakeUtf8(selectionId)
                };

                return await session.ExecuteDataQuery(
                    query: query,
                    parameters: parameters,
                    txControl: TxControl.BeginSerializableRW().Commit()
                );
            });

            if (response.Status.IsSuccess && response.Result.ResultSets.Count > 0)
            {
                var resultSet = response.Result.ResultSets[0];
                if (resultSet.Rows.Count > 0)
                {
                    var row = resultSet.Rows[0];
                    return MapRowToSelection(row);
                }
            }
            return null;
        }

        private Selection MapRowToSelection(Row row)
        {
            return new Selection
            {
                Id = row["id"].GetOptionalUtf8() ?? "",
                Title = row["title"].GetOptionalUtf8(),
                Recipes = new List<string>() // Список загружается отдельно
            };
        }

        private User MapRowToUser(Row row)
        {
            return new User
            {
                Id = row["id"].GetOptionalUtf8() ?? "",
                Username = row["username"].GetOptionalUtf8() ?? "",
                Email = row["email"].GetOptionalUtf8() ?? "",
                PasswordHash = row["password_hash"].GetOptionalUtf8() ?? "",
                ImageUrl = row["image_url"].GetOptionalUtf8(),
                AddedRecipes = new List<string>(), // Списки загружаются отдельно
                FavoriteRecipes = new List<string>(), // Списки загружаются отдельно
                CreatedAt = row["created_at"].GetTimestamp(),
                UpdatedAt = row["updated_at"].GetTimestamp()
            };
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _driver?.Dispose();
                _disposed = true;
            }
        }
    }
}