using QuantumCore.API.Extensions;
using QuantumCore.API.Game.Types.Players;
using QuantumCore.Game.Extensions;

namespace QuantumCore.Game.Quest.Conditions;

/// <summary>
/// Checks if the player belongs to a specific class.
/// Used for class-specific quests.
/// </summary>
public class ClassCheckCondition : QuestConditionBase
{
    /// <summary>
    /// The required player class.
    /// </summary>
    public required EPlayerClass Class { get; init; }

    /// <inheritdoc />
    public override bool Evaluate(QuestConditionContext context)
    {
        return context.Player.Player.PlayerClass.GetClass() == Class;
    }
}
