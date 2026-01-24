using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QuantumCore.Game.Persistence.Entities;

/// <summary>
/// Represents a single quest flag (key-value pair) for persistent quest state.
/// Flags are used to track quest progress, counters, and other state.
/// </summary>
public class QuestFlag
{
    public required Guid Id { get; set; } = Guid.NewGuid();
    public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public required DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The quest data this flag belongs to.
    /// </summary>
    public required Guid QuestDataId { get; set; }

    /// <summary>
    /// The flag name (e.g., "kill_count", "spoke_to_npc").
    /// </summary>
    public required string Name { get; set; } = "";

    /// <summary>
    /// The flag value stored as integer.
    /// </summary>
    [DefaultValue(0)]
    public required int Value { get; set; }

    /// <summary>
    /// Navigation property to the quest data.
    /// </summary>
    public QuestData? QuestData { get; set; }

    public static void Configure(EntityTypeBuilder<QuestFlag> builder, DatabaseFacade database)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.QuestDataId, x.Name }).IsUnique();

        builder.Property(x => x.Name).HasMaxLength(128);
        builder.Property(x => x.Value).HasDefaultValue(0);

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
            .WithMany(x => x.Flags)
            .HasForeignKey(x => x.QuestDataId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
