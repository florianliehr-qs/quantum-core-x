using Microsoft.EntityFrameworkCore;
using QuantumCore.API.Core.Models;
using QuantumCore.Game.Persistence.Entities;

namespace QuantumCore.Game.Persistence;

/// <summary>
/// Repository implementation for quest state persistence.
/// </summary>
public class DbQuestRepository : IDbQuestRepository
{
    private readonly GameDbContext _db;

    public DbQuestRepository(GameDbContext db)
    {
        _db = db;
    }

    public async Task<ICollection<QuestState>> GetPlayerQuestsAsync(uint playerId)
    {
        var questData = await _db.QuestData
            .AsNoTracking()
            .Include(q => q.Flags)
            .Include(q => q.Timers)
            .Where(q => q.PlayerId == playerId)
            .ToListAsync();

        return questData.Select(MapToQuestState).ToList();
    }

    public async Task<QuestState?> GetPlayerQuestAsync(uint playerId, string questName)
    {
        var questData = await _db.QuestData
            .AsNoTracking()
            .Include(q => q.Flags)
            .Include(q => q.Timers)
            .FirstOrDefaultAsync(q => q.PlayerId == playerId && q.QuestName == questName);

        return questData is null ? null : MapToQuestState(questData);
    }

    public async Task<ICollection<(uint PlayerId, string QuestName, string TimerName, DateTime TriggerAt)>> GetPendingTimersAsync()
    {
        var now = DateTime.UtcNow;
        var timers = await _db.QuestTimers
            .AsNoTracking()
            .Include(t => t.QuestData)
            .Where(t => !t.IsProcessed && t.TriggerAt <= now)
            .ToListAsync();

        return timers
            .Where(t => t.QuestData != null)
            .Select(t => (t.QuestData!.PlayerId, t.QuestData.QuestName, t.Name, t.TriggerAt))
            .ToList();
    }

    public async Task SaveQuestStateAsync(uint playerId, QuestState state)
    {
        var existingQuest = await _db.QuestData
            .Include(q => q.Flags)
            .Include(q => q.Timers)
            .FirstOrDefaultAsync(q => q.PlayerId == playerId && q.QuestName == state.QuestName);

        if (existingQuest is not null)
        {
            // Update existing quest
            existingQuest.StateIndex = state.StateIndex;
            existingQuest.IsStarted = state.IsStarted;
            existingQuest.IsCompleted = state.IsCompleted;
            existingQuest.UpdatedAt = DateTime.UtcNow;

            // Update flags
            UpdateFlags(existingQuest, state.Flags);

            // Update timers
            UpdateTimers(existingQuest, state.Timers);

            _db.QuestData.Update(existingQuest);
        }
        else
        {
            // Create new quest
            var questData = new QuestData
            {
                Id = Guid.NewGuid(),
                PlayerId = playerId,
                QuestName = state.QuestName,
                StateIndex = state.StateIndex,
                IsStarted = state.IsStarted,
                IsCompleted = state.IsCompleted,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Add flags
            foreach (var (name, value) in state.Flags)
            {
                questData.Flags.Add(new QuestFlag
                {
                    Id = Guid.NewGuid(),
                    QuestDataId = questData.Id,
                    Name = name,
                    Value = value,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            // Add timers
            foreach (var (name, triggerAt) in state.Timers)
            {
                questData.Timers.Add(new QuestTimer
                {
                    Id = Guid.NewGuid(),
                    QuestDataId = questData.Id,
                    Name = name,
                    TriggerAt = triggerAt,
                    IsProcessed = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            _db.QuestData.Add(questData);
            state.Id = questData.Id;
        }

        await _db.SaveChangesAsync();
        state.IsDirty = false;
    }

    public async Task DeleteQuestStateAsync(uint playerId, string questName)
    {
        var questData = await _db.QuestData
            .FirstOrDefaultAsync(q => q.PlayerId == playerId && q.QuestName == questName);

        if (questData is not null)
        {
            _db.QuestData.Remove(questData);
            await _db.SaveChangesAsync();
        }
    }

    public async Task MarkTimerProcessedAsync(uint playerId, string questName, string timerName)
    {
        var timer = await _db.QuestTimers
            .Include(t => t.QuestData)
            .FirstOrDefaultAsync(t =>
                t.QuestData != null &&
                t.QuestData.PlayerId == playerId &&
                t.QuestData.QuestName == questName &&
                t.Name == timerName);

        if (timer is not null)
        {
            timer.IsProcessed = true;
            timer.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

    public async Task SaveAllDirtyQuestsAsync(uint playerId, IEnumerable<QuestState> states)
    {
        foreach (var state in states.Where(s => s.IsDirty))
        {
            await SaveQuestStateAsync(playerId, state);
        }
    }

    private void UpdateFlags(QuestData questData, Dictionary<string, int> flags)
    {
        // Remove flags that no longer exist
        var flagsToRemove = questData.Flags
            .Where(f => !flags.ContainsKey(f.Name))
            .ToList();
        foreach (var flag in flagsToRemove)
        {
            _db.QuestFlags.Remove(flag);
        }

        // Update or add flags
        foreach (var (name, value) in flags)
        {
            var existingFlag = questData.Flags.FirstOrDefault(f => f.Name == name);
            if (existingFlag is not null)
            {
                existingFlag.Value = value;
                existingFlag.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                questData.Flags.Add(new QuestFlag
                {
                    Id = Guid.NewGuid(),
                    QuestDataId = questData.Id,
                    Name = name,
                    Value = value,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }
    }

    private void UpdateTimers(QuestData questData, Dictionary<string, DateTime> timers)
    {
        // Remove timers that no longer exist
        var timersToRemove = questData.Timers
            .Where(t => !timers.ContainsKey(t.Name))
            .ToList();
        foreach (var timer in timersToRemove)
        {
            _db.QuestTimers.Remove(timer);
        }

        // Update or add timers
        foreach (var (name, triggerAt) in timers)
        {
            var existingTimer = questData.Timers.FirstOrDefault(t => t.Name == name);
            if (existingTimer is not null)
            {
                existingTimer.TriggerAt = triggerAt;
                existingTimer.IsProcessed = false;
                existingTimer.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                questData.Timers.Add(new QuestTimer
                {
                    Id = Guid.NewGuid(),
                    QuestDataId = questData.Id,
                    Name = name,
                    TriggerAt = triggerAt,
                    IsProcessed = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }
    }

    private static QuestState MapToQuestState(QuestData data)
    {
        var state = new QuestState
        {
            Id = data.Id,
            QuestName = data.QuestName,
            StateIndex = data.StateIndex,
            IsStarted = data.IsStarted,
            IsCompleted = data.IsCompleted,
            IsDirty = false
        };

        foreach (var flag in data.Flags)
        {
            state.Flags[flag.Name] = flag.Value;
        }

        foreach (var timer in data.Timers.Where(t => !t.IsProcessed))
        {
            state.Timers[timer.Name] = timer.TriggerAt;
        }

        return state;
    }
}
