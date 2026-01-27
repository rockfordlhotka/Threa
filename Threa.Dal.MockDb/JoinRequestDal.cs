using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace Threa.Dal.MockDb;

/// <summary>
/// MockDb implementation of join request data access.
/// </summary>
public class JoinRequestDal : IJoinRequestDal
{
    public JoinRequest GetBlank()
    {
        return new JoinRequest
        {
            Id = Guid.NewGuid(),
            RequestedAt = DateTime.UtcNow,
            Status = JoinRequestStatus.Pending
        };
    }

    public Task<List<JoinRequest>> GetRequestsByPlayerAsync(int playerId)
    {
        var requests = MockDb.JoinRequests
            .Where(r => r.PlayerId == playerId && r.Status != JoinRequestStatus.Denied)
            .ToList();
        return Task.FromResult(requests);
    }

    public Task<List<JoinRequest>> GetPendingRequestsForTableAsync(Guid tableId)
    {
        var requests = MockDb.JoinRequests
            .Where(r => r.TableId == tableId && r.Status == JoinRequestStatus.Pending)
            .ToList();
        return Task.FromResult(requests);
    }

    public Task<JoinRequest?> GetPendingRequestAsync(int characterId, Guid tableId)
    {
        var request = MockDb.JoinRequests
            .FirstOrDefault(r => r.CharacterId == characterId
                && r.TableId == tableId
                && r.Status == JoinRequestStatus.Pending);
        return Task.FromResult(request);
    }

    public Task<JoinRequest?> GetRequestAsync(Guid id)
    {
        var request = MockDb.JoinRequests.FirstOrDefault(r => r.Id == id);
        return Task.FromResult(request);
    }

    public Task<JoinRequest> SaveRequestAsync(JoinRequest request)
    {
        lock (MockDb.JoinRequests)
        {
            var existing = MockDb.JoinRequests.FirstOrDefault(r => r.Id == request.Id);
            if (existing == null)
            {
                MockDb.JoinRequests.Add(request);
            }
            else if (!ReferenceEquals(existing, request))
            {
                MockDb.JoinRequests.Remove(existing);
                MockDb.JoinRequests.Add(request);
            }
        }
        return Task.FromResult(request);
    }

    public Task DeleteRequestAsync(Guid id)
    {
        MockDb.JoinRequests.RemoveAll(r => r.Id == id);
        return Task.CompletedTask;
    }

    public Task<int> GetPendingCountForTableAsync(Guid tableId)
    {
        var count = MockDb.JoinRequests
            .Count(r => r.TableId == tableId && r.Status == JoinRequestStatus.Pending);
        return Task.FromResult(count);
    }

    public Task DeleteRequestsByCharacterAsync(int characterId)
    {
        MockDb.JoinRequests.RemoveAll(r => r.CharacterId == characterId);
        return Task.CompletedTask;
    }
}
