using System.Text.Json.Serialization;

namespace QuantumCore.Game.Quest.Conditions;

/// <summary>
/// Checks if current time (hour) is within a specified range.
/// Useful for time-limited quests or daily quests.
/// </summary>
public class TimeCheckCondition : QuestConditionBase
{
    /// <summary>
    /// Start hour (0-23, inclusive).
    /// </summary>
    [JsonPropertyName("start_hour")]
    public required byte StartHour { get; init; }

    /// <summary>
    /// End hour (0-23, inclusive).
    /// </summary>
    [JsonPropertyName("end_hour")]
    public required byte EndHour { get; init; }

    public override bool Evaluate(QuestConditionContext context)
    {
        if (StartHour > 23 || EndHour > 23)
        {
            return false;
        }

        var currentHour = DateTime.Now.Hour;

        // Handle ranges that cross midnight (e.g., 22:00 to 02:00)
        if (StartHour <= EndHour)
        {
            // Normal range (e.g., 08:00 to 18:00)
            return currentHour >= StartHour && currentHour <= EndHour;
        }
        else
        {
            // Range crosses midnight (e.g., 22:00 to 02:00)
            return currentHour >= StartHour || currentHour <= EndHour;
        }
    }
}
