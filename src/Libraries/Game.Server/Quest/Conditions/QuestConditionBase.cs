using Microsoft.Extensions.DependencyInjection;
using QuantumCore.API;
using QuantumCore.Game.World;

namespace QuantumCore.Game.Quest.Conditions;

/// <summary>
/// Abstract base class for quest conditions providing common helper methods.
/// </summary>
public abstract class QuestConditionBase : IQuestCondition
{
    /// <inheritdoc />
    public abstract bool Evaluate(QuestConditionContext context);

    /// <summary>
    /// Gets the item manager from the service provider.
    /// </summary>
    protected IItemManager GetItemManager(QuestConditionContext context)
        => context.Services.GetRequiredService<IItemManager>();
}
