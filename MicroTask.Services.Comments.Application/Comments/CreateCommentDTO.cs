namespace MicroTask.Services.Comments.Application.Comments;

public record CreateCommentDTO
{
    public string? Content { get; set; }
    public int UserId { get; set; }
    public int TaskId { get; set; }
}
