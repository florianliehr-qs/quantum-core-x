using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;

namespace QuantumCore.API;

/// <summary>
/// Quest event types corresponding to the original Lua event system.
/// </summary>
public enum QuestEventType
{
    NpcClick,
    Kill,
    Timer,
    LevelUp,
    Login,
    Logout,
    Button,
    Info,
    Chat,
    AttrIn,
    AttrOut,
    ItemUse,
    ServerTimer,
    EnterState,
    LeaveState,
    Letter,
    ItemTake,
    Target,
    PartyKill,
    Unmount,
    ItemPick,
    SigUse,
    ItemInformer,
    NpcGive
}

/// <summary>
/// Manages quest events and their handlers.
/// </summary>
public interface IQuestEventManager
{
    // NPC Click events
    void RegisterNpcClickEvent(string name, uint npcId, Func<IPlayerEntity, Task> callback,
        Func<IPlayerEntity, bool>? condition = null);

    Task OnNpcClick(uint npcId, IPlayerEntity player);

    // NPC Give events (player gives item to NPC)
    void RegisterNpcGiveEvent(string name, uint npcId, Func<IPlayerEntity, ItemInstance, Task> callback,
        Func<IPlayerEntity, ItemInstance, bool>? condition = null);

    Task OnNpcGive(uint npcId, IPlayerEntity player, ItemInstance item);

    // Kill events (monster killed by player)
    void RegisterKillEvent(string name, uint mobVnum, Func<IPlayerEntity, uint, Task> callback,
        Func<IPlayerEntity, uint, bool>? condition = null);

    Task OnKill(uint mobVnum, IPlayerEntity player);

    // Level up events
    void RegisterLevelUpEvent(string name, Func<IPlayerEntity, int, Task> callback,
        Func<IPlayerEntity, int, bool>? condition = null);

    Task OnLevelUp(IPlayerEntity player, int newLevel);

    // Login events
    void RegisterLoginEvent(string name, Func<IPlayerEntity, Task> callback,
        Func<IPlayerEntity, bool>? condition = null);

    Task OnLogin(IPlayerEntity player);

    // Logout events
    void RegisterLogoutEvent(string name, Func<IPlayerEntity, Task> callback,
        Func<IPlayerEntity, bool>? condition = null);

    Task OnLogout(IPlayerEntity player);

    // Item use events
    void RegisterItemUseEvent(string name, uint itemVnum, Func<IPlayerEntity, ItemInstance, Task> callback,
        Func<IPlayerEntity, ItemInstance, bool>? condition = null);

    Task<bool> OnItemUse(uint itemVnum, IPlayerEntity player, ItemInstance item);

    // Item pick events (player picks up item from ground)
    void RegisterItemPickEvent(string name, uint itemVnum, Func<IPlayerEntity, ItemInstance, Task> callback,
        Func<IPlayerEntity, ItemInstance, bool>? condition = null);

    Task OnItemPick(uint itemVnum, IPlayerEntity player, ItemInstance item);

    // Target events (player targets entity)
    void RegisterTargetEvent(string name, Func<IPlayerEntity, IEntity, Task> callback,
        Func<IPlayerEntity, IEntity, bool>? condition = null);

    Task OnTarget(IPlayerEntity player, IEntity target);

    // Area enter/leave events
    void RegisterAreaEnterEvent(string name, string areaName, Func<IPlayerEntity, Task> callback,
        Func<IPlayerEntity, bool>? condition = null);

    void RegisterAreaLeaveEvent(string name, string areaName, Func<IPlayerEntity, Task> callback,
        Func<IPlayerEntity, bool>? condition = null);

    Task OnAreaEnter(string areaName, IPlayerEntity player);

    Task OnAreaLeave(string areaName, IPlayerEntity player);

    // Timer events (quest-specific timers)
    void RegisterTimerEvent(string questName, string timerName, Func<IPlayerEntity, Task> callback);

    Task OnTimer(IPlayerEntity player, string questName, string timerName);

    // Server timer events (global timers)
    void RegisterServerTimerEvent(string timerName, Func<Task> callback);

    Task OnServerTimer(string timerName);

    // Chat events
    void RegisterChatEvent(string name, string pattern, Func<IPlayerEntity, string, Task> callback,
        Func<IPlayerEntity, string, bool>? condition = null);

    Task<bool> OnChat(IPlayerEntity player, string message);

    // Unregister events
    void UnregisterNpcClickEvent(string name, uint npcId);
    void UnregisterKillEvent(string name, uint mobVnum);
    void UnregisterAllPlayerEvents(IPlayerEntity player);

    // Clear all events (for server shutdown/restart)
    void Clear();
}
