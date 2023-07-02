namespace MicroTask.Services.Comments.Domain;

public class ApplicationTask
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? UserId { get; set; }
    public int? CategoryId { get; set; }
}
