using System;
using System.Threading.Tasks;

namespace Threa.Dal.MockDb;

internal class ImageDal : IImageDal
{
    public Task<string> GetImage(int id)
    {
        try
        {
            return Task.FromResult(MockDb.Images[id]);
        }
        catch (Exception ex)
        {
            throw new NotFoundException($"Image {id} not found", ex);
        }
    }

    public Task<int> AddImage(string data)
    {
        MockDb.Images.Add(data);
        return Task.FromResult(MockDb.Images.Count - 1);
    }

    public Task UpdateImage(int id, string data)
    {
        try
        {
            MockDb.Images[id] = data;
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new NotFoundException($"Image {id} not found", ex);
        }
    }

    public Task DeleteImage(int id)
    {
        try
        {
            MockDb.Images.RemoveAt(id);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new NotFoundException($"Image {id} not found", ex);
        }
    }
}
