namespace UrlShort.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; }
    public bool IsAdmin { get; set; } = false;
    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }
}