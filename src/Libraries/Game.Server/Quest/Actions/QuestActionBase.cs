using Microsoft.Extensions.DependencyInjection;
using QuantumCore.API;
using QuantumCore.Game.Persistence;
using QuantumCore.Game.World;

namespace QuantumCore.Game.Quest.Actions;

/// <summary>
/// Abstract base class for quest actions providing common helper methods.
/// </summary>
public abstract class QuestActionBase : IQuestAction
{
    /// <inheritdoc />
    public abstract Task ExecuteAsync(QuestActionContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the item manager from the service provider.
    /// </summary>
    protected IItemManager GetItemManager(QuestActionContext context)
        => context.Services.GetRequiredService<IItemManager>();

    /// <summary>
    /// Gets the quest repository from the service provider.
    /// </summary>
    protected IDbQuestRepository GetQuestRepository(QuestActionContext context)
        => context.Services.GetRequiredService<IDbQuestRepository>();
}
