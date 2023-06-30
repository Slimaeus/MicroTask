namespace MicroTask.Services.Users.Domain;

public class ApplicationUser
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
