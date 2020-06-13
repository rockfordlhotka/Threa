using Microsoft.AspNetCore.Components.Server.Circuits;
using System;
using System.Collections.Generic;
using System.Timers;

namespace Threa.Services
{
  public class ChatService
  {
    public event Action NewMessages;

    private Timer timer;
    private ChatHub hub;

    public ChatService(CircuitHandler circuit, ChatHub chatHub)
    {
      hub = chatHub;
      hub.NewMessages += App_NewMessages;
      var myCircuit = ((CircuitSessionService)circuit);
      myCircuit.CircuitActive += (id, active) =>
      {
        if (active)
        {
          if (!string.IsNullOrWhiteSpace(id))
          {
            timer?.Stop();
            timer = new Timer(3000)
            {
              AutoReset = true,
              Enabled = true
            };
          }
        }
        else
        {
          hub.NewMessages -= App_NewMessages;
          timer?.Stop();
          timer = null;
        }
      };
    }

    public void SendMessage(string message)
    {
      hub.SendMessage(message);
    }

    public List<string> GetMessages()
    {
      return hub.GetMessages();
    }

    private void App_NewMessages()
    {
      NewMessages?.Invoke();
    }
  }
}
