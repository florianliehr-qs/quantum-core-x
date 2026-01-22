namespace QuantumCore.API.Core.Models;

/// <summary>
/// Represents the state of a quest for a specific player.
/// Tracks quest progress, flags, and lifecycle information.
/// </summary>
public class QuestState
{
    /// <summary>
    /// The player this quest state belongs to
    /// </summary>
    public Guid PlayerId { get; set; }

    /// <summary>
    /// Unique identifier for the quest
    /// </summary>
    public string QuestId { get; set; } = "";

    /// <summary>
    /// Current state in the quest state machine
    /// </summary>
    public string CurrentState { get; set; } = "start";

    /// <summary>
    /// When the quest was started
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// When the quest was completed (null if not completed)
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Whether the quest is completed
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Integer flags for quest state (counters, numeric values)
    /// </summary>
    public Dictionary<string, int> IntFlags { get; set; } = new();

    /// <summary>
    /// String flags for quest state (text values, descriptions)
    /// </summary>
    public Dictionary<string, string> StringFlags { get; set; } = new();

    /// <summary>
    /// Boolean flags for quest state (true/false conditions)
    /// </summary>
    public Dictionary<string, bool> BoolFlags { get; set; } = new();

    /// <summary>
    /// Gets an integer flag value, returning a default value if not found
    /// </summary>
    /// <param name="key">The flag name</param>
    /// <param name="defaultValue">Value to return if flag doesn't exist (default: 0)</param>
    /// <returns>The flag value or default value</returns>
    public int GetIntFlag(string key, int defaultValue = 0)
        => IntFlags.TryGetValue(key, out var value) ? value : defaultValue;

    /// <summary>
    /// Sets an integer flag value
    /// </summary>
    /// <param name="key">The flag name</param>
    /// <param name="value">The value to set</param>
    public void SetIntFlag(string key, int value)
        => IntFlags[key] = value;

    /// <summary>
    /// Increments an integer flag by the specified amount
    /// </summary>
    /// <param name="key">The flag name</param>
    /// <param name="amount">Amount to increment (default: 1)</param>
    public void IncIntFlag(string key, int amount = 1)
        => IntFlags[key] = GetIntFlag(key) + amount;

    /// <summary>
    /// Decrements an integer flag by the specified amount
    /// </summary>
    /// <param name="key">The flag name</param>
    /// <param name="amount">Amount to decrement (default: 1)</param>
    public void DecIntFlag(string key, int amount = 1)
        => IntFlags[key] = GetIntFlag(key) - amount;

    /// <summary>
    /// Gets a string flag value, returning a default value if not found
    /// </summary>
    /// <param name="key">The flag name</param>
    /// <param name="defaultValue">Value to return if flag doesn't exist (default: empty string)</param>
    /// <returns>The flag value or default value</returns>
    public string GetStringFlag(string key, string defaultValue = "")
        => StringFlags.TryGetValue(key, out var value) ? value : defaultValue;

    /// <summary>
    /// Sets a string flag value
    /// </summary>
    /// <param name="key">The flag name</param>
    /// <param name="value">The value to set</param>
    public void SetStringFlag(string key, string value)
        => StringFlags[key] = value;

    /// <summary>
    /// Gets a boolean flag value, returning a default value if not found
    /// </summary>
    /// <param name="key">The flag name</param>
    /// <param name="defaultValue">Value to return if flag doesn't exist (default: false)</param>
    /// <returns>The flag value or default value</returns>
    public bool GetBoolFlag(string key, bool defaultValue = false)
        => BoolFlags.TryGetValue(key, out var value) ? value : defaultValue;

    /// <summary>
    /// Sets a boolean flag value
    /// </summary>
    /// <param name="key">The flag name</param>
    /// <param name="value">The value to set</param>
    public void SetBoolFlag(string key, bool value)
        => BoolFlags[key] = value;

    /// <summary>
    /// Checks if an integer flag exists
    /// </summary>
    /// <param name="key">The flag name</param>
    /// <returns>True if the flag exists</returns>
    public bool HasIntFlag(string key)
        => IntFlags.ContainsKey(key);

    /// <summary>
    /// Checks if a string flag exists
    /// </summary>
    /// <param name="key">The flag name</param>
    /// <returns>True if the flag exists</returns>
    public bool HasStringFlag(string key)
        => StringFlags.ContainsKey(key);

    /// <summary>
    /// Checks if a boolean flag exists
    /// </summary>
    /// <param name="key">The flag name</param>
    /// <returns>True if the flag exists</returns>
    public bool HasBoolFlag(string key)
        => BoolFlags.ContainsKey(key);

    /// <summary>
    /// Removes an integer flag
    /// </summary>
    /// <param name="key">The flag name</param>
    /// <returns>True if the flag was removed</returns>
    public bool RemoveIntFlag(string key)
        => IntFlags.Remove(key);

    /// <summary>
    /// Removes a string flag
    /// </summary>
    /// <param name="key">The flag name</param>
    /// <returns>True if the flag was removed</returns>
    public bool RemoveStringFlag(string key)
        => StringFlags.Remove(key);

    /// <summary>
    /// Removes a boolean flag
    /// </summary>
    /// <param name="key">The flag name</param>
    /// <returns>True if the flag was removed</returns>
    public bool RemoveBoolFlag(string key)
        => BoolFlags.Remove(key);

    /// <summary>
    /// Clears all quest flags
    /// </summary>
    public void ClearAllFlags()
    {
        IntFlags.Clear();
        StringFlags.Clear();
        BoolFlags.Clear();
    }
}