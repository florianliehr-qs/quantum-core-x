using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QuantumCore.Game.Persistence.Entities;

/// <summary>
/// Represents persistent quest data for a player's quest progress.
/// </summary>
public class QuestData
{
    public required Guid Id { get; set; } = Guid.NewGuid();
    public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public required DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The player ID this quest data belongs to.
    /// </summary>
    public required uint PlayerId { get; set; }

    /// <summary>
    /// The quest identifier (typically the quest class name).
    /// </summary>
    public required string QuestName { get; set; } = "";

    /// <summary>
    /// Current state index within the quest (for multi-state quests).
    /// </summary>
    [DefaultValue(0)]
    public required int StateIndex { get; set; }

    /// <summary>
    /// Whether the quest has been started.
    /// </summary>
    [DefaultValue(false)]
    public required bool IsStarted { get; set; }

    /// <summary>
    /// Whether the quest has been completed.
    /// </summary>
    [DefaultValue(false)]
    public required bool IsCompleted { get; set; }

    /// <summary>
    /// Navigation property to the player.
    /// </summary>
    public Player? Player { get; set; }

    /// <summary>
    /// Navigation property to quest flags.
    /// </summary>
    public ICollection<QuestFlag> Flags { get; set; } = new List<QuestFlag>();

    /// <summary>
    /// Navigation property to quest timers.
    /// </summary>
    public ICollection<QuestTimer> Timers { get; set; } = new List<QuestTimer>();

    public static void Configure(EntityTypeBuilder<QuestData> builder, DatabaseFacade database)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.PlayerId, x.QuestName }).IsUnique();

        builder.Property(x => x.QuestName).HasMaxLength(256);
        builder.Property(x => x.StateIndex).HasDefaultValue(0);
        builder.Property(x => x.IsStarted).HasDefaultValue(false);
        builder.Property(x => x.IsCompleted).HasDefaultValue(false);

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

        builder.HasOne(x => x.Player)
            .WithMany()
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
