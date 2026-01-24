using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QuantumCore.Game.Persistence.Entities;

/// <summary>
/// Represents a quest timer for delayed quest actions.
/// Timers are persisted to survive server restarts.
/// </summary>
public class QuestTimer
{
    public required Guid Id { get; set; } = Guid.NewGuid();
    public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public required DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The quest data this timer belongs to.
    /// </summary>
    public required Guid QuestDataId { get; set; }

    /// <summary>
    /// The timer name (e.g., "hint_timer", "respawn_timer").
    /// </summary>
    public required string Name { get; set; } = "";

    /// <summary>
    /// When the timer should trigger (UTC).
    /// </summary>
    public required DateTime TriggerAt { get; set; }

    /// <summary>
    /// Whether this timer has already been processed.
    /// </summary>
    public required bool IsProcessed { get; set; }

    /// <summary>
    /// Navigation property to the quest data.
    /// </summary>
    public QuestData? QuestData { get; set; }

    public static void Configure(EntityTypeBuilder<QuestTimer> builder, DatabaseFacade database)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.QuestDataId, x.Name }).IsUnique();
        builder.HasIndex(x => new { x.TriggerAt, x.IsProcessed }); // For efficient timer queries

        builder.Property(x => x.Name).HasMaxLength(128);
        builder.Property(x => x.IsProcessed).HasDefaultValue(false);

        if (database.IsSqlite() || database.IsNpgsql())
        {
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("current_timestamp");
            builder.Property(x => x.UpdatedAt).HasDefaultValueSql("current_timestamp");
        }
        else if (database.IsMySql())
        {
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))");
            builder.Property(x => x.UpdatedAt).HasDefaultValueSql("(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))");
        }

        builder.HasOne(x => x.QuestData)
            .WithMany(x => x.Timers)
            .HasForeignKey(x => x.QuestDataId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
