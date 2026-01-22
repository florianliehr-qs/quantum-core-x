using System.Text.Json.Serialization;
using QuantumCore.API.Core.Models;

namespace QuantumCore.Game.Quest.Conditions;

/// <summary>
/// Checks if a specific quest has been completed by the player.
/// </summary>
public class QuestCompletedCondition : QuestConditionBase
{
    /// <summary>
    /// Quest ID to check for completion.
    /// </summary>
    [JsonPropertyName("quest_id")]
    public required string QuestId { get; init; }

    public override bool Evaluate(QuestConditionContext context)
    {
        // Check if player has this quest
        if (!context.Player.Quests.TryGetValue(QuestId, out var quest))
        {
            // Quest not found means not started, so definitely not completed
            return false;
        }

        // Check if quest has completed state
        var questStateProperty = quest.GetType().GetProperty("State");
        if (questStateProperty != null)
        {
            var questState = questStateProperty.GetValue(quest) as QuestState;
            return questState?.IsCompleted ?? false;
        }

        return false;
    }
}
