using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace QuantumCore.Game.Quest.Models;

/// <summary>
/// Represents a single state in a quest state machine.
/// Contains triggers for events and actions to execute on state entry/exit.
/// </summary>
public class StateDefinition
{
    /// <summary>
    /// Actions to execute when entering this state
    /// </summary>
    [JsonPropertyName("on_enter")]
    public List<JsonObject>? OnEnter { get; init; }

    /// <summary>
    /// Actions to execute when exiting this state
    /// </summary>
    [JsonPropertyName("on_exit")]
    public List<JsonObject>? OnExit { get; init; }

    /// <summary>
    /// List of event triggers for this state
    /// </summary>
    [JsonPropertyName("triggers")]
    public List<TriggerDefinition>? Triggers { get; init; }
}
