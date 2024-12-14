using System.Threading.Tasks;

namespace Threa.Dal;

public interface IImageDal
{
    Task<string> GetImage(int id);
    Task UpdateImage(int id, string data);
    Task<int> AddImage(string data);
    Task DeleteImage(int id);

}
