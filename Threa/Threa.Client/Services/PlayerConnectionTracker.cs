using System.Collections.Concurrent;
using GameMechanics.Messaging;
using Threa.Dal;
using Threa.Dal.Dto;

namespace Threa.Client.Services;

/// <summary>
/// Tracks which players/characters are currently connected via active Blazor circuits.
/// Updates the ConnectionStatus in the database when players connect/disconnect.
/// Publishes CharacterUpdateMessage so the GM table page refreshes in real time.
/// </summary>
public class PlayerConnectionTracker
{
    private readonly IServiceProvider _serviceProvider;
    // Map from CharacterId to list of circuit IDs (a character can have multiple tabs open)
    private readonly ConcurrentDictionary<int, HashSet<string>> _characterCircuits = new();
    // Map from circuit ID to character/table info
    private readonly ConcurrentDictionary<string, PlayerConnection> _circuitToConnection = new();
    private readonly object _lock = new();

    public PlayerConnectionTracker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Records that a player connected with a specific character at a table.
    /// </summary>
    public async Task RegisterConnectionAsync(string circuitId, int characterId, Guid tableId)
    {
        bool isFirstConnection = false;

        lock (_lock)
        {
            // Add circuit to character's circuit list
            if (!_characterCircuits.TryGetValue(characterId, out var circuits))
            {
                circuits = new HashSet<string>();
                _characterCircuits[characterId] = circuits;
                isFirstConnection = true; // This is the first connection for this character
            }
            
            circuits.Add(circuitId);
            _circuitToConnection[circuitId] = new PlayerConnection(characterId, tableId);
        }

        // Update database only if this is the first connection for this character
        if (isFirstConnection)
        {
            await UpdateConnectionStatusAsync(characterId, tableId, ConnectionStatus.Connected);
        }
    }

    /// <summary>
    /// Records that a player's circuit closed (disconnected).
    /// </summary>
    public async Task UnregisterConnectionAsync(string circuitId)
    {
        bool isLastConnection = false;
        int characterId = 0;
        Guid tableId = Guid.Empty;

        lock (_lock)
        {
            if (_circuitToConnection.TryRemove(circuitId, out var connection))
            {
                characterId = connection.CharacterId;
                tableId = connection.TableId;

                if (_characterCircuits.TryGetValue(characterId, out var circuits))
                {
                    circuits.Remove(circuitId);
                    
                    // If no more circuits for this character, remove the entry
                    if (circuits.Count == 0)
                    {
                        _characterCircuits.TryRemove(characterId, out _);
                        isLastConnection = true; // This was the last connection for this character
                    }
                }
            }
        }

        // Update database only if this was the last connection for this character
        if (isLastConnection)
        {
            await UpdateConnectionStatusAsync(characterId, tableId, ConnectionStatus.Disconnected);
        }
    }

    /// <summary>
    /// Checks if a character is currently connected.
    /// </summary>
    public bool IsCharacterConnected(int characterId)
    {
        lock (_lock)
        {
            return _characterCircuits.ContainsKey(characterId);
        }
    }

    private async Task UpdateConnectionStatusAsync(int characterId, Guid tableId, ConnectionStatus status)
    {
        try
        {
            // Create a scope to get scoped services
            using var scope = _serviceProvider.CreateScope();
            var tableDal = scope.ServiceProvider.GetRequiredService<ITableDal>();

            // Update the connection status in the database
            await tableDal.UpdateCharacterConnectionStatusAsync(tableId, characterId, status, DateTime.UtcNow);

            Console.WriteLine($"[PlayerConnectionTracker] Updated character {characterId} at table {tableId} to {status}");

            // Notify the GM table page so it refreshes the character cards
            var publisher = scope.ServiceProvider.GetRequiredService<ITimeEventPublisher>();
            if (publisher.IsConnected)
            {
                await publisher.PublishCharacterUpdateAsync(new CharacterUpdateMessage
                {
                    CampaignId = tableId.ToString(),
                    CharacterId = characterId,
                    UpdateType = CharacterUpdateType.General,
                    Description = status == ConnectionStatus.Connected ? "Player connected" : "Player disconnected"
                });
            }
        }
        catch (Exception ex)
        {
            // Log error but don't throw - connection tracking is not critical
            Console.WriteLine($"[PlayerConnectionTracker] Error updating connection status: {ex}");
        }
    }

    private record PlayerConnection(int CharacterId, Guid TableId);
}
