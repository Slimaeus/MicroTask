using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MicroTask.Services.Tasks.Domain;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var securitySchema = new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    options.AddSecurityDefinition("Bearer", securitySchema);

    var securityRequirement = new OpenApiSecurityRequirement
                {
                    { securitySchema, new[] { "Bearer" } }
                };

    options.AddSecurityRequirement(securityRequirement);
});

var tokenKey = builder.Configuration["JwtSettings:SecretKey"];

var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey!));

builder.Services.AddAuthentication(config =>
{
    config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

})
    .AddJwtBearer(config =>
    {
        config.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(5)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

//app.UseHttpsRedirection();

var tasks = new List<ApplicationTask>
{
    new ApplicationTask
    {
        Id = 1,
        Title = "First Task",
        Description = "Do homework",
        UserId = 1,
        CategoryId = 1
    },
    new ApplicationTask
    {
        Id = 2,
        Title = "Second Task",
        Description = "Go to sleep",
        UserId = 2,
        CategoryId = 2
    }
};

const string TaskEndpoint = "Tasks";
const string CategoryEndpoint = "Categories";
const string UserEndpoint = "Users";
string CategoriesApi = Environment.GetEnvironmentVariable("CATEGORIES_SERVICE") is not null ? $"http://{Environment.GetEnvironmentVariable("CATEGORIES_SERVICE")}/api" : "http://microtask.services.categories.api/api";
string UsersApi = Environment.GetEnvironmentVariable("USERS_SERVICE") is not null ? $"http://{Environment.GetEnvironmentVariable("USERS_SERVICE")}/api" : "http://microtask.services.users.api/api";

app.MapGet($"api/{TaskEndpoint}", () =>
{
    return Results.Ok(tasks);
});

app.MapGet($"api/{TaskEndpoint}/{{id:int}}", async (int id, HttpClient client) =>
{
    var task = tasks.FirstOrDefault(x => x.Id == id);
    if (task is null)
    {
        return Results.NotFound();
    }

    var getTasks = new Task<HttpResponseMessage>[]
    {
        client.GetAsync($"{CategoriesApi}/{CategoryEndpoint}/{task.CategoryId}"),
        client.GetAsync($"{UsersApi}/{UserEndpoint}/{task.UserId}")
    };

    var responseMessages = await Task.WhenAll(getTasks);

    var categoryResponse = await responseMessages[0].Content.ReadAsStringAsync();
    var userResponse = await responseMessages[1].Content.ReadAsStringAsync();

    task.Category = JsonConvert.DeserializeObject<Category>(categoryResponse);
    task.User = JsonConvert.DeserializeObject<ApplicationUser>(userResponse);

    return Results.Ok(task);
});

app.MapPost($"api/{TaskEndpoint}", async (ApplicationTask task, ClaimsPrincipal user, HttpClient client) =>
{
    var responseMessage = await client.GetAsync($"{CategoriesApi}/{CategoryEndpoint}/{task.CategoryId}");
    var category = JsonConvert.DeserializeObject<Category>(await responseMessage.Content.ReadAsStringAsync());
    if (category is null)
    {
        return Results.BadRequest("Category not found");
    }
    task.Id = tasks.Max(x => x.Id) + 1;
    task.Category = category;
    task.UserId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    tasks.Add(task);
    return Results.Created($"api/{TaskEndpoint}", task);
}).RequireAuthorization();

app.MapPut($"api/{TaskEndpoint}/{{id:int}}", (int id, ApplicationTask taskDto) =>
{
    if (id != taskDto.Id) return Results.BadRequest();
    var task = tasks.FirstOrDefault(x => x.Id == id);
    if (task is null) return Results.NotFound();
    task.Title = taskDto.Title ?? task.Title;
    task.Description = taskDto.Description ?? task.Description;
    return Results.NoContent();
});

app.MapDelete($"api/{TaskEndpoint}/{{id:int}}", (int id) =>
{
    var task = tasks.FirstOrDefault(x => x.Id == id);
    if (task is null) return Results.NotFound();
    var removeResult = tasks.Remove(task);
    return removeResult switch
    {
        true => Results.NoContent(),
        false => Results.BadRequest()
    };
});


app.Run();
