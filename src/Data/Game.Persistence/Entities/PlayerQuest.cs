using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QuantumCore.Game.Persistence.Entities;

/// <summary>
/// Represents a quest in progress or completed by a player.
/// Stores quest state and progress information.
/// </summary>
public class PlayerQuest
{
    /// <summary>
    /// Unique identifier for this player quest instance
    /// </summary>
    [Key]
    public required Guid Id { get; set; }

    /// <summary>
    /// The player this quest belongs to
    /// </summary>
    public required uint PlayerId { get; set; }
    public Player Player { get; set; } = null!;

    /// <summary>
    /// Unique identifier of the quest definition (matches quest JSON file id)
    /// </summary>
    [StringLength(255)]
    public required string QuestId { get; set; }

    /// <summary>
    /// Current state in the quest state machine
    /// </summary>
    [StringLength(100)]
    public required string CurrentState { get; set; }

    /// <summary>
    /// When the quest was started
    /// </summary>
    public required DateTime StartedAt { get; set; }

    /// <summary>
    /// When the quest was completed (null if not completed)
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Whether the quest is completed
    /// </summary>
    public required bool IsCompleted { get; set; }

    /// <summary>
    /// JSON serialized quest state data (flags, counters, etc.)
    /// Stored as JSON for flexibility - each quest can have different flags
    /// </summary>
    public required string QuestDataJson { get; set; }

    /// <summary>
    /// Configures the entity for Entity Framework Core
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    /// <param name="database">The database facade for database-specific configuration</param>
    public static void Configure(EntityTypeBuilder<PlayerQuest> builder, DatabaseFacade database)
    {
        // Primary key
        builder.HasKey(x => x.Id);

        // Unique index on PlayerId + QuestId (player can only have one instance of each quest)
        builder.HasIndex(x => new { x.PlayerId, x.QuestId })
            .IsUnique()
            .HasDatabaseName("IX_PlayerQuests_PlayerId_QuestId");

        // Index on PlayerId for efficient queries
        builder.HasIndex(x => x.PlayerId)
            .HasDatabaseName("IX_PlayerQuests_PlayerId");

        // Index on QuestId for global quest queries
        builder.HasIndex(x => x.QuestId)
            .HasDatabaseName("IX_PlayerQuests_QuestId");

        // Index on IsCompleted for filtering active/completed quests
        builder.HasIndex(x => x.IsCompleted)
            .HasDatabaseName("IX_PlayerQuests_IsCompleted");

        // Foreign key relationship to Player
        builder.HasOne(x => x.Player)
            .WithMany()
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Cascade); // Delete quests when player is deleted

        // Default values for database columns (database-specific)
        if (database.IsSqlite() || database.IsNpgsql())
        {
            builder.Property(x => x.StartedAt).HasDefaultValueSql("current_timestamp");
            builder.Property(x => x.IsCompleted).HasDefaultValue(false);
            builder.Property(x => x.QuestDataJson).HasDefaultValue("{}");
            builder.Property(x => x.CurrentState).HasDefaultValue("start");
        }
        else if (database.IsMySql())
        {
            builder.Property(x => x.StartedAt)
                .HasDefaultValueSql("(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))");
            builder.Property(x => x.IsCompleted).HasDefaultValue(false);
            builder.Property(x => x.QuestDataJson).HasDefaultValue("{}");
            builder.Property(x => x.CurrentState).HasDefaultValue("start");
        }

        // Column types
        builder.Property(x => x.QuestDataJson).HasColumnType("text");
    }
}
