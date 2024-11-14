using Backend.Extensions;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

// Add services to the container.

services.Configure<JwtOptions>(configuration.GetSection(nameof(JwtOptions)));
services.AddApiAuthentication(configuration);
services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDB"));

services.AddSingleton<JwtProvider>();
services.AddSingleton<PasswordHasher>();
services.AddScoped<RecipeService>();
services.AddScoped<UserService>();
services.AddScoped<FavoriteRecipeService>();

//services.AddControllersWithViews();
services.AddControllers();

services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "SmartDocs",
        Description = "Cutting-edge documentation service <br><br> <a href='api-docs'>Redoc</a>",
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                  });
    c.CustomSchemaIds(type => type.ToString());
});

services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin();
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
    });
});


var app = builder.Build();

app.UseCors();
app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCookiePolicy(new CookiePolicyOptions
{
    HttpOnly = HttpOnlyPolicy.Always,
    MinimumSameSitePolicy = SameSiteMode.Strict,
    Secure = CookieSecurePolicy.Always
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.MapControllers();

app.Run();
