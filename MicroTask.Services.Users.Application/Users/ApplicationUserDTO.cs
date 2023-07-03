namespace MicroTask.Services.Users.Application.Users;

public record ApplicationUserDTO
{
    public int Id { get; set; }
    public string? UserName { get; set; }
}
