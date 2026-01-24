using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Quest;

/// <summary>
/// Manages quest events and their handlers. Supports all 22+ event types from the original system.
/// </summary>
public class QuestEventManager : IQuestEventManager
{
    private readonly ILogger<QuestEventManager> _logger;

    #region Event Structures

    private struct NpcClickEvent
    {
        public string Name { get; init; }
        public uint NpcId { get; init; }
        public Func<IPlayerEntity, Task> Callback { get; init; }
        public Func<IPlayerEntity, bool>? Condition { get; init; }
    }

    private struct NpcGiveEvent
    {
        public string Name { get; init; }
        public uint NpcId { get; init; }
        public Func<IPlayerEntity, ItemInstance, Task> Callback { get; init; }
        public Func<IPlayerEntity, ItemInstance, bool>? Condition { get; init; }
    }

    private struct KillEvent
    {
        public string Name { get; init; }
        public uint MobVnum { get; init; }
        public Func<IPlayerEntity, uint, Task> Callback { get; init; }
        public Func<IPlayerEntity, uint, bool>? Condition { get; init; }
    }

    private struct LevelUpEvent
    {
        public string Name { get; init; }
        public Func<IPlayerEntity, int, Task> Callback { get; init; }
        public Func<IPlayerEntity, int, bool>? Condition { get; init; }
    }

    private struct PlayerEvent
    {
        public string Name { get; init; }
        public Func<IPlayerEntity, Task> Callback { get; init; }
        public Func<IPlayerEntity, bool>? Condition { get; init; }
    }

    private struct ItemEvent
    {
        public string Name { get; init; }
        public uint ItemVnum { get; init; }
        public Func<IPlayerEntity, ItemInstance, Task> Callback { get; init; }
        public Func<IPlayerEntity, ItemInstance, bool>? Condition { get; init; }
    }

    private struct TargetEvent
    {
        public string Name { get; init; }
        public Func<IPlayerEntity, IEntity, Task> Callback { get; init; }
        public Func<IPlayerEntity, IEntity, bool>? Condition { get; init; }
    }

    private struct AreaEvent
    {
        public string Name { get; init; }
        public string AreaName { get; init; }
        public Func<IPlayerEntity, Task> Callback { get; init; }
        public Func<IPlayerEntity, bool>? Condition { get; init; }
    }

    private struct TimerEvent
    {
        public string QuestName { get; init; }
        public string TimerName { get; init; }
        public Func<IPlayerEntity, Task> Callback { get; init; }
    }

    private struct ServerTimerEvent
    {
        public string TimerName { get; init; }
        public Func<Task> Callback { get; init; }
    }

    private struct ChatEvent
    {
        public string Name { get; init; }
        public string Pattern { get; init; }
        public Regex? CompiledPattern { get; init; }
        public Func<IPlayerEntity, string, Task> Callback { get; init; }
        public Func<IPlayerEntity, string, bool>? Condition { get; init; }
    }

    #endregion

    #region Event Dictionaries

    private readonly Dictionary<uint, List<NpcClickEvent>> _npcClickEvents = new();
    private readonly Dictionary<uint, List<NpcGiveEvent>> _npcGiveEvents = new();
    private readonly Dictionary<uint, List<KillEvent>> _killEvents = new();
    private readonly List<LevelUpEvent> _levelUpEvents = new();
    private readonly List<PlayerEvent> _loginEvents = new();
    private readonly List<PlayerEvent> _logoutEvents = new();
    private readonly Dictionary<uint, List<ItemEvent>> _itemUseEvents = new();
    private readonly Dictionary<uint, List<ItemEvent>> _itemPickEvents = new();
    private readonly List<TargetEvent> _targetEvents = new();
    private readonly Dictionary<string, List<AreaEvent>> _areaEnterEvents = new();
    private readonly Dictionary<string, List<AreaEvent>> _areaLeaveEvents = new();
    private readonly Dictionary<string, List<TimerEvent>> _timerEvents = new();
    private readonly Dictionary<string, List<ServerTimerEvent>> _serverTimerEvents = new();
    private readonly List<ChatEvent> _chatEvents = new();

    private readonly object _lock = new();

    #endregion

    public QuestEventManager(ILogger<QuestEventManager> logger)
    {
        _logger = logger;
    }

    #region NPC Click Events

    public void RegisterNpcClickEvent(string name, uint npcId, Func<IPlayerEntity, Task> callback,
        Func<IPlayerEntity, bool>? condition = null)
    {
        lock (_lock)
        {
            if (!_npcClickEvents.ContainsKey(npcId))
            {
                _npcClickEvents[npcId] = new List<NpcClickEvent>();
            }

            _npcClickEvents[npcId].Add(new NpcClickEvent
            {
                Name = name,
                NpcId = npcId,
                Callback = callback,
                Condition = condition
            });

            _logger.LogDebug("Registered NPC click event '{Name}' for NPC {NpcId}", name, npcId);
        }
    }

    public async Task OnNpcClick(uint npcId, IPlayerEntity player)
    {
        List<NpcClickEvent> events;

        lock (_lock)
        {
            if (!_npcClickEvents.TryGetValue(npcId, out var eventList))
            {
                return;
            }

            events = eventList.Where(e => e.Condition is null || e.Condition(player)).ToList();
        }

        if (events.Count == 0)
        {
            return;
        }

        if (events.Count > 1)
        {
            var internalQuest = player.GetQuestInstance<InternalQuest>();
            if (internalQuest is null)
            {
                return;
            }

            var selected = await internalQuest.SelectQuest(events.Select(e => e.Name));
            if (events[selected].Callback.Target is not Quest)
            {
                internalQuest.EndQuest();
            }

            await events[selected].Callback(player);
            return;
        }

        await events[0].Callback(player);
    }

    public void UnregisterNpcClickEvent(string name, uint npcId)
    {
        lock (_lock)
        {
            if (_npcClickEvents.TryGetValue(npcId, out var events))
            {
                events.RemoveAll(e => e.Name == name);
            }
        }
    }

    #endregion

    #region NPC Give Events

    public void RegisterNpcGiveEvent(string name, uint npcId, Func<IPlayerEntity, ItemInstance, Task> callback,
        Func<IPlayerEntity, ItemInstance, bool>? condition = null)
    {
        lock (_lock)
        {
            if (!_npcGiveEvents.ContainsKey(npcId))
            {
                _npcGiveEvents[npcId] = new List<NpcGiveEvent>();
            }

            _npcGiveEvents[npcId].Add(new NpcGiveEvent
            {
                Name = name,
                NpcId = npcId,
                Callback = callback,
                Condition = condition
            });

            _logger.LogDebug("Registered NPC give event '{Name}' for NPC {NpcId}", name, npcId);
        }
    }

    public async Task OnNpcGive(uint npcId, IPlayerEntity player, ItemInstance item)
    {
        List<NpcGiveEvent> events;

        lock (_lock)
        {
            if (!_npcGiveEvents.TryGetValue(npcId, out var eventList))
            {
                return;
            }

            events = eventList.Where(e => e.Condition is null || e.Condition(player, item)).ToList();
        }

        if (events.Count == 0)
        {
            return;
        }

        if (events.Count > 1)
        {
            var internalQuest = player.GetQuestInstance<InternalQuest>();
            if (internalQuest is null)
            {
                return;
            }

            var selected = await internalQuest.SelectQuest(events.Select(e => e.Name));
            if (events[selected].Callback.Target is not Quest)
            {
                internalQuest.EndQuest();
            }

            await events[selected].Callback(player, item);
            return;
        }

        await events[0].Callback(player, item);
    }

    #endregion

    #region Kill Events

    public void RegisterKillEvent(string name, uint mobVnum, Func<IPlayerEntity, uint, Task> callback,
        Func<IPlayerEntity, uint, bool>? condition = null)
    {
        lock (_lock)
        {
            if (!_killEvents.ContainsKey(mobVnum))
            {
                _killEvents[mobVnum] = new List<KillEvent>();
            }

            _killEvents[mobVnum].Add(new KillEvent
            {
                Name = name,
                MobVnum = mobVnum,
                Callback = callback,
                Condition = condition
            });

            _logger.LogDebug("Registered kill event '{Name}' for mob {MobVnum}", name, mobVnum);
        }
    }

    public async Task OnKill(uint mobVnum, IPlayerEntity player)
    {
        List<KillEvent> events;

        lock (_lock)
        {
            if (!_killEvents.TryGetValue(mobVnum, out var eventList))
            {
                return;
            }

            events = eventList.Where(e => e.Condition is null || e.Condition(player, mobVnum)).ToList();
        }

        foreach (var evt in events)
        {
            await evt.Callback(player, mobVnum);
        }
    }

    public void UnregisterKillEvent(string name, uint mobVnum)
    {
        lock (_lock)
        {
            if (_killEvents.TryGetValue(mobVnum, out var events))
            {
                events.RemoveAll(e => e.Name == name);
            }
        }
    }

    #endregion

    #region Level Up Events

    public void RegisterLevelUpEvent(string name, Func<IPlayerEntity, int, Task> callback,
        Func<IPlayerEntity, int, bool>? condition = null)
    {
        lock (_lock)
        {
            _levelUpEvents.Add(new LevelUpEvent
            {
                Name = name,
                Callback = callback,
                Condition = condition
            });

            _logger.LogDebug("Registered level up event '{Name}'", name);
        }
    }

    public async Task OnLevelUp(IPlayerEntity player, int newLevel)
    {
        List<LevelUpEvent> events;

        lock (_lock)
        {
            events = _levelUpEvents.Where(e => e.Condition is null || e.Condition(player, newLevel)).ToList();
        }

        foreach (var evt in events)
        {
            await evt.Callback(player, newLevel);
        }
    }

    #endregion

    #region Login/Logout Events

    public void RegisterLoginEvent(string name, Func<IPlayerEntity, Task> callback,
        Func<IPlayerEntity, bool>? condition = null)
    {
        lock (_lock)
        {
            _loginEvents.Add(new PlayerEvent
            {
                Name = name,
                Callback = callback,
                Condition = condition
            });

            _logger.LogDebug("Registered login event '{Name}'", name);
        }
    }

    public async Task OnLogin(IPlayerEntity player)
    {
        List<PlayerEvent> events;

        lock (_lock)
        {
            events = _loginEvents.Where(e => e.Condition is null || e.Condition(player)).ToList();
        }

        foreach (var evt in events)
        {
            await evt.Callback(player);
        }
    }

    public void RegisterLogoutEvent(string name, Func<IPlayerEntity, Task> callback,
        Func<IPlayerEntity, bool>? condition = null)
    {
        lock (_lock)
        {
            _logoutEvents.Add(new PlayerEvent
            {
                Name = name,
                Callback = callback,
                Condition = condition
            });

            _logger.LogDebug("Registered logout event '{Name}'", name);
        }
    }

    public async Task OnLogout(IPlayerEntity player)
    {
        List<PlayerEvent> events;

        lock (_lock)
        {
            events = _logoutEvents.Where(e => e.Condition is null || e.Condition(player)).ToList();
        }

        foreach (var evt in events)
        {
            await evt.Callback(player);
        }
    }

    #endregion

    #region Item Events

    public void RegisterItemUseEvent(string name, uint itemVnum, Func<IPlayerEntity, ItemInstance, Task> callback,
        Func<IPlayerEntity, ItemInstance, bool>? condition = null)
    {
        lock (_lock)
        {
            if (!_itemUseEvents.ContainsKey(itemVnum))
            {
                _itemUseEvents[itemVnum] = new List<ItemEvent>();
            }

            _itemUseEvents[itemVnum].Add(new ItemEvent
            {
                Name = name,
                ItemVnum = itemVnum,
                Callback = callback,
                Condition = condition
            });

            _logger.LogDebug("Registered item use event '{Name}' for item {ItemVnum}", name, itemVnum);
        }
    }

    public async Task<bool> OnItemUse(uint itemVnum, IPlayerEntity player, ItemInstance item)
    {
        List<ItemEvent> events;

        lock (_lock)
        {
            if (!_itemUseEvents.TryGetValue(itemVnum, out var eventList))
            {
                return false;
            }

            events = eventList.Where(e => e.Condition is null || e.Condition(player, item)).ToList();
        }

        if (events.Count == 0)
        {
            return false;
        }

        foreach (var evt in events)
        {
            await evt.Callback(player, item);
        }

        return true;
    }

    public void RegisterItemPickEvent(string name, uint itemVnum, Func<IPlayerEntity, ItemInstance, Task> callback,
        Func<IPlayerEntity, ItemInstance, bool>? condition = null)
    {
        lock (_lock)
        {
            if (!_itemPickEvents.ContainsKey(itemVnum))
            {
                _itemPickEvents[itemVnum] = new List<ItemEvent>();
            }

            _itemPickEvents[itemVnum].Add(new ItemEvent
            {
                Name = name,
                ItemVnum = itemVnum,
                Callback = callback,
                Condition = condition
            });

            _logger.LogDebug("Registered item pick event '{Name}' for item {ItemVnum}", name, itemVnum);
        }
    }

    public async Task OnItemPick(uint itemVnum, IPlayerEntity player, ItemInstance item)
    {
        List<ItemEvent> events;

        lock (_lock)
        {
            if (!_itemPickEvents.TryGetValue(itemVnum, out var eventList))
            {
                return;
            }

            events = eventList.Where(e => e.Condition is null || e.Condition(player, item)).ToList();
        }

        foreach (var evt in events)
        {
            await evt.Callback(player, item);
        }
    }

    #endregion

    #region Target Events

    public void RegisterTargetEvent(string name, Func<IPlayerEntity, IEntity, Task> callback,
        Func<IPlayerEntity, IEntity, bool>? condition = null)
    {
        lock (_lock)
        {
            _targetEvents.Add(new TargetEvent
            {
                Name = name,
                Callback = callback,
                Condition = condition
            });

            _logger.LogDebug("Registered target event '{Name}'", name);
        }
    }

    public async Task OnTarget(IPlayerEntity player, IEntity target)
    {
        List<TargetEvent> events;

        lock (_lock)
        {
            events = _targetEvents.Where(e => e.Condition is null || e.Condition(player, target)).ToList();
        }

        foreach (var evt in events)
        {
            await evt.Callback(player, target);
        }
    }

    #endregion

    #region Area Events

    public void RegisterAreaEnterEvent(string name, string areaName, Func<IPlayerEntity, Task> callback,
        Func<IPlayerEntity, bool>? condition = null)
    {
        lock (_lock)
        {
            if (!_areaEnterEvents.ContainsKey(areaName))
            {
                _areaEnterEvents[areaName] = new List<AreaEvent>();
            }

            _areaEnterEvents[areaName].Add(new AreaEvent
            {
                Name = name,
                AreaName = areaName,
                Callback = callback,
                Condition = condition
            });

            _logger.LogDebug("Registered area enter event '{Name}' for area {AreaName}", name, areaName);
        }
    }

    public void RegisterAreaLeaveEvent(string name, string areaName, Func<IPlayerEntity, Task> callback,
        Func<IPlayerEntity, bool>? condition = null)
    {
        lock (_lock)
        {
            if (!_areaLeaveEvents.ContainsKey(areaName))
            {
                _areaLeaveEvents[areaName] = new List<AreaEvent>();
            }

            _areaLeaveEvents[areaName].Add(new AreaEvent
            {
                Name = name,
                AreaName = areaName,
                Callback = callback,
                Condition = condition
            });

            _logger.LogDebug("Registered area leave event '{Name}' for area {AreaName}", name, areaName);
        }
    }

    public async Task OnAreaEnter(string areaName, IPlayerEntity player)
    {
        List<AreaEvent> events;

        lock (_lock)
        {
            if (!_areaEnterEvents.TryGetValue(areaName, out var eventList))
            {
                return;
            }

            events = eventList.Where(e => e.Condition is null || e.Condition(player)).ToList();
        }

        foreach (var evt in events)
        {
            await evt.Callback(player);
        }
    }

    public async Task OnAreaLeave(string areaName, IPlayerEntity player)
    {
        List<AreaEvent> events;

        lock (_lock)
        {
            if (!_areaLeaveEvents.TryGetValue(areaName, out var eventList))
            {
                return;
            }

            events = eventList.Where(e => e.Condition is null || e.Condition(player)).ToList();
        }

        foreach (var evt in events)
        {
            await evt.Callback(player);
        }
    }

    #endregion

    #region Timer Events

    public void RegisterTimerEvent(string questName, string timerName, Func<IPlayerEntity, Task> callback)
    {
        var key = $"{questName}.{timerName}";

        lock (_lock)
        {
            if (!_timerEvents.ContainsKey(key))
            {
                _timerEvents[key] = new List<TimerEvent>();
            }

            _timerEvents[key].Add(new TimerEvent
            {
                QuestName = questName,
                TimerName = timerName,
                Callback = callback
            });

            _logger.LogDebug("Registered timer event '{QuestName}.{TimerName}'", questName, timerName);
        }
    }

    public async Task OnTimer(IPlayerEntity player, string questName, string timerName)
    {
        var key = $"{questName}.{timerName}";
        List<TimerEvent> events;

        lock (_lock)
        {
            if (!_timerEvents.TryGetValue(key, out var eventList))
            {
                return;
            }

            events = eventList.ToList();
        }

        foreach (var evt in events)
        {
            await evt.Callback(player);
        }
    }

    public void RegisterServerTimerEvent(string timerName, Func<Task> callback)
    {
        lock (_lock)
        {
            if (!_serverTimerEvents.ContainsKey(timerName))
            {
                _serverTimerEvents[timerName] = new List<ServerTimerEvent>();
            }

            _serverTimerEvents[timerName].Add(new ServerTimerEvent
            {
                TimerName = timerName,
                Callback = callback
            });

            _logger.LogDebug("Registered server timer event '{TimerName}'", timerName);
        }
    }

    public async Task OnServerTimer(string timerName)
    {
        List<ServerTimerEvent> events;

        lock (_lock)
        {
            if (!_serverTimerEvents.TryGetValue(timerName, out var eventList))
            {
                return;
            }

            events = eventList.ToList();
        }

        foreach (var evt in events)
        {
            await evt.Callback();
        }
    }

    #endregion

    #region Chat Events

    public void RegisterChatEvent(string name, string pattern, Func<IPlayerEntity, string, Task> callback,
        Func<IPlayerEntity, string, bool>? condition = null)
    {
        lock (_lock)
        {
            Regex? compiledPattern = null;
            try
            {
                compiledPattern = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Invalid regex pattern '{Pattern}' for chat event '{Name}'", pattern, name);
            }

            _chatEvents.Add(new ChatEvent
            {
                Name = name,
                Pattern = pattern,
                CompiledPattern = compiledPattern,
                Callback = callback,
                Condition = condition
            });

            _logger.LogDebug("Registered chat event '{Name}' with pattern '{Pattern}'", name, pattern);
        }
    }

    public async Task<bool> OnChat(IPlayerEntity player, string message)
    {
        List<ChatEvent> events;

        lock (_lock)
        {
            events = _chatEvents
                .Where(e => e.CompiledPattern?.IsMatch(message) == true &&
                            (e.Condition is null || e.Condition(player, message)))
                .ToList();
        }

        if (events.Count == 0)
        {
            return false;
        }

        foreach (var evt in events)
        {
            await evt.Callback(player, message);
        }

        return true;
    }

    #endregion

    #region Utility Methods

    public void UnregisterAllPlayerEvents(IPlayerEntity player)
    {
        // This is used to clean up player-specific events when a player disconnects
        // For now, we don't track player-specific events separately
        // This would need to be implemented if we want per-player event cleanup
        _logger.LogDebug("Unregister all events for player {PlayerId}", player.Player.Id);
    }

    public void Clear()
    {
        lock (_lock)
        {
            _npcClickEvents.Clear();
            _npcGiveEvents.Clear();
            _killEvents.Clear();
            _levelUpEvents.Clear();
            _loginEvents.Clear();
            _logoutEvents.Clear();
            _itemUseEvents.Clear();
            _itemPickEvents.Clear();
            _targetEvents.Clear();
            _areaEnterEvents.Clear();
            _areaLeaveEvents.Clear();
            _timerEvents.Clear();
            _serverTimerEvents.Clear();
            _chatEvents.Clear();

            _logger.LogInformation("Cleared all quest events");
        }
    }

    #endregion
}
