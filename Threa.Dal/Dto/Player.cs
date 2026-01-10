namespace Threa.Dal.Dto;

public class Player
{
    public int Id { get; set; } = -1;
    public string Name { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public string HashedPassword { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string? Roles { get; set; }
}
