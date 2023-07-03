
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MicroTask.Services.Comments.Application.Comments;
using MicroTask.Services.Comments.Domain;
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

var comments = new List<Comment>
{
    new Comment
    {
        Id = 1,
        Content = "Hello world",
        UserId = 1,
        TaskId = 1
    },
    new Comment
    {
        Id = 2,
        Content = "Nice",
        UserId = 2,
        TaskId = 2
    }
};

const string CommentEndpoint = "Comments";
const string TaskEndpoint = "Tasks";
const string UserEndpoint = "Users";
string TasksApi = Environment.GetEnvironmentVariable("TASKS_SERVICE") is not null ? $"http://{Environment.GetEnvironmentVariable("TASKS_SERVICE")}/api" : "http://microtask.services.tasks.api/api";
string UsersApi = Environment.GetEnvironmentVariable("USERS_SERVICE") is not null ? $"http://{Environment.GetEnvironmentVariable("USERS_SERVICE")}/api" : "http://microtask.services.users.api/api";

app.MapGet($"api/{CommentEndpoint}", () =>
{
    var commentDTOs = comments.Select(comment => new CommentDTO { Id = comment.Id, Content = comment.Content });
    return Results.Ok(commentDTOs);
});

app.MapGet($"api/{CommentEndpoint}/{{id:int}}", async (int id, HttpClient client) =>
{
    var comment = comments.FirstOrDefault(x => x.Id == id);
    if (comment is null)
    {
        return Results.NotFound();
    }

    var tasks = new Task<HttpResponseMessage>[]
    {
        client.GetAsync($"{TasksApi}/{TaskEndpoint}/{comment.TaskId}"),
        client.GetAsync($"{UsersApi}/{UserEndpoint}/{comment.UserId}")
    };

    var responseMessages = await Task.WhenAll(tasks);

    var taskResponse = await responseMessages[0].Content.ReadAsStringAsync();
    var userResponse = await responseMessages[1].Content.ReadAsStringAsync();

    comment.Task = JsonConvert.DeserializeObject<ApplicationTask>(taskResponse);
    comment.User = JsonConvert.DeserializeObject<ApplicationUser>(userResponse);

    return Results.Ok(new CommentDTO { Id = comment.Id, Content = comment.Content, Task = comment.Task, User = comment.User });
});


app.MapPost($"api/{CommentEndpoint}", async (CreateCommentDTO createCommentDTO, ClaimsPrincipal claimsPrincipal, HttpClient client) =>
{
    var tasks = new Task<HttpResponseMessage>[]
    {
        client.GetAsync($"{TasksApi}/{TaskEndpoint}/{createCommentDTO.TaskId}"),
        client.GetAsync($"{UsersApi}/{UserEndpoint}/{createCommentDTO.UserId}")
    };

    var responseMessages = await Task.WhenAll(tasks);

    var taskResponse = await responseMessages[0].Content.ReadAsStringAsync();
    var userResponse = await responseMessages[1].Content.ReadAsStringAsync();

    var task = JsonConvert.DeserializeObject<ApplicationTask>(taskResponse);
    if (task is null)
    {
        return Results.BadRequest("Task not found");
    }
    var user = JsonConvert.DeserializeObject<ApplicationUser>(userResponse);
    if (user is null)
    {
        return Results.BadRequest("User not found");
    }
    var comment = new Comment();
    comment.Id = comments.Max(x => x.Id) + 1;
    comment.Content = comment.Content;
    comment.TaskId = task.Id;
    comment.UserId = user.Id;
    comments.Add(comment);
    return Results.Created($"api/{CommentEndpoint}", new CommentDTO { Id = comment.Id, Content = comment.Content, Task = task, User = user });
});

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
    var comment = comments.FirstOrDefault(x => x.Id == id);
    if (comment is null) return Results.NotFound();
    var removeResult = comments.Remove(comment);
    return removeResult switch
    {
        true => Results.NoContent(),
        false => Results.BadRequest()
    };
});


app.Run();
