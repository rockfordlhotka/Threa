namespace Threa.Services;

using Microsoft.AspNetCore.Components.Server.Circuits;
using Threa.Client.Services;

public class ActiveCircuitHandler : CircuitHandler
{
    private readonly ActiveCircuitState _state;
    private readonly PlayerConnectionTracker _connectionTracker;
    private readonly CircuitIdProvider _circuitIdProvider;

    public ActiveCircuitHandler(ActiveCircuitState state, PlayerConnectionTracker connectionTracker, CircuitIdProvider circuitIdProvider)
    {
        _state = state;
        _connectionTracker = connectionTracker;
        _circuitIdProvider = circuitIdProvider;
    }

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _state.CircuitExists = true;
        _circuitIdProvider.CircuitId = circuit.Id;
        // Note: We can't register the connection here because we don't know which character/table yet.
        // The Play page will call RegisterConnectionAsync when it initializes.
        return base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _state.CircuitExists = false;
        // Unregister any connections associated with this circuit
        _ = _connectionTracker.UnregisterConnectionAsync(circuit.Id);
        return base.OnCircuitClosedAsync(circuit, cancellationToken);
    }
}
