using System.Text.Json.Serialization;

namespace QuantumCore.Game.Quest.Models;

/// <summary>
/// Represents a single page of quest dialog.
/// Can display text and optionally present choices or a next button.
/// </summary>
public class DialogPage
{
    /// <summary>
    /// The text to display to the player
    /// </summary>
    [JsonPropertyName("text")]
    public required string Text { get; init; }

    /// <summary>
    /// If true, shows a "Next" button to advance to the next page
    /// </summary>
    [JsonPropertyName("next")]
    public bool Next { get; init; }

    /// <summary>
    /// List of choices to present to the player
    /// </summary>
    [JsonPropertyName("choices")]
    public List<DialogChoice>? Choices { get; init; }

    /// <summary>
    /// Optional quest skin to use for this dialog page
    /// </summary>
    [JsonPropertyName("skin")]
    public string? Skin { get; init; }
}
