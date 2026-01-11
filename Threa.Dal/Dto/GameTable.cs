using System;
using System.Collections.Generic;

namespace Threa.Dal.Dto;

/// <summary>
/// Represents an active game session (table) where players and GM interact.
/// </summary>
public class GameTable
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int GameMasterId { get; set; }
    public int? CampaignId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public TableStatus Status { get; set; }

    // Time state
    public int CurrentRound { get; set; }
    public bool IsInCombat { get; set; }
    public DateTime? CombatStartedAt { get; set; }
    public DateTime? LastTimeAdvance { get; set; }

    // Connected characters (managed separately for efficiency)
    public List<TableCharacter> Characters { get; set; } = [];

    // NPCs at this table (managed separately for efficiency)
    public List<TableNpc> Npcs { get; set; } = [];
}

/// <summary>
/// Status of a game table.
/// </summary>
public enum TableStatus
{
    Lobby = 0,
    Active = 1,
    Paused = 2,
    Ended = 3
}

/// <summary>
/// Represents a character connected to a table.
/// </summary>
public class TableCharacter
{
    public Guid TableId { get; set; }
    public int CharacterId { get; set; }
    public int PlayerId { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public ConnectionStatus ConnectionStatus { get; set; }
    public DateTime? LastActivity { get; set; }
}

/// <summary>
/// Connection status of a player/character at a table.
/// </summary>
public enum ConnectionStatus
{
    Connected = 0,
    Disconnected = 1,
    Away = 2
}

/// <summary>
/// Represents an NPC at a table (simplified stat block).
/// </summary>
public class TableNpc
{
    public Guid Id { get; set; }
    public Guid TableId { get; set; }
    public string Name { get; set; } = string.Empty;

    // Core stats for quick reference
    public int VitValue { get; set; }
    public int VitBaseValue { get; set; }
    public int FatValue { get; set; }
    public int FatBaseValue { get; set; }
    public int ActionPointMax { get; set; }
    public int ActionPointAvailable { get; set; }

    // Optional reference to a full character if this NPC is based on one
    public int? CharacterTemplateId { get; set; }

    // JSON blob for additional stats/data
    public string StatsJson { get; set; } = string.Empty;
}
