using System.Diagnostics;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types.Entities;
using QuantumCore.API.Game.Types.Quest;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Packets.Quest;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Quest;

/// <summary>
/// Base class for all quests. Provides dialog, state management, and reward APIs.
/// </summary>
public abstract class Quest : IQuest
{
    public QuestState State { get; }
    public IPlayerEntity Player { get; }

    private readonly PlayerEntity _player;
    protected readonly IItemManager ItemManager;
    private string _questScript = "";
    private QuestSkin _currentSkin = QuestSkin.NORMAL;

    // Task completion sources for async dialog flow
    private TaskCompletionSource? _currentNextTask;
    private TaskCompletionSource<byte>? _currentChoiceTask;
    private TaskCompletionSource<string>? _currentInputTask;
    private TaskCompletionSource<bool>? _currentConfirmTask;
    private TaskCompletionSource<uint>? _currentSelectItemTask;

    // Quest button tracking
    private ushort _questIndex;

    public Quest(QuestState state, IPlayerEntity player, IItemManager itemManager)
    {
        State = state;
        Player = player;
        _player = (PlayerEntity)player;
        ItemManager = itemManager;
    }

    public abstract void Init();

    #region Dialog Flow - Answer Methods

    public void Answer(byte answer)
    {
        // Answer 254 = Next button clicked
        if (answer == 254)
        {
            _currentNextTask?.TrySetResult();
            return;
        }

        _currentChoiceTask?.TrySetResult(answer);
    }

    public void AnswerInput(string input)
    {
        _currentInputTask?.TrySetResult(input);
    }

    public void AnswerConfirm(bool confirmed)
    {
        _currentConfirmTask?.TrySetResult(confirmed);
    }

    public void OnButton(uint index)
    {
        // Client sends quest button index with high bit set (0x80000000)
        // Mask it off to get the actual quest index
        const uint QUEST_BUTTON_MASK = 0x80000000;
        var actualIndex = index & ~QUEST_BUTTON_MASK;

        // Check if this is our quest's button
        if (actualIndex == _questIndex)
        {
            // Fire-and-forget to avoid blocking
            _ = Task.Run(async () =>
            {
                try
                {
                    await OnQuestButtonClick();
                }
                catch
                {
                    // Silently ignore errors in quest button click
                }
            });
        }
    }

    /// <summary>
    /// Called when the player clicks on this quest's button in the mission tab.
    /// Override to show quest progress or information.
    /// </summary>
    protected virtual Task OnQuestButtonClick()
    {
        // Default implementation shows basic quest info
        if (IsStarted && !IsCompleted)
        {
            Text($"Quest: {State.Title ?? "Unknown"}");
            if (!string.IsNullOrEmpty(State.CounterName))
            {
                Text($"{State.CounterName}: {State.CounterValue}");
            }
            Text("");
            Text("Continue your quest objectives.");
            Done();
        }
        else if (IsCompleted)
        {
            Text("This quest has been completed.");
            Done();
        }
        return Task.CompletedTask;
    }

    public void OnSelectItem(uint itemId)
    {
        _currentSelectItemTask?.TrySetResult(itemId);
    }

    public virtual Task OnTimer(string timerName)
    {
        // Override in derived classes to handle timer events
        return Task.CompletedTask;
    }

    #endregion

    #region Script Building

    protected void SendScript()
    {
        _player.Connection.Send(new QuestScript
        {
            Skin = (byte)_currentSkin,
            Source = _questScript
        });

        _currentSkin = QuestSkin.NORMAL;
        _questScript = "";
    }

    protected void SetSkin(QuestSkin skin)
    {
        _currentSkin = skin;
    }

    protected void Text(string str)
    {
        _questScript += str + "[ENTER]";
    }

    protected void TextLine(string str)
    {
        _questScript += str;
    }

    protected void Clear()
    {
        _questScript += "[CLEAR]";
    }

    protected void Delay(int milliseconds)
    {
        _questScript += $"[DELAY {milliseconds}]";
    }

    #endregion

    #region Dialog UI Controls

    protected void SetWindowSize(int width, int height)
    {
        _questScript += $"[WINDOW_SIZE {width};{height}]";
    }

    protected void SetColor(float r, float g, float b)
    {
        _questScript += $"[COLOR {r};{g};{b}]";
    }

    protected void SetColor256(int r, int g, int b)
    {
        _questScript += $"[COLOR256 {r};{g};{b}]";
    }

    protected void LeftImage(string path)
    {
        _questScript += $"[LEFTIMAGE {path}]";
    }

    protected void TopImage(string path)
    {
        _questScript += $"[TOPIMAGE {path}]";
    }

    protected void BgImage(string path)
    {
        _questScript += $"[BGIMAGE {path}]";
    }

    protected void Image(int x, int y, string path)
    {
        _questScript += $"[IMAGE {x};{y};{path}]";
    }

    protected void Letter(string text)
    {
        _questScript += $"[LETTER {text}]";
    }

    protected void InsertItemName(uint vnum)
    {
        _questScript += $"[ITEM {vnum}]";
    }

    protected void InsertMobName(uint vnum)
    {
        _questScript += $"[MOB {vnum}]";
    }

    #endregion

    #region Map/Camera Controls

    protected void AddMapSignal(int x, int y)
    {
        _questScript += $"[ADD_MAP_SIGNAL {x};{y}]";
    }

    protected void ClearMapSignal()
    {
        _questScript += "[CLEAR_MAP_SIGNAL]";
    }

    protected void SetCamera(int x, int y, int z)
    {
        _questScript += $"[SET_CAMERA {x};{y};{z}]";
    }

    protected void BlendCamera(int x, int y, int z, int duration)
    {
        _questScript += $"[BLEND_CAMERA {x};{y};{z};{duration}]";
    }

    protected void RestoreCamera()
    {
        _questScript += "[RESTORE_CAMERA]";
    }

    #endregion

    #region Dialog Interaction Methods

    protected void Next()
    {
        _currentNextTask?.TrySetCanceled();
        _currentNextTask = new TaskCompletionSource();

        _questScript += "[NEXT]";
        SendScript();

        _player.CurrentQuest = this;
    }

    protected async Task WaitNext()
    {
        _currentNextTask?.TrySetCanceled();
        _currentNextTask = new TaskCompletionSource();

        _questScript += "[NEXT]";
        SendScript();

        _player.CurrentQuest = this;
        await _currentNextTask.Task;
    }

    protected async Task<byte> Choice(bool done = false, params string[] options)
    {
        Debug.Assert(options.Length > 0);

        _currentChoiceTask?.TrySetCanceled();
        _currentChoiceTask = new TaskCompletionSource<byte>();

        _questScript += "[QUESTION ";

        for (var i = 0; i < options.Length; i++)
        {
            if (i != 0)
            {
                _questScript += "|";
            }

            Debug.Assert(!options[i].Contains(';'));
            Debug.Assert(!options[i].Contains('|'));

            _questScript += $"{i + 1};" + options[i];
        }

        _questScript += "]";

        if (done)
        {
            _questScript += "[DONE]";
        }

        SendScript();

        _player.CurrentQuest = this;
        return await _currentChoiceTask.Task;
    }

    protected async Task<string> Input()
    {
        _currentInputTask?.TrySetCanceled();
        _currentInputTask = new TaskCompletionSource<string>();

        _questScript += "[INPUT]";
        SendScript();

        _player.CurrentQuest = this;
        return await _currentInputTask.Task;
    }

    protected async Task<bool> Confirm(string message, int timeoutSeconds = 10)
    {
        _currentConfirmTask?.TrySetCanceled();
        _currentConfirmTask = new TaskCompletionSource<bool>();

        _player.Connection.Send(new QuestConfirm
        {
            Message = message,
            Timeout = timeoutSeconds,
            RequestPID = Player.Player.Id
        });

        _player.CurrentQuest = this;
        return await _currentConfirmTask.Task;
    }

    protected async Task<uint> SelectItem()
    {
        _currentSelectItemTask?.TrySetCanceled();
        _currentSelectItemTask = new TaskCompletionSource<uint>();

        _questScript += "[SELECT_ITEM]";
        SendScript();

        _player.CurrentQuest = this;
        return await _currentSelectItemTask.Task;
    }

    protected void Done(bool silent = false)
    {
        if (!silent)
        {
            _questScript += "[ENTER]";
        }

        _questScript += "[DONE]";

        SendScript();
    }

    #endregion

    #region Quest Button UI

    protected void ShowQuestButton(string name, string iconType = "file", string iconName = "")
    {
        _questScript += $"[QUESTBUTTON {_questIndex};{name};{iconType};{iconName}]";
    }

    protected void HideQuestButton()
    {
        _questScript += $"[QUESTBUTTON_CLOSE {_questIndex}]";
    }

    #endregion

    #region Quest State Management

    public void SetQuestIndex(ushort index)
    {
        _questIndex = index;
    }

    public void SendQuestInfo()
    {
        byte flags = 0;

        // Always send IS_BEGIN flag if quest has been started
        // IsBegin=true shows the quest, IsBegin=false removes it from the list
        if (State.IsStarted)
        {
            flags |= QuestInfoFlags.QUEST_SEND_IS_BEGIN;
        }

        if (!string.IsNullOrEmpty(State.Title))
        {
            flags |= QuestInfoFlags.QUEST_SEND_TITLE;
        }

        if (!string.IsNullOrEmpty(State.ClockName))
        {
            flags |= QuestInfoFlags.QUEST_SEND_CLOCK_NAME;
            flags |= QuestInfoFlags.QUEST_SEND_CLOCK_VALUE;
        }

        if (!string.IsNullOrEmpty(State.CounterName))
        {
            flags |= QuestInfoFlags.QUEST_SEND_COUNTER_NAME;
            flags |= QuestInfoFlags.QUEST_SEND_COUNTER_VALUE;
        }

        if (!string.IsNullOrEmpty(State.IconPath))
        {
            flags |= QuestInfoFlags.QUEST_SEND_ICON_FILE;
        }

        _player.Connection.Send(new QuestInfo
        {
            Index = _questIndex,
            Flag = flags,
            IsBegin = State.IsStarted && !State.IsCompleted,
            Title = State.Title ?? "",
            ClockName = State.ClockName ?? "",
            ClockValue = State.ClockValue,
            CounterName = State.CounterName ?? "",
            CounterValue = State.CounterValue,
            IconFile = State.IconPath ?? ""
        });
    }

    public void MarkDirty()
    {
        State.IsDirty = true;
    }

    #endregion

    #region Flag Management

    protected int GetFlag(string name, int defaultValue = 0)
    {
        return State.GetFlag(name, defaultValue);
    }

    protected void SetFlag(string name, int value)
    {
        State.SetFlag(name, value);
    }

    protected int IncrementFlag(string name, int amount = 1)
    {
        return State.IncrementFlag(name, amount);
    }

    protected bool HasFlag(string name)
    {
        return State.HasFlag(name);
    }

    protected void ClearFlag(string name)
    {
        State.ClearFlag(name);
    }

    #endregion

    #region Timer Management

    protected void SetTimer(string name, TimeSpan duration)
    {
        State.SetTimer(name, duration);
    }

    protected void SetTimer(string name, int seconds)
    {
        State.SetTimer(name, TimeSpan.FromSeconds(seconds));
    }

    protected void ClearTimer(string name)
    {
        State.ClearTimer(name);
    }

    protected TimeSpan? GetTimerRemaining(string name)
    {
        return State.GetTimerRemaining(name);
    }

    protected bool IsTimerExpired(string name)
    {
        return State.IsTimerExpired(name);
    }

    #endregion

    #region Quest UI State

    protected void SetTitle(string title)
    {
        State.Title = title;
        State.IsDirty = true;
    }

    protected void SetIcon(string iconPath)
    {
        State.IconPath = iconPath;
        State.IsDirty = true;
    }

    protected void SetCounter(string name, int value)
    {
        State.CounterName = name;
        State.CounterValue = value;
        State.IsDirty = true;
    }

    protected void SetClock(string name, int seconds)
    {
        State.ClockName = name;
        State.ClockValue = seconds;
        State.IsDirty = true;
    }

    protected void ClearCounter()
    {
        State.CounterName = null;
        State.CounterValue = 0;
        State.IsDirty = true;
    }

    protected void ClearClock()
    {
        State.ClockName = null;
        State.ClockValue = 0;
        State.IsDirty = true;
    }

    #endregion

    #region Quest Progress

    protected void StartQuest()
    {
        State.Start();
        SendQuestInfo();
    }

    protected void CompleteQuest()
    {
        State.Complete();
        ClearCounter();
        ClearClock();
        SendQuestInfo();
    }

    protected void ResetQuest()
    {
        State.Reset();
        SendQuestInfo();
    }

    protected bool IsStarted => State.IsStarted;

    protected bool IsCompleted => State.IsCompleted;

    protected int CurrentState
    {
        get => State.StateIndex;
        set
        {
            State.StateIndex = value;
            State.IsDirty = true;
        }
    }

    #endregion

    #region Rewards

    protected void GiveExp(int amount)
    {
        _player.AddPoint(EPoint.EXPERIENCE, amount);
        _player.SendPoints();
    }

    protected void GiveGold(int amount)
    {
        _player.AddPoint(EPoint.GOLD, amount);
        _player.SendPoints();
    }

    protected bool TakeGold(int amount)
    {
        var currentGold = _player.GetPoint(EPoint.GOLD);
        if (currentGold < amount)
        {
            return false;
        }

        _player.AddPoint(EPoint.GOLD, -amount);
        _player.SendPoints();
        return true;
    }

    protected bool GiveItem(uint itemId, byte count = 1)
    {
        var proto = ItemManager.GetItem(itemId);
        if (proto is null)
        {
            return false;
        }

        var item = ItemManager.CreateItem(proto, count);
        var result = _player.Inventory.PlaceItem(item).Result;

        if (result)
        {
            _player.SendItem(item);
        }

        return result;
    }

    protected bool HasItem(uint itemId, int count = 1)
    {
        var totalCount = 0;
        foreach (var item in _player.Inventory.Items)
        {
            if (item.ItemId == itemId)
            {
                totalCount += item.Count;
                if (totalCount >= count)
                {
                    return true;
                }
            }
        }

        return false;
    }

    protected bool TakeItem(uint itemId, int count = 1)
    {
        var remaining = count;

        foreach (var item in _player.Inventory.Items.ToList())
        {
            if (item.ItemId != itemId)
            {
                continue;
            }

            if (item.Count <= remaining)
            {
                remaining -= item.Count;
                _player.DestroyItem(item);
            }
            else
            {
                item.Count -= (byte)remaining;
                _player.SendItem(item);
                remaining = 0;
            }

            if (remaining <= 0)
            {
                return true;
            }
        }

        return remaining <= 0;
    }

    protected int CountItem(uint itemId)
    {
        var count = 0;
        foreach (var item in _player.Inventory.Items)
        {
            if (item.ItemId == itemId)
            {
                count += item.Count;
            }
        }

        return count;
    }

    #endregion

    #region Player Information

    protected int PlayerLevel => Player.Player.Level;

    protected uint PlayerGold => Player.Player.Gold;

    protected string PlayerName => Player.Name;

    protected byte PlayerClass => (byte)Player.Player.PlayerClass;

    protected byte PlayerEmpire => (byte)Player.Player.Empire;

    #endregion
}

/// <summary>
/// Quest info packet flags.
/// </summary>
public static class QuestInfoFlags
{
    public const byte QUEST_SEND_IS_BEGIN = 0x01;
    public const byte QUEST_SEND_TITLE = 0x02;
    public const byte QUEST_SEND_CLOCK_NAME = 0x04;
    public const byte QUEST_SEND_CLOCK_VALUE = 0x08;
    public const byte QUEST_SEND_COUNTER_NAME = 0x10;
    public const byte QUEST_SEND_COUNTER_VALUE = 0x20;
    public const byte QUEST_SEND_ICON_FILE = 0x40;
}
