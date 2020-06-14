using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace Threa.Services
{
  public class CircuitSessionService : CircuitHandler
  {
    public Circuit CurrentCircuit { get; private set; }
    public string SessionId { get; private set; }
    public string Email { get; private set; }
    private readonly SessionList sessionList;

    public CircuitSessionService(SessionList sessionList)
    {
      this.sessionList = sessionList;
    }

    public int Count
    {
      get => sessionList.Count;
    }

    public List<string> ActiveUsers
    {
      get => sessionList.ActiveUsers;
    }

    public event Action<string, bool> CircuitActive;
    public event Action SessionCountChanged;

    public bool IsCircuitActive { get; private set; }

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
      SessionId = Guid.NewGuid().ToString();
      Email = Csla.ApplicationContext.User.Identity.Name;
      CurrentCircuit = circuit;
      IsCircuitActive = true;
      sessionList.ListChanged += SessionList_ListChanged;
      sessionList.AddSession(this);
      CircuitActive?.Invoke(SessionId, true);
      return base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
      if (circuit.Id == CurrentCircuit?.Id)
      {
        IsCircuitActive = false;
        sessionList.RemoveSession(this);
        sessionList.ListChanged -= SessionList_ListChanged;
        CircuitActive?.Invoke(SessionId, false);
        CurrentCircuit = null;
      }
      return base.OnCircuitClosedAsync(circuit, cancellationToken);
    }

    private void SessionList_ListChanged()
    {
      SessionCountChanged?.Invoke();
    }
  }
}