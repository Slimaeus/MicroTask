using MicroTask.Services.Users.Domain;

namespace MicroTask.Services.Comments.Domain;

public class Comment
{
    public int Id { get; set; }
    public string? Content { get; set; }
    public int UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public int TaskId { get; set; }
    public ApplicationTask? Task { get; set; }
}
