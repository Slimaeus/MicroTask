namespace MicroTask.Services.Categories.Application.Categories.DTOs;
public record CreateCategoryDTO
{
    public string? Title { get; set; }
    public string? Description { get; set; }
}
