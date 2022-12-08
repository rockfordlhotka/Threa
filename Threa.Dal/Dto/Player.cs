namespace Threa.Dal.Dto
{
  public class Player : IPlayer
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string ImageUrl { get; set; }
  }
}
