using AutoMapper;
using Backend.Models;
using Ydb.Sdk.Value;
using System.Text;
using static Ydb.Sdk.Value.ResultSet;
using Ydb.Sdk.Services.Table;

namespace Backend.Services
{
    public class UserService
    {
        private readonly YdbService _ydbService;
        private readonly PasswordHasher _passwordHasher;
        private readonly JwtProvider _jwtProvider;
        private readonly IMapper _mapper;

        public UserService(
            YdbService ydbService,
            PasswordHasher passwordHasher,
            JwtProvider jwtProvider,
            IMapper mapper)
        {
            _ydbService = ydbService;
            _passwordHasher = passwordHasher;
            _jwtProvider = jwtProvider;
            _mapper = mapper;
        }

        public async Task<string> Login(string email, string password)
        {
            var user = await GetUserByEmailAsync(email)
                ?? throw new Exception("Incorrect password or login, check it out");

            var result = _passwordHasher.Verify(password, user.PasswordHash);

            if (!result)
            {
                throw new Exception("Incorrect password or login, check it out");
            }

            var token = _jwtProvider.GenerateToken(user);

            return token;
        }

        public async Task SignUp(string username, string email, string password)
        {
            // Проверяем, не существует ли пользователь с таким email
            var existingUser = await GetUserByEmailAsync(email);
            if (existingUser != null)
            {
                throw new Exception("User with this email already exists");
            }

            var hashedPassword = _passwordHasher.Generate(password);
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = username,
                Email = email,
                PasswordHash = hashedPassword,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await CreateUserAsync(user);
        }

        public async Task<User> GetUserInfo(string userId)
        {
            return await GetUserByIdAsync(userId)
                ?? throw new Exception("User not found");
        }

        public async Task UpdateUserImageAsync(string userId, string imageUrl)
        {
            var user = await GetUserByIdAsync(userId)
                ?? throw new Exception("User not found");

            user.ImageUrl = imageUrl;
            user.UpdatedAt = DateTime.UtcNow;

            await UpdateUserAsync(user);
        }

        public async Task UpdateUserProfileAsync(string userId, string username, string email)
        {
            var user = await GetUserByIdAsync(userId)
                ?? throw new Exception("User not found");

            // Проверяем, не занят ли email другим пользователем
            if (user.Email != email)
            {
                var existingUser = await GetUserByEmailAsync(email);
                if (existingUser != null && existingUser.Id != userId)
                {
                    throw new Exception("Email is already taken by another user");
                }
            }

            user.Username = username;
            user.Email = email;
            user.UpdatedAt = DateTime.UtcNow;

            await UpdateUserAsync(user);
        }

        public async Task ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await GetUserByIdAsync(userId)
                ?? throw new Exception("User not found");

            // Проверяем текущий пароль
            var isCurrentPasswordValid = _passwordHasher.Verify(currentPassword, user.PasswordHash);
            if (!isCurrentPasswordValid)
            {
                throw new Exception("Current password is incorrect");
            }

            user.PasswordHash = _passwordHasher.Generate(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await UpdateUserAsync(user);
        }

        public async Task UpdateRecipeImageAsync(string userId, string imageUrl)
        {
            await UpdateUserImageAsync(userId, imageUrl);
        }

        private async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _ydbService.SessionExec(async (session) =>
            {
                var query = @"
                    SELECT id, username, email, password_hash, image_url, created_at, updated_at
                    FROM users 
                    WHERE email = $email
                ";

                var parameters = new Dictionary<string, YdbValue>
                {
                    ["$email"] = YdbValue.MakeUtf8(email)
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
                        return MapRowToUser(resultSet.Rows[0]);
                    }
                }
                return null;
            });
        }

        private async Task<User?> GetUserByIdAsync(string userId)
        {
            return await _ydbService.SessionExec(async (session) =>
            {
                var query = @"
                    SELECT id, username, email, password_hash, image_url, created_at, updated_at
                    FROM users 
                    WHERE id = $id
                ";

                var parameters = new Dictionary<string, YdbValue>
                {
                    ["$id"] = YdbValue.MakeUtf8(userId)
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
                        return MapRowToUser(resultSet.Rows[0]);
                    }
                }
                return null;
            });
        }

        private async Task CreateUserAsync(User user)
        {
            await _ydbService.SessionExec(async (session) =>
            {
                var query = @"
                    INSERT INTO users (id, username, email, password_hash, image_url, created_at, updated_at)
                    VALUES ($id, $username, $email, $password_hash, $image_url, $created_at, $updated_at)
                ";

                var parameters = new Dictionary<string, YdbValue>
                {
                    ["$id"] = YdbValue.MakeUtf8(user.Id),
                    ["$username"] = YdbValue.MakeUtf8(user.Username),
                    ["$email"] = YdbValue.MakeUtf8(user.Email),
                    ["$password_hash"] = YdbValue.MakeUtf8(user.PasswordHash),
                    ["$image_url"] = string.IsNullOrEmpty(user.ImageUrl) ? YdbValue.MakeOptionalUtf8(null) : YdbValue.MakeOptionalUtf8(user.ImageUrl),
                    ["$created_at"] = YdbValue.MakeTimestamp(user.CreatedAt),
                    ["$updated_at"] = YdbValue.MakeTimestamp(user.UpdatedAt)
                };

                var response = await session.ExecuteDataQuery(
                    query: query,
                    parameters: parameters,
                    txControl: TxControl.BeginSerializableRW().Commit()
                );

                if (!response.Status.IsSuccess)
                {
                    throw new Exception($"Failed to create user: {response.Status}");
                }

                return response;
            });
        }

        private async Task UpdateUserAsync(User user)
        {
            await _ydbService.SessionExec(async (session) =>
            {
                var query = @"
                    UPDATE users 
                    SET username = $username, 
                        email = $email, 
                        password_hash = $password_hash, 
                        image_url = $image_url, 
                        updated_at = $updated_at
                    WHERE id = $id
                ";

                var parameters = new Dictionary<string, YdbValue>
                {
                    ["$id"] = YdbValue.MakeUtf8(user.Id),
                    ["$username"] = YdbValue.MakeUtf8(user.Username),
                    ["$email"] = YdbValue.MakeUtf8(user.Email),
                    ["$password_hash"] = YdbValue.MakeUtf8(user.PasswordHash),
                    ["$image_url"] = string.IsNullOrEmpty(user.ImageUrl) ? YdbValue.MakeOptionalUtf8(null) : YdbValue.MakeOptionalUtf8(user.ImageUrl),
                    ["$updated_at"] = YdbValue.MakeTimestamp(user.UpdatedAt)
                };

                var response = await session.ExecuteDataQuery(
                    query: query,
                    parameters: parameters,
                    txControl: TxControl.BeginSerializableRW().Commit()
                );

                if (!response.Status.IsSuccess)
                {
                    throw new Exception($"Failed to update user: {response.Status}");
                }

                return response;
            });
        }

        private User MapRowToUser(Row row)
        {
            return new User
            {
                Id = Encoding.UTF8.GetString(row["id"].GetString()),
                Username = Encoding.UTF8.GetString(row["username"].GetString()),
                Email = Encoding.UTF8.GetString(row["email"].GetString()),
                PasswordHash = Encoding.UTF8.GetString(row["password_hash"].GetString()),
                ImageUrl = row["image_url"].GetOptionalString() != null ?
                    Encoding.UTF8.GetString(row["image_url"].GetOptionalString()) : null,
                CreatedAt = row["created_at"].GetTimestamp(),
                UpdatedAt = row["updated_at"].GetTimestamp()
            };
        }
    }
}