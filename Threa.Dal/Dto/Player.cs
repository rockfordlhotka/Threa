namespace Threa.Dal.Dto;

public class Player
{
    public int Id { get; set; } = -1;
    public string Name { get; set; }
    public string Salt { get; set; }
    public string HashedPassword { get; set; }
    public string Email { get; set; }
    public string ImageUrl { get; set; }
}
