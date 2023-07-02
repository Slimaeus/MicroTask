
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MicroTask.Services.Comments.Domain;
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

var comments = new List<Comment>
{
    new Comment
    {
        Id = 1,
        Content = "Hello world",
        UserId = 1
    },
    new Comment
    {
        Id = 2,
        Content = "Nice",
        UserId = 2
    }
};

const string CommentEndpoint = "Comments";
string CategoriesApi = $"http://{Environment.GetEnvironmentVariable("CATEGORIES_SERVICE")}/api" ?? "http://microtask.services.categories.api/api";

app.MapGet($"api/{CommentEndpoint}", () =>
{
    return Results.Ok(comments);
});

app.MapGet($"api/{CommentEndpoint}/{{id:int}}", async (int id, HttpClient client) =>
{
    var comment = comments.SingleOrDefault(x => x.Id == id);
    if (comment is null)
    {
        return Results.NotFound();
    }
    //var responseMessage = await client.GetAsync($"{CategoriesApi}/Categories/{comment.CategoryId}");
    //var category = JsonConvert.DeserializeObject<Category>(await responseMessage.Content.ReadAsStringAsync());
    //if (category is not null)
    //{
    //    comment.Category = category;
    //}
    return Results.Ok(comment);
});

app.MapPost($"api/{CommentEndpoint}", async (Comment comment, ClaimsPrincipal user, HttpClient client) =>
{
    //var responseMessage = await client.GetAsync($"{CategoriesApi}/Categories/{comment.CategoryId}");
    //var category = JsonConvert.DeserializeObject<Category>(await responseMessage.Content.ReadAsStringAsync());
    //if (category is null)
    //{
    //    return Results.BadRequest("Category not found");
    //}
    comment.Id = comments.Max(x => x.Id) + 1;
    comment.UserId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    comments.Add(comment);
    return Results.Created($"api/{CommentEndpoint}", comment);
}).RequireAuthorization();

app.MapPut($"api/{CommentEndpoint}/{{id:int}}", (int id, Comment commentDto) =>
{
    if (id != commentDto.Id) return Results.BadRequest();
    var comment = comments.SingleOrDefault(x => x.Id == id);
    if (comment is null) return Results.NotFound();
    comment.Content = commentDto.Content ?? comment.Content;
    return Results.NoContent();
});

app.MapDelete($"api/{CommentEndpoint}/{{id:int}}", (int id) =>
{
    var comment = comments.SingleOrDefault(x => x.Id == id);
    if (comment is null) return Results.NotFound();
    var removeResult = comments.Remove(comment);
    return removeResult switch
    {
        true => Results.NoContent(),
        false => Results.BadRequest()
    };
});


app.Run();
