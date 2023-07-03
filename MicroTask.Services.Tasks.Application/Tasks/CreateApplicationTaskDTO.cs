namespace MicroTask.Services.Tasks.Application.Tasks;

public record CreateApplicationTaskDTO
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? UserId { get; set; }
    public int? CategoryId { get; set; }
}
