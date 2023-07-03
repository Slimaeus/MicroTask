namespace MicroTask.Services.Categories.Application.Categories.DTOs;
public record UpdateCategoryDTO
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
}
