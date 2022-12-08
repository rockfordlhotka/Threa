using MongoDB.Driver;

namespace Threa.Dal.MongoDb
{
  public class ThreaDbContext
  {
    private IMongoDatabase Db { get; set; }

    public ThreaDbContext(ThreaDbSettings config)
    {
      var mongoClient = new MongoClient(config.Connection);
      Db = mongoClient.GetDatabase(config.DatabaseName);
    }

    public IMongoCollection<T> GetCollection<T>(string name)
    {
      return Db.GetCollection<T>(name);
    }

    public IMongoCollection<T> GetCollection<T>()
    {
      return Db.GetCollection<T>($"{typeof(T).Name}");
    }
  }
}
