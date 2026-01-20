using System.Text.Json.Serialization;

namespace QuantumCore.Game.Quest.Models;

/// <summary>
/// Represents a complete quest definition loaded from JSON.
/// Contains all states, triggers, and quest metadata.
/// </summary>
public class QuestDefinition
{
    /// <summary>
    /// Unique identifier for the quest
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// Display name of the quest
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Quest version for tracking changes
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; init; } = "1.0.0";

    /// <summary>
    /// Optional description of the quest
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// Dictionary of quest states, keyed by state name
    /// </summary>
    [JsonPropertyName("states")]
    public required Dictionary<string, StateDefinition> States { get; init; }

    /// <summary>
    /// Optional quest metadata (requirements, rewards summary, etc.)
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }
}
