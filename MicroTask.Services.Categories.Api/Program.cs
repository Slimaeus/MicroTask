using MicroTask.Services.Categories.Application.Categories.DTOs;
using MicroTask.Services.Categories.Domain;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}
app.UseSwagger();
app.UseSwaggerUI();

//app.UseHttpsRedirection();

var categories = new List<Category>
{
    new Category
    {
        Id = 1,
        Title = "Learning",
        Description = "Learning"
    },
    new Category
    {
        Id = 2,
        Title = "Relaxing",
        Description = "Relaxing"
    }
};

const string CategoryEndpoint = "Categories";

app.MapGet($"api/{CategoryEndpoint}", () =>
{
    return Results.Ok(categories);
});

app.MapGet($"api/{CategoryEndpoint}/{{id:int}}", (int id) =>
{
    var category = categories.SingleOrDefault(x => x.Id == id);
    return category switch
    {
        null => Results.NotFound(),
        _ => Results.Ok(category)
    };
});

app.MapPost($"api/{CategoryEndpoint}", (CreateCategoryDTO categoryDTO, ClaimsPrincipal user) =>
{
    var category = new Category
    {
        Id = categories.Max(x => x.Id) + 1,
        Title = categoryDTO.Title,
        Description = categoryDTO.Description
    };
    categories.Add(category);
    return Results.Created($"api/{CategoryEndpoint}", category);
});

app.MapPut($"api/{CategoryEndpoint}/{{id:int}}", (int id, Category categoryDto) =>
{
    if (id != categoryDto.Id) return Results.BadRequest();
    var category = categories.SingleOrDefault(x => x.Id == id);
    if (category is null) return Results.NotFound();
    category.Title = categoryDto.Title ?? category.Title;
    category.Description = categoryDto.Description ?? category.Description;
    return Results.NoContent();
});

app.MapDelete($"api/{CategoryEndpoint}/{{id:int}}", (int id) =>
{
    var category = categories.SingleOrDefault(x => x.Id == id);
    if (category is null) return Results.NotFound();
    var removeResult = categories.Remove(category);
    return removeResult switch
    {
        true => Results.NoContent(),
        false => Results.BadRequest()
    };
});


app.Run();
