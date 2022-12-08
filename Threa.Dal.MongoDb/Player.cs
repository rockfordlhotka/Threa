using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Threa.Dal.MongoDb
{
  public class Player :IPlayer
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
  }
}
