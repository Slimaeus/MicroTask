using MicroTask.Services.Comments.Domain;

namespace MicroTask.Services.Comments.Application.Comments;
public record CommentDTO
{
    public int Id { get; set; }
    public string? Content { get; set; }
    public ApplicationUser? User { get; set; }
    public ApplicationTask? Task { get; set; }
}
