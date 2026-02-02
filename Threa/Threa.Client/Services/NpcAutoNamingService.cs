namespace Threa.Services;

/// <summary>
/// Session-scoped service for generating unique NPC names.
/// Uses a global counter across all NPCs with template-specific prefix memory.
/// Per CONTEXT.md: "GLOBAL counter across all NPCs (not per-template)"
/// </summary>
public class NpcAutoNamingService
{
    private int _globalCounter = 0;
    private readonly Dictionary<int, string> _templatePrefixes = new();

    /// <summary>
    /// Generates the next unique name for an NPC from the given template.
    /// </summary>
    /// <param name="templateId">Template ID for prefix lookup.</param>
    /// <param name="defaultPrefix">Default prefix (template name) if not previously set.</param>
    /// <returns>Generated name like "Goblin 1", "Bandit 2".</returns>
    public string GenerateName(int templateId, string defaultPrefix)
    {
        _globalCounter++;
        var prefix = GetOrSetPrefix(templateId, defaultPrefix);
        return $"{prefix} {_globalCounter}";
    }

    /// <summary>
    /// Gets the current prefix for a template, or sets it if not yet defined.
    /// </summary>
    public string GetOrSetPrefix(int templateId, string defaultPrefix)
    {
        if (!_templatePrefixes.TryGetValue(templateId, out var prefix))
        {
            prefix = defaultPrefix;
            _templatePrefixes[templateId] = prefix;
        }
        return prefix;
    }

    /// <summary>
    /// Updates the prefix for a template. Called when GM customizes the name.
    /// Extracts prefix from name like "Bandit Leader 3" -> "Bandit Leader".
    /// </summary>
    public void UpdatePrefixFromName(int templateId, string fullName)
    {
        // Extract prefix by removing trailing number
        var match = System.Text.RegularExpressions.Regex.Match(fullName, @"^(.+?)\s*\d*$");
        if (match.Success && !string.IsNullOrWhiteSpace(match.Groups[1].Value))
        {
            _templatePrefixes[templateId] = match.Groups[1].Value.Trim();
        }
    }

    /// <summary>
    /// Gets the current global counter value (for debugging/testing).
    /// </summary>
    public int CurrentCounter => _globalCounter;
}
