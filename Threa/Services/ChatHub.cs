using System;

namespace Threa.Services
{
  public class ChatHub
  {
    public event Action<string> NewMessage;

    public void SendMessage(string text)
    {
      NewMessage?.Invoke(text);
    }
  }
}
