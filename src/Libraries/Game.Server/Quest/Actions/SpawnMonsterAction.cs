using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Services;
using QuantumCore.Game.World;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Quest.Actions;

/// <summary>
/// Spawns a monster at a specific location.
/// </summary>
public class SpawnMonsterAction : QuestActionBase
{
    /// <summary>
    /// Monster ID (vnum) to spawn.
    /// </summary>
    [JsonPropertyName("monster_id")]
    public required uint MonsterId { get; init; }

    /// <summary>
    /// Target X coordinate (in world units). If not specified, spawns at player's current X position.
    /// </summary>
    [JsonPropertyName("x")]
    public int? X { get; init; }

    /// <summary>
    /// Target Y coordinate (in world units). If not specified, spawns at player's current Y position.
    /// </summary>
    [JsonPropertyName("y")]
    public int? Y { get; init; }

    /// <summary>
    /// Spawn range for randomization. Default is 0 (exact position).
    /// </summary>
    [JsonPropertyName("range")]
    public int Range { get; init; } = 0;

    public override Task ExecuteAsync(QuestActionContext context, CancellationToken cancellationToken = default)
    {
        var monsterManager = context.Services.GetRequiredService<IMonsterManager>();
        var dropProvider = context.Services.GetRequiredService<IDropProvider>();
        var animationManager = context.Services.GetRequiredService<IAnimationManager>();
        var itemManager = context.Services.GetRequiredService<IItemManager>();

        var monsterData = monsterManager.GetMonster(MonsterId);
        if (monsterData == null)
        {
            context.Logger.LogWarning("SpawnMonsterAction: Monster {MonsterId} not found for quest {QuestId}",
                MonsterId, context.State.QuestId);
            return Task.CompletedTask;
        }

        if (context.Player.Map == null)
        {
            context.Logger.LogWarning("SpawnMonsterAction: Player's map is null for quest {QuestId}",
                context.State.QuestId);
            return Task.CompletedTask;
        }

        // Determine spawn position
        var spawnX = X ?? context.Player.PositionX;
        var spawnY = Y ?? context.Player.PositionY;

        // Apply random range if specified
        if (Range > 0)
        {
            var random = new Random();
            spawnX += random.Next(-Range, Range);
            spawnY += random.Next(-Range, Range);
        }

        // Create monster entity
        var monster = new MonsterEntity(
            monsterManager,
            dropProvider,
            animationManager,
            context.Services,
            context.Player.Map,
            context.Logger,
            itemManager,
            MonsterId,
            spawnX,
            spawnY
        );

        // Spawn the monster
        context.Player.Map.SpawnEntity(monster);

        context.Logger.LogDebug("SpawnMonsterAction: Spawned monster {MonsterId} at ({X}, {Y}) for quest {QuestId}",
            MonsterId, spawnX, spawnY, context.State.QuestId);

        return Task.CompletedTask;
    }
}
