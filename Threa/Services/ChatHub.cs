using System;
using System.Collections.Generic;
using System.Linq;

namespace Threa.Services
{
  public class ChatHub
  {
    private readonly System.Collections.ObjectModel.ObservableCollection<string> Messages =
      new System.Collections.ObjectModel.ObservableCollection<string>();

    public event Action NewMessages;

    public void SendMessage(string text)
    {
      lock (Messages)
      {
        Messages.Add(text);
        while (Messages.Count > 20)
          Messages.RemoveAt(0);
      }
      NewMessages?.Invoke();
    }

    public List<string> GetMessages()
    {
      List<string> result;
      lock (Messages)
      {
        result = Messages.ToList();
      }
      return result;
    }
  }
}
