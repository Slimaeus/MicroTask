using MicroTask.Services.Users.Domain;
using MicroTask.Services.Users.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<TokenGenerator>();
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

var users = new List<ApplicationUser>
{
    new ApplicationUser
    {
        Id = 1,
        UserName = "admin",
        Password = "admin"
    },
    new ApplicationUser
    {
        Id = 2,
        UserName = "user",
        Password = "user"
    }
};

app.MapPost("api/Users/login", (TokenGenerator tokenGenerator, ApplicationUser userDto) =>
{
    var user = users.SingleOrDefault(x => x.UserName == userDto.UserName);
    if (user is null) return Results.NotFound();
    if (user.Password != userDto.Password) return Results.Unauthorized();
    var token = tokenGenerator.GenerateToken(user);
    return Results.Ok(token);
});

app.MapGet("api/Tasks", async (HttpClient client) =>
{
    var result = await client.GetAsync("http://microtask.services.tasks.api/api/tasks");
    return Results.Ok(result);
});

app.Run();