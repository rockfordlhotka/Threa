using Microsoft.AspNetCore.Components.Server.Circuits;
using System;

namespace Threa.Services
{
  public class ChatService
  {
    public event Action<string> NewMessage;

    private readonly ChatHub hub;
    public bool IsActive { get; private set; }

    public ChatService(CircuitHandler circuit, ChatHub chatHub)
    {
      hub = chatHub;
      var myCircuit = ((CircuitSessionService)circuit);
      myCircuit.CircuitActive += (id, active) =>
      {
        if (active && !string.IsNullOrWhiteSpace(id))
        {
          ActivateService();
        }
        else
        {
          active = false;
          hub.NewMessage -= App_NewMessage;
        }
      };
      ActivateService();
    }

    private void ActivateService()
    {
      if (!IsActive)
      {
        hub.NewMessage += App_NewMessage;
      }
      IsActive = true;
    }

    public void SendMessage(string message)
    {
      hub.SendMessage(message);
    }

    private void App_NewMessage(string message)
    {
      NewMessage?.Invoke(message);
    }
  }
}
