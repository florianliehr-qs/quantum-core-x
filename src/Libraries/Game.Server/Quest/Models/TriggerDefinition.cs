using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace QuantumCore.Game.Quest.Models;

/// <summary>
/// Represents an event trigger that can execute actions.
/// Triggers respond to game events like NPC clicks, item acquisition, etc.
/// </summary>
public class TriggerDefinition
{
    /// <summary>
    /// Type of trigger (npc_click, npc_give, kill, item_acquired, etc.)
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    /// <summary>
    /// NPC ID for npc_click and npc_give triggers
    /// </summary>
    [JsonPropertyName("npc_id")]
    public uint? NpcId { get; init; }

    /// <summary>
    /// Monster ID for kill triggers
    /// </summary>
    [JsonPropertyName("monster_id")]
    public uint? MonsterId { get; init; }

    /// <summary>
    /// Item ID for item_acquired and npc_give triggers
    /// </summary>
    [JsonPropertyName("item_id")]
    public uint? ItemId { get; init; }

    /// <summary>
    /// Condition that must be met for the trigger to fire
    /// </summary>
    [JsonPropertyName("condition")]
    public JsonObject? Condition { get; init; }

    /// <summary>
    /// Actions to execute when the trigger fires
    /// </summary>
    [JsonPropertyName("actions")]
    public required List<JsonObject> Actions { get; init; }

    /// <summary>
    /// Optional state to transition to after actions execute
    /// </summary>
    [JsonPropertyName("next_state")]
    public string? NextState { get; init; }
}
