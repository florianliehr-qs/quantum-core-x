using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Quest.Actions;

/// <summary>
/// Teleports the player to a specific location.
/// </summary>
public class WarpAction : QuestActionBase
{
    /// <summary>
    /// Target X coordinate (in world units).
    /// </summary>
    [JsonPropertyName("x")]
    public required int X { get; init; }

    /// <summary>
    /// Target Y coordinate (in world units).
    /// </summary>
    [JsonPropertyName("y")]
    public required int Y { get; init; }

    /// <summary>
    /// Optional map name. If not specified, warps within the current map.
    /// </summary>
    [JsonPropertyName("map")]
    public string? Map { get; init; }

    public override Task ExecuteAsync(QuestActionContext context, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(Map))
        {
            var world = context.Services.GetRequiredService<IWorld>();
            var maps = world.FindMapsByName(Map);

            if (maps.Count == 0)
            {
                context.Logger.LogWarning("WarpAction: Map '{MapName}' not found for quest {QuestId}",
                    Map, context.State.QuestId);
                return Task.CompletedTask;
            }

            if (maps.Count > 1)
            {
                context.Logger.LogWarning("WarpAction: Map name '{MapName}' is ambiguous for quest {QuestId}",
                    Map, context.State.QuestId);
                return Task.CompletedTask;
            }

            // If coordinates are relative to map, add map offset
            // Otherwise, use absolute coordinates
            context.Player.Move(X, Y);
        }
        else
        {
            // Warp within current map
            context.Player.Move(X, Y);
        }

        // Refresh player visibility
        context.Player.ShowEntity(context.Player.Connection);

        return Task.CompletedTask;
    }
}
