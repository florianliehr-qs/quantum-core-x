namespace QuantumCore.API.Core.Models;

/// <summary>
/// Represents the runtime state of a quest for a player.
/// This is loaded from and saved to the database.
/// </summary>
public class QuestState
{
    /// <summary>
    /// The database ID of this quest state (null if not yet persisted).
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// The quest name/identifier.
    /// </summary>
    public string QuestName { get; set; } = "";

    /// <summary>
    /// Current state index within the quest (for multi-state quests).
    /// </summary>
    public int StateIndex { get; set; }

    /// <summary>
    /// Whether the quest has been started.
    /// </summary>
    public bool IsStarted { get; set; }

    /// <summary>
    /// Whether the quest has been completed.
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Quest flags (key-value pairs for tracking progress).
    /// </summary>
    public Dictionary<string, int> Flags { get; set; } = new();

    /// <summary>
    /// Active quest timers (name -> trigger time UTC).
    /// </summary>
    public Dictionary<string, DateTime> Timers { get; set; } = new();

    /// <summary>
    /// Whether this state has been modified since loading.
    /// </summary>
    public bool IsDirty { get; set; }

    /// <summary>
    /// The quest title displayed in the quest log.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// The icon file path for the quest UI.
    /// </summary>
    public string? IconPath { get; set; }

    /// <summary>
    /// Counter name displayed in quest UI.
    /// </summary>
    public string? CounterName { get; set; }

    /// <summary>
    /// Counter value displayed in quest UI.
    /// </summary>
    public int CounterValue { get; set; }

    /// <summary>
    /// Clock name displayed in quest UI.
    /// </summary>
    public string? ClockName { get; set; }

    /// <summary>
    /// Clock value (remaining seconds) displayed in quest UI.
    /// </summary>
    public int ClockValue { get; set; }

    /// <summary>
    /// Gets a flag value, returning default value if not found.
    /// </summary>
    public int GetFlag(string name, int defaultValue = 0)
    {
        return Flags.TryGetValue(name, out var value) ? value : defaultValue;
    }

    /// <summary>
    /// Sets a flag value and marks the state as dirty.
    /// </summary>
    public void SetFlag(string name, int value)
    {
        Flags[name] = value;
        IsDirty = true;
    }

    /// <summary>
    /// Increments a flag value by the specified amount.
    /// </summary>
    public int IncrementFlag(string name, int amount = 1)
    {
        var newValue = GetFlag(name) + amount;
        SetFlag(name, newValue);
        return newValue;
    }

    /// <summary>
    /// Checks if a flag exists.
    /// </summary>
    public bool HasFlag(string name)
    {
        return Flags.ContainsKey(name);
    }

    /// <summary>
    /// Clears a flag.
    /// </summary>
    public void ClearFlag(string name)
    {
        if (Flags.Remove(name))
        {
            IsDirty = true;
        }
    }

    /// <summary>
    /// Sets a timer to trigger at the specified time.
    /// </summary>
    public void SetTimer(string name, DateTime triggerAt)
    {
        Timers[name] = triggerAt;
        IsDirty = true;
    }

    /// <summary>
    /// Sets a timer to trigger after the specified duration.
    /// </summary>
    public void SetTimer(string name, TimeSpan duration)
    {
        SetTimer(name, DateTime.UtcNow + duration);
    }

    /// <summary>
    /// Clears a timer.
    /// </summary>
    public void ClearTimer(string name)
    {
        if (Timers.Remove(name))
        {
            IsDirty = true;
        }
    }

    /// <summary>
    /// Gets the remaining time for a timer (or null if not found/expired).
    /// </summary>
    public TimeSpan? GetTimerRemaining(string name)
    {
        if (!Timers.TryGetValue(name, out var triggerAt))
        {
            return null;
        }

        var remaining = triggerAt - DateTime.UtcNow;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    /// <summary>
    /// Checks if a timer has expired.
    /// </summary>
    public bool IsTimerExpired(string name)
    {
        if (!Timers.TryGetValue(name, out var triggerAt))
        {
            return true;
        }

        return DateTime.UtcNow >= triggerAt;
    }

    /// <summary>
    /// Starts the quest.
    /// </summary>
    public void Start()
    {
        if (!IsStarted)
        {
            IsStarted = true;
            IsDirty = true;
        }
    }

    /// <summary>
    /// Completes the quest.
    /// </summary>
    public void Complete()
    {
        if (!IsCompleted)
        {
            IsCompleted = true;
            IsDirty = true;
        }
    }

    /// <summary>
    /// Resets the quest state (for repeatable quests).
    /// </summary>
    public void Reset()
    {
        StateIndex = 0;
        IsStarted = false;
        IsCompleted = false;
        Flags.Clear();
        Timers.Clear();
        Title = null;
        IconPath = null;
        CounterName = null;
        CounterValue = 0;
        ClockName = null;
        ClockValue = 0;
        IsDirty = true;
    }
}
