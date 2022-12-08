using System;
using System.Collections.Generic;
using System.Linq;

namespace Threa.Services
{
  public class SessionList
  {
    private List<CircuitSessionService> sessions = new List<CircuitSessionService>();

    public event Action ListChanged;

    public int Count 
    { 
      get
      {
        lock (sessions)
        {
          return sessions.Count;
        }
      }
    }

    public List<string> ActiveUsers
    {
      get
      {
        lock (sessions)
        {
          return sessions.Select(r => r.Email).Distinct().ToList();
        }
      }
    }

    public void AddSession(CircuitSessionService circuit)
    {
      lock (sessions)
      {
        sessions.Add(circuit);
      }
      ListChanged?.Invoke();
    }

    public void RemoveSession(CircuitSessionService circuit)
    {
      lock (sessions)
      {
        sessions.Remove(circuit);
      }
      ListChanged?.Invoke();
    }
  }
}
