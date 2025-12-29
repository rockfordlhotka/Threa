using System.Collections.Generic;
using System.Linq;

namespace GameMechanics.Actions;

/// <summary>
/// Aggregates all modifiers that affect an Ability Score for an action.
/// Provides breakdown for UI display and total calculation.
/// </summary>
public class ModifierStack
{
    private readonly List<AsModifier> _modifiers = new();

    /// <summary>
    /// Gets all modifiers in the stack.
    /// </summary>
    public IReadOnlyList<AsModifier> Modifiers => _modifiers.AsReadOnly();

    /// <summary>
    /// Gets the total of all modifiers.
    /// </summary>
    public int Total => _modifiers.Sum(m => m.Value);

    /// <summary>
    /// Gets the total of all bonuses (positive modifiers).
    /// </summary>
    public int TotalBonuses => _modifiers.Where(m => m.Value > 0).Sum(m => m.Value);

    /// <summary>
    /// Gets the total of all penalties (negative modifiers).
    /// </summary>
    public int TotalPenalties => _modifiers.Where(m => m.Value < 0).Sum(m => m.Value);

    /// <summary>
    /// Adds a modifier to the stack.
    /// </summary>
    public void Add(AsModifier modifier)
    {
        _modifiers.Add(modifier);
    }

    /// <summary>
    /// Adds a modifier to the stack.
    /// </summary>
    public void Add(ModifierSource source, string description, int value)
    {
        _modifiers.Add(new AsModifier(source, description, value));
    }

    /// <summary>
    /// Adds multiple modifiers to the stack.
    /// </summary>
    public void AddRange(IEnumerable<AsModifier> modifiers)
    {
        _modifiers.AddRange(modifiers);
    }

    /// <summary>
    /// Removes all modifiers from a specific source.
    /// </summary>
    public void RemoveBySource(ModifierSource source)
    {
        _modifiers.RemoveAll(m => m.Source == source);
    }

    /// <summary>
    /// Gets modifiers from a specific source.
    /// </summary>
    public IEnumerable<AsModifier> GetBySource(ModifierSource source)
    {
        return _modifiers.Where(m => m.Source == source);
    }

    /// <summary>
    /// Gets the sum of modifiers from a specific source.
    /// </summary>
    public int GetTotalBySource(ModifierSource source)
    {
        return _modifiers.Where(m => m.Source == source).Sum(m => m.Value);
    }

    /// <summary>
    /// Clears all modifiers from the stack.
    /// </summary>
    public void Clear()
    {
        _modifiers.Clear();
    }

    /// <summary>
    /// Creates a copy of this modifier stack.
    /// </summary>
    public ModifierStack Clone()
    {
        var clone = new ModifierStack();
        clone._modifiers.AddRange(_modifiers.Select(m => new AsModifier(m.Source, m.Description, m.Value)));
        return clone;
    }

    /// <summary>
    /// Gets a formatted breakdown string for display.
    /// </summary>
    public string GetBreakdown()
    {
        if (_modifiers.Count == 0)
            return "No modifiers";

        var lines = _modifiers.Select(m => m.ToString());
        return string.Join("\n", lines) + $"\n---\nTotal: {(Total >= 0 ? "+" : "")}{Total}";
    }
}
