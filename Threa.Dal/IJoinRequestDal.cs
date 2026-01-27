using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace Threa.Dal;

/// <summary>
/// Data access layer for join requests.
/// </summary>
public interface IJoinRequestDal
{
    /// <summary>
    /// Gets a blank join request for creation with default values.
    /// </summary>
    JoinRequest GetBlank();

    /// <summary>
    /// Gets all active join requests for a player (excludes denied).
    /// </summary>
    Task<List<JoinRequest>> GetRequestsByPlayerAsync(int playerId);

    /// <summary>
    /// Gets all pending join requests for a specific table.
    /// </summary>
    Task<List<JoinRequest>> GetPendingRequestsForTableAsync(Guid tableId);

    /// <summary>
    /// Gets the pending join request for a specific character at a specific table.
    /// </summary>
    Task<JoinRequest?> GetPendingRequestAsync(int characterId, Guid tableId);

    /// <summary>
    /// Gets a join request by its ID.
    /// </summary>
    Task<JoinRequest?> GetRequestAsync(Guid id);

    /// <summary>
    /// Saves a join request (insert or update).
    /// </summary>
    Task<JoinRequest> SaveRequestAsync(JoinRequest request);

    /// <summary>
    /// Deletes a join request by its ID.
    /// </summary>
    Task DeleteRequestAsync(Guid id);

    /// <summary>
    /// Gets the count of pending join requests for a table.
    /// </summary>
    Task<int> GetPendingCountForTableAsync(Guid tableId);

    /// <summary>
    /// Deletes all join requests for a character (cleanup when character is deleted).
    /// </summary>
    Task DeleteRequestsByCharacterAsync(int characterId);
}
