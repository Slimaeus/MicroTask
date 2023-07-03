namespace MicroTask.Services.Categories.Application.Categories.DTOs;
public record CategoryDTO
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
}
