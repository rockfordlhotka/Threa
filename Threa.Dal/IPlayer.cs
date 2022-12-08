namespace Threa.Dal
{
  public interface IPlayer
  {
    int Id { get; set; }
    string Name { get; set; }
    string Email { get; set; }
    string ImageUrl { get; set; }
  }
}
