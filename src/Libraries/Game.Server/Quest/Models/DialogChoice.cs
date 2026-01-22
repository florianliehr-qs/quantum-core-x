using System.Text.Json.Serialization;

namespace QuantumCore.Game.Quest.Models;

/// <summary>
/// Represents a choice that a player can make in quest dialog.
/// </summary>
public class DialogChoice
{
    /// <summary>
    /// The text displayed for this choice
    /// </summary>
    [JsonPropertyName("text")]
    public required string Text { get; init; }

    /// <summary>
    /// State to transition to when this choice is selected
    /// </summary>
    [JsonPropertyName("next_state")]
    public string? NextState { get; init; }

    /// <summary>
    /// Action to perform (e.g., "close" to close dialog)
    /// </summary>
    [JsonPropertyName("action")]
    public string? Action { get; init; }
}
