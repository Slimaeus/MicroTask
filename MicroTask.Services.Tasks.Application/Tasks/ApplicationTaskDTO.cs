using MicroTask.Services.Tasks.Domain;

namespace MicroTask.Services.Tasks.Application.Tasks;

public record ApplicationTaskDTO
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public ApplicationUser? User { get; set; }
    public Category? Category { get; set; }
}
