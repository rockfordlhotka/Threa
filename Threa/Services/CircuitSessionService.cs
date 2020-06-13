using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;


namespace Threa.Services
{
  public class CircuitSessionService : CircuitHandler
  {
    public Circuit CurrentCircuit { get; private set; }
    public string SessionId { get; private set; }

    private static volatile int SessionCount;

    public int Count
    {
      get => SessionCount;
    }

    public event Action<string, bool> CircuitActive;

    public bool IsCircuitActive { get; private set; }

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
      Interlocked.Increment(ref SessionCount);
      SessionId = Guid.NewGuid().ToString();
      CurrentCircuit = circuit;
      IsCircuitActive = true;
      OnCircuitActive(SessionId, true);
      return base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
      if (circuit.Id == CurrentCircuit?.Id)
      {
        Interlocked.Decrement(ref SessionCount);
        IsCircuitActive = false;
        OnCircuitActive(SessionId, false);
        CurrentCircuit = null;
      }
      return base.OnCircuitClosedAsync(circuit, cancellationToken);
    }

    protected virtual void OnCircuitActive(string id, bool active)
    {
      CircuitActive?.Invoke(id, active);
    }
  }
}