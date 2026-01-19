# Quest System Architecture

## Overview

The Quantum Core X Quest System is a **hybrid declarative and code-based quest framework** that supports both JSON-defined quests (for simplicity) and C# plugin quests (for complexity). This document covers the technical architecture and implementation details.

## Design Philosophy

### Why Not Lua?

Original Metin2 used Lua for quest scripting. We evaluated and rejected Lua for QCX because:

1. **Performance Trade-offs**: NLua/MoonSharp have significant overhead
   - NLua: Fast pure Lua (40ms) but slow C# callbacks (20,000ms with callbacks)
   - MoonSharp: Slower pure Lua (400ms) but better C# interop (600ms with callbacks)
   - Quests need LOTS of C# callbacks (items, NPCs, conditions)

2. **No Infrastructure**: Zero Lua libraries or bridge layer in codebase

3. **Development Experience**: Losing type safety, IntelliSense, compile-time errors, DI integration

4. **No Legacy Constraints**: This is a clean rewrite, not bound by original Metin2's choices

### Chosen Approach: JSON + C# Hybrid

**90% of quests** → JSON declarative format (simple dialogs, fetch quests, kill quests)
**10% of quests** → C# plugins (complex logic, algorithms, external APIs)

Benefits:
- ✅ Quest designer accessibility (non-programmers can write quests)
- ✅ Hot-reloading (v2 feature)
- ✅ Type safety where it matters (C#)
- ✅ No bridge layer complexity
- ✅ Better performance for C#-heavy operations
- ✅ Fits existing architecture patterns

## Architecture Layers

```
┌────────────────────────────────────────────┐
│           Quest Definition Layer            │
│  ┌──────────────┬──────────────────────┐   │
│  │ JSON Files   │  C# Plugin Quests    │   │
│  │ (data/quests)│  ([Quest] attribute) │   │
│  └──────────────┴──────────────────────┘   │
└────────────────────────────────────────────┘
                    ↓
┌────────────────────────────────────────────┐
│            Quest Loading Layer              │
│  ┌──────────────────────────────────────┐  │
│  │  DeclarativeQuestProvider (ILoadable)│  │
│  │  - Scans data/quests/*.json          │  │
│  │  - Validates quest definitions       │  │
│  │  - Stores in ImmutableDictionary     │  │
│  └──────────────────────────────────────┘  │
└────────────────────────────────────────────┘
                    ↓
┌────────────────────────────────────────────┐
│          Quest Management Layer             │
│  ┌──────────────────────────────────────┐  │
│  │  QuestManager (Singleton)            │  │
│  │  - Registers C# & JSON quests        │  │
│  │  - Initializes player quests         │  │
│  │  - Manages quest lifecycle           │  │
│  └──────────────────────────────────────┘  │
└────────────────────────────────────────────┘
                    ↓
┌────────────────────────────────────────────┐
│          Quest Runtime Layer                │
│  ┌──────────────┬──────────────────────┐   │
│  │ Declarative  │  C# Plugin Quest     │   │
│  │ Quest        │  (extends Quest)     │   │
│  │ (extends     │                      │   │
│  │  Quest)      │                      │   │
│  └──────────────┴──────────────────────┘   │
│  - Trigger registration                    │
│  - Action execution                        │
│  - Condition evaluation                    │
│  - State management                        │
└────────────────────────────────────────────┘
                    ↓
┌────────────────────────────────────────────┐
│         Quest Persistence Layer             │
│  ┌──────────────────────────────────────┐  │
│  │  IDbQuestRepository                  │  │
│  │  - Save/load quest state             │  │
│  │  - Database: PlayerQuests table      │  │
│  │  - JSON serialization of flags       │  │
│  └──────────────────────────────────────┘  │
└────────────────────────────────────────────┘
                    ↓
┌────────────────────────────────────────────┐
│            Game Event Layer                 │
│  ┌──────────────────────────────────────┐  │
│  │  GameEventManager (Static)           │  │
│  │  - NPC click events                  │  │
│  │  - NPC give events                   │  │
│  │  - Entity lifecycle events           │  │
│  └──────────────────────────────────────┘  │
└────────────────────────────────────────────┘
```

## Core Components

### 1. Quest Base Class

All quests (C# and declarative) extend the base `Quest` class:

**File:** `Libraries/Game.Server/Quest/Quest.cs`

```csharp
public abstract class Quest : IQuest
{
    protected Quest(QuestState state, IPlayerEntity player)
    {
        State = state;
        Player = player;
    }

    public QuestState State { get; }
    public IPlayerEntity Player { get; }

    // Abstract method - implemented by subclasses
    public abstract void Init();

    // Quest API Methods
    protected void Text(string str) { /* ... */ }
    protected void Next() { /* ... */ }
    protected async Task<byte> Choice(bool done, params string[] options) { /* ... */ }
    protected void Done(bool silent = false) { /* ... */ }
    protected void SetSkin(QuestSkin skin) { /* ... */ }

    // Internal packet handling
    protected void SendScript() { /* ... */ }
    public void Answer(byte answer) { /* ... */ }
}
```

**Quest Script Protocol:**
- `[ENTER]` - Line break
- `[NEXT]` - Wait for player to click next
- `[QUESTION 1;opt1|2;opt2]` - Show choices
- `[DONE]` - Close dialog

### 2. Quest State Model

**File:** `CorePluginAPI/Core/Models/QuestState.cs`

```csharp
public class QuestState
{
    // Identification
    public Guid PlayerId { get; set; }
    public string QuestId { get; set; } = "";

    // State machine
    public string CurrentState { get; set; } = "start";

    // Lifecycle
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsCompleted { get; set; }

    // Quest flags (flexible key-value storage)
    public Dictionary<string, int> IntFlags { get; set; } = new();
    public Dictionary<string, string> StringFlags { get; set; } = new();
    public Dictionary<string, bool> BoolFlags { get; set; } = new();

    // Helper methods
    public int GetIntFlag(string key, int defaultValue = 0)
        => IntFlags.TryGetValue(key, out var value) ? value : defaultValue;

    public void SetIntFlag(string key, int value)
        => IntFlags[key] = value;

    public void IncIntFlag(string key, int amount = 1)
        => IntFlags[key] = GetIntFlag(key) + amount;
}
```

**Design Decisions:**
- **Flexible flags** instead of fixed columns - quests define their own variables
- **Dictionary-based** for easy serialization to JSON
- **Typed helpers** for common operations (int flags for counters)

### 3. Database Schema

**Entity:** `Data/Game.Persistence/Entities/PlayerQuest.cs`

```csharp
public class PlayerQuest
{
    [Key]
    public Guid Id { get; set; }

    public uint PlayerId { get; set; }
    public Player Player { get; set; } = null!;

    public string QuestId { get; set; } = "";
    public string CurrentState { get; set; } = "start";

    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsCompleted { get; set; }

    // JSON blob for quest-specific data
    public string QuestDataJson { get; set; } = "{}";

    // Entity configuration
    public static void Configure(EntityTypeBuilder<PlayerQuest> builder, DatabaseFacade database)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.PlayerId, x.QuestId }).IsUnique();
        builder.HasOne(x => x.Player)
            .WithMany()
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

**Why JSON blob?**
- Quest flags are rarely queried (no need for normalized columns)
- Flexible schema - each quest defines its own flags
- Easy to add new quest variables without migrations
- Efficient storage for variable-length data

### 4. Quest Repository

**Interface:** `Data/Game.Persistence/IDbQuestRepository.cs`

```csharp
public interface IDbQuestRepository
{
    Task<QuestState?> GetQuestStateAsync(uint playerId, string questId);
    Task SaveQuestStateAsync(uint playerId, QuestState state);
    Task<List<QuestState>> GetPlayerQuestsAsync(uint playerId);
}
```

**Implementation:** Handles JSON serialization/deserialization of quest flags:

```csharp
public async Task SaveQuestStateAsync(uint playerId, QuestState state)
{
    var playerQuest = await _db.PlayerQuests
        .FirstOrDefaultAsync(pq => pq.PlayerId == playerId && pq.QuestId == state.QuestId);

    var questData = new QuestStateData
    {
        IntFlags = state.IntFlags,
        StringFlags = state.StringFlags,
        BoolFlags = state.BoolFlags
    };

    var jsonData = JsonSerializer.Serialize(questData);

    if (playerQuest == null)
    {
        // Insert new quest
        playerQuest = new PlayerQuest { /* ... */ };
        _db.PlayerQuests.Add(playerQuest);
    }
    else
    {
        // Update existing
        playerQuest.CurrentState = state.CurrentState;
        playerQuest.QuestDataJson = jsonData;
    }

    await _db.SaveChangesAsync();
}
```

## Declarative Quest System

### JSON Quest Format

**File:** `data/quests/example_quest.json`

```json
{
  "id": "example_quest",
  "name": "Example Quest",
  "version": "1.0.0",
  "description": "A simple example quest",

  "states": {
    "start": {
      "triggers": [
        {
          "type": "npc_click",
          "npc_id": 20354,
          "condition": {
            "type": "and",
            "conditions": [
              { "type": "quest_not_started" },
              { "type": "level_min", "value": 5 }
            ]
          },
          "actions": [
            {
              "type": "dialog",
              "pages": [
                {
                  "text": "Hello! I need your help.",
                  "next": true
                },
                {
                  "text": "Will you help me?",
                  "choices": [
                    { "text": "Yes!", "next_state": "accepted" },
                    { "text": "No.", "action": "close" }
                  ]
                }
              ]
            }
          ]
        }
      ]
    },

    "accepted": {
      "on_enter": [
        { "type": "set_quest_flag", "flag": "accepted", "value": 1 },
        { "type": "give_item", "item_id": 11001, "count": 1 }
      ],
      "triggers": [
        {
          "type": "npc_click",
          "npc_id": 20354,
          "actions": [
            { "type": "dialog", "pages": [{ "text": "Thank you!", "next": true }] },
            { "type": "give_exp", "amount": 1000 },
            { "type": "complete_quest" }
          ]
        }
      ]
    }
  }
}
```

### Quest Definition Models

**File:** `Libraries/Game.Server/Quest/Models/QuestDefinition.cs`

```csharp
public class QuestDefinition
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Version { get; set; } = "1.0.0";
    public string Description { get; set; } = "";
    public Dictionary<string, StateDefinition> States { get; set; } = new();
}

public class StateDefinition
{
    public List<ActionDefinition> OnEnter { get; set; } = new();
    public List<ActionDefinition> OnExit { get; set; } = new();
    public List<TriggerDefinition> Triggers { get; set; } = new();
}

public class TriggerDefinition
{
    public string Type { get; set; } = ""; // npc_click, kill, item_acquired
    public uint NpcId { get; set; }
    public uint MonsterId { get; set; }
    public uint ItemId { get; set; }
    public ConditionDefinition? Condition { get; set; }
    public List<ActionDefinition> Actions { get; set; } = new();
}

public class ActionDefinition
{
    public string Type { get; set; } = "";
    public JsonElement Data { get; set; }
}

public class ConditionDefinition
{
    public string Type { get; set; } = "";
    public JsonElement Data { get; set; }
}
```

### Declarative Quest Provider

**File:** `Libraries/Game.Server/Quest/DeclarativeQuestProvider.cs`

Implements `ILoadable` for automatic discovery and parallel loading:

```csharp
public class DeclarativeQuestProvider : ILoadable
{
    private readonly IFileProvider _fileProvider;
    private readonly ILogger<DeclarativeQuestProvider> _logger;

    public ImmutableDictionary<string, QuestDefinition> Quests { get; private set; }
        = ImmutableDictionary<string, QuestDefinition>.Empty;

    public async Task LoadAsync(CancellationToken token = default)
    {
        var questsDir = _fileProvider.GetDirectoryContents("quests");

        if (!questsDir.Exists)
        {
            _logger.LogWarning("Quests directory does not exist");
            return;
        }

        var questDict = new Dictionary<string, QuestDefinition>();

        foreach (var file in questsDir.Where(f => f.Name.EndsWith(".json")))
        {
            try
            {
                await using var stream = file.CreateReadStream();
                var quest = await JsonSerializer.DeserializeAsync<QuestDefinition>(
                    stream,
                    new JsonSerializerOptions
                    {
                        ReadCommentHandling = JsonCommentHandling.Skip,
                        AllowTrailingCommas = true,
                        PropertyNameCaseInsensitive = true
                    },
                    cancellationToken: token);

                if (quest == null) continue;

                // Validate quest
                var errors = ValidateQuest(quest);
                if (errors.Any())
                {
                    _logger.LogError("Quest {Id} has errors: {Errors}",
                        quest.Id, string.Join(", ", errors));
                    continue;
                }

                questDict[quest.Id] = quest;
                _logger.LogInformation("Loaded quest {Id} from {File}",
                    quest.Id, file.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load quest from {File}", file.Name);
            }
        }

        Quests = questDict.ToImmutableDictionary();
        _logger.LogInformation("Loaded {Count} declarative quests", Quests.Count);
    }

    private List<string> ValidateQuest(QuestDefinition quest)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(quest.Id))
            errors.Add("Quest ID is required");

        if (!quest.States.ContainsKey("start"))
            errors.Add("Quest must have a 'start' state");

        // Validate state transitions
        foreach (var (stateName, state) in quest.States)
        {
            foreach (var trigger in state.Triggers)
            {
                foreach (var action in trigger.Actions)
                {
                    if (action.Type == "set_state" &&
                        action.Data.TryGetProperty("state", out var nextState))
                    {
                        var nextStateName = nextState.GetString();
                        if (!quest.States.ContainsKey(nextStateName!))
                        {
                            errors.Add($"State '{stateName}' references " +
                                     $"non-existent state '{nextStateName}'");
                        }
                    }
                }
            }
        }

        return errors;
    }
}
```

### Declarative Quest Runtime

**File:** `Libraries/Game.Server/Quest/DeclarativeQuest.cs`

Bridges JSON definitions to the Quest base class:

```csharp
public class DeclarativeQuest : Quest
{
    private readonly QuestDefinition _definition;
    private readonly IServiceProvider _services;
    private readonly IDbQuestRepository _questRepository;
    private readonly ILogger<DeclarativeQuest> _logger;

    public DeclarativeQuest(
        QuestState state,
        IPlayerEntity player,
        QuestDefinition definition,
        IServiceProvider services,
        IDbQuestRepository questRepository,
        ILogger<DeclarativeQuest> logger)
        : base(state, player)
    {
        _definition = definition;
        _services = services;
        _questRepository = questRepository;
        _logger = logger;
    }

    public override void Init()
    {
        // Register triggers for current state
        RegisterTriggersForState(State.CurrentState);
    }

    private void RegisterTriggersForState(string stateName)
    {
        if (!_definition.States.TryGetValue(stateName, out var state))
        {
            _logger.LogError("State {State} not found in quest {QuestId}",
                stateName, _definition.Id);
            return;
        }

        foreach (var trigger in state.Triggers)
        {
            RegisterTrigger(trigger);
        }
    }

    private void RegisterTrigger(TriggerDefinition trigger)
    {
        switch (trigger.Type)
        {
            case "npc_click":
                RegisterNpcClickTrigger(trigger);
                break;
            case "npc_give":
                RegisterNpcGiveTrigger(trigger);
                break;
            case "kill":
                RegisterKillTrigger(trigger);
                break;
            case "item_acquired":
                RegisterItemAcquiredTrigger(trigger);
                break;
            // More trigger types...
        }
    }

    private void RegisterNpcClickTrigger(TriggerDefinition trigger)
    {
        var condition = CreateCondition(trigger.Condition);

        GameEventManager.RegisterNpcClickEvent(
            $"{_definition.Id}:{State.CurrentState}",
            trigger.NpcId,
            async (player) =>
            {
                if (player.Vid != Player.Vid) return;

                var context = new QuestConditionContext
                {
                    Player = player,
                    State = State,
                    Services = _services
                };

                if (condition == null || condition.Evaluate(context))
                {
                    await ExecuteActions(trigger.Actions);
                }
            },
            player => player.Vid == Player.Vid
        );
    }

    private async Task ExecuteActions(List<ActionDefinition> actionDefs)
    {
        foreach (var actionDef in actionDefs)
        {
            var action = CreateAction(actionDef);
            var context = new QuestActionContext
            {
                Player = Player,
                State = State,
                Quest = this,
                Services = _services
            };

            await action.ExecuteAsync(context);
        }
    }

    private IQuestAction CreateAction(ActionDefinition def)
    {
        // Factory pattern - deserialize JSON to concrete action
        return def.Type switch
        {
            "dialog" => JsonSerializer.Deserialize<DialogAction>(def.Data.GetRawText())!,
            "give_item" => JsonSerializer.Deserialize<GiveItemAction>(def.Data.GetRawText())!,
            // ... more actions
            _ => throw new NotSupportedException($"Action type {def.Type} not supported")
        };
    }

    private IQuestCondition? CreateCondition(ConditionDefinition? def)
    {
        if (def == null) return null;

        return def.Type switch
        {
            "and" => JsonSerializer.Deserialize<AndCondition>(def.Data.GetRawText())!,
            "level_min" => JsonSerializer.Deserialize<LevelMinCondition>(def.Data.GetRawText())!,
            // ... more conditions
            _ => throw new NotSupportedException($"Condition type {def.Type} not supported")
        };
    }
}
```

## Action System

### Action Interface

**File:** `Libraries/Game.Server/Quest/Actions/IQuestAction.cs`

```csharp
public interface IQuestAction
{
    Task ExecuteAsync(QuestActionContext context);
}

public class QuestActionContext
{
    public IPlayerEntity Player { get; init; }
    public QuestState State { get; init; }
    public DeclarativeQuest Quest { get; init; }
    public IServiceProvider Services { get; init; }

    public T GetService<T>() where T : notnull
        => Services.GetRequiredService<T>();
}
```

### Example Actions

#### DialogAction

```csharp
public class DialogAction : IQuestAction
{
    public List<DialogPage> Pages { get; set; } = new();

    public async Task ExecuteAsync(QuestActionContext context)
    {
        var quest = context.Quest;

        foreach (var page in Pages)
        {
            quest.Text(page.Text);

            if (page.Next)
            {
                quest.Next();
                await Task.CompletedTask; // Wait for next button
            }
            else if (page.Choices?.Count > 0)
            {
                var choice = await quest.Choice(false,
                    page.Choices.Select(c => c.Text).ToArray());

                var selectedChoice = page.Choices[choice - 1];

                if (selectedChoice.NextState != null)
                {
                    context.State.CurrentState = selectedChoice.NextState;
                    await context.GetService<IDbQuestRepository>()
                        .SaveQuestStateAsync(context.Player.Player.Id, context.State);
                }
            }
        }
    }
}

public class DialogPage
{
    public string Text { get; set; } = "";
    public bool Next { get; set; }
    public List<DialogChoice>? Choices { get; set; }
}

public class DialogChoice
{
    public string Text { get; set; } = "";
    public string? NextState { get; set; }
    public string? Action { get; set; }
}
```

#### GiveItemAction

```csharp
public class GiveItemAction : IQuestAction
{
    public uint ItemId { get; set; }
    public int Count { get; set; } = 1;

    public async Task ExecuteAsync(QuestActionContext context)
    {
        var itemManager = context.GetService<IItemManager>();
        var proto = itemManager.GetItem(ItemId);

        if (proto == null)
        {
            throw new InvalidOperationException($"Item {ItemId} not found");
        }

        for (int i = 0; i < Count; i++)
        {
            var item = itemManager.CreateItem(proto);
            context.Player.Inventory.PlaceItem(item);
        }

        await Task.CompletedTask;
    }
}
```

### Planned Actions (15 total)

1. ✅ **DialogAction** - Show dialog pages
2. ✅ **GiveItemAction** - Add item to inventory
3. **RemoveItemAction** - Remove item from inventory
4. **GiveExpAction** - Award experience
5. **GiveGoldAction** - Award currency
6. **SetQuestFlagAction** - Set flag value
7. **IncQuestFlagAction** - Increment flag
8. **SetStateAction** - Change quest state
9. **CompleteQuestAction** - Mark quest complete
10. **SendLetterAction** - Send quest letter
11. **ClearLetterAction** - Clear quest letter
12. **WarpAction** - Teleport player
13. **SpawnMonsterAction** - Spawn monster
14. **ConditionAction** - Conditional execution
15. **DelayAction** - Wait for duration

## Condition System

### Condition Interface

**File:** `Libraries/Game.Server/Quest/Conditions/IQuestCondition.cs`

```csharp
public interface IQuestCondition
{
    bool Evaluate(QuestConditionContext context);
}

public class QuestConditionContext
{
    public IPlayerEntity Player { get; init; }
    public QuestState State { get; init; }
    public IServiceProvider Services { get; init; }

    public T GetService<T>() where T : notnull
        => Services.GetRequiredService<T>();
}
```

### Example Conditions

#### AndCondition

```csharp
public class AndCondition : IQuestCondition
{
    public List<IQuestCondition> Conditions { get; set; } = new();

    public bool Evaluate(QuestConditionContext context)
    {
        return Conditions.All(c => c.Evaluate(context));
    }
}
```

#### LevelMinCondition

```csharp
public class LevelMinCondition : IQuestCondition
{
    public int Value { get; set; }

    public bool Evaluate(QuestConditionContext context)
    {
        return context.Player.Level >= Value;
    }
}
```

#### HasItemCondition

```csharp
public class HasItemCondition : IQuestCondition
{
    public uint ItemId { get; set; }
    public int Count { get; set; } = 1;

    public bool Evaluate(QuestConditionContext context)
    {
        return context.Player.Inventory.GetItemCount(ItemId) >= Count;
    }
}
```

### Planned Conditions (15 total)

1. ✅ **AndCondition** - All conditions true
2. ✅ **OrCondition** - Any condition true
3. ✅ **NotCondition** - Invert condition
4. ✅ **LevelMinCondition** - Min level check
5. ✅ **LevelMaxCondition** - Max level check
6. ✅ **HasItemCondition** - Inventory check
7. **QuestFlagGteCondition** - Flag >= value
8. **QuestFlagEqCondition** - Flag == value
9. **QuestNotStartedCondition** - Quest not started
10. **QuestCompletedCondition** - Quest completed
11. **ClassCheckCondition** - Player class check
12. **GuildCheckCondition** - Guild membership
13. **GoldCheckCondition** - Currency check
14. **TimeCheckCondition** - Time range check
15. **LevelRangeCondition** - Level in range

## Trigger System

### Supported Triggers

1. **npc_click** - Player clicks NPC
2. **npc_give** - Player gives item to NPC
3. **kill** - Player kills monster
4. **item_acquired** - Player obtains item
5. **login** - Player logs in
6. **levelup** - Player levels up
7. **chat** - Player sends chat message

### Trigger Registration

Triggers are registered via `GameEventManager`:

```csharp
// NPC Click
GameEventManager.RegisterNpcClickEvent(
    eventName: "Quest:State",
    npcId: 20354,
    callback: async (player) => { /* handler */ },
    condition: player => player.Level >= 5
);

// NPC Give Item
GameEventManager.RegisterNpcGiveEvent(
    eventName: "Quest:State",
    npcId: 20354,
    callback: (player, item) => { /* handler */ },
    condition: (player, item) => item.Id == 50001
);
```

## Quest Manager Integration

**File:** `Libraries/Game.Server/Quest/QuestManager.cs`

```csharp
public class QuestManager : IQuestManager, ILoadable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly DeclarativeQuestProvider _declarativeQuestProvider;
    private readonly Dictionary<string, Type> _csharpQuests = new();

    public async Task LoadAsync(CancellationToken token = default)
    {
        // 1. Load C# quests via reflection
        var assembly = Assembly.GetAssembly(typeof(QuestManager));
        foreach (var questType in assembly.GetTypes()
            .Where(type => type.GetCustomAttribute<QuestAttribute>() is not null))
        {
            RegisterQuest(questType);
        }

        // 2. Load declarative quests from JSON
        await _declarativeQuestProvider.LoadAsync(token);
    }

    public void InitializePlayer(IPlayerEntity player)
    {
        if (player is not PlayerEntity p) return;

        var questRepository = _serviceProvider.GetRequiredService<IDbQuestRepository>();

        // Initialize C# quests
        foreach (var (id, questType) in _csharpQuests)
        {
            var state = questRepository.GetQuestStateAsync(p.Player.Id, id).Result
                ?? new QuestState { QuestId = id };

            var quest = (Quest)ActivatorUtilities.CreateInstance(
                _serviceProvider, questType, state, player);
            quest.Init();
            p.Quests[id] = quest;
        }

        // Initialize declarative quests
        foreach (var (id, definition) in _declarativeQuestProvider.Quests)
        {
            var state = questRepository.GetQuestStateAsync(p.Player.Id, id).Result
                ?? new QuestState { QuestId = id };

            var quest = new DeclarativeQuest(
                state, player, definition, _serviceProvider,
                questRepository, _logger);
            quest.Init();
            p.Quests[id] = quest;
        }
    }
}
```

## Data Flow Diagrams

### Quest Loading Flow

```
Server Startup
    │
    ├──→ QuestManager.LoadAsync()
    │       │
    │       ├──→ Scan for [Quest] attributes (C# quests)
    │       │       └──→ Register in _csharpQuests
    │       │
    │       └──→ DeclarativeQuestProvider.LoadAsync()
    │               │
    │               ├──→ Scan data/quests/*.json
    │               ├──→ Parse JSON → QuestDefinition
    │               ├──→ Validate quest structure
    │               └──→ Store in ImmutableDictionary
    │
    └──→ Ready for player connections
```

### Player Quest Initialization

```
Player Login
    │
    └──→ QuestManager.InitializePlayer(player)
            │
            ├──→ For each C# quest:
            │       ├──→ Load QuestState from DB
            │       ├──→ Instantiate quest (DI)
            │       ├──→ Call Quest.Init()
            │       └──→ Store in player.Quests
            │
            └──→ For each declarative quest:
                    ├──→ Load QuestState from DB
                    ├──→ Create DeclarativeQuest
                    ├──→ Call Quest.Init()
                    │       └──→ Register triggers
                    └──→ Store in player.Quests
```

### Quest Execution Flow

```
Player Clicks NPC
    │
    └──→ GameEventManager.OnNpcClick(npcId, player)
            │
            ├──→ Find matching NpcClickEvents
            │       └──→ Filter by condition
            │
            └──→ Execute callback
                    │
                    └──→ DeclarativeQuest.ExecuteActions()
                            │
                            ├──→ For each ActionDefinition:
                            │       ├──→ CreateAction()
                            │       └──→ action.ExecuteAsync(context)
                            │
                            ├──→ If state changed:
                            │       ├──→ Call OnExit (old state)
                            │       ├──→ Update State.CurrentState
                            │       ├──→ Call OnEnter (new state)
                            │       ├──→ Re-register triggers
                            │       └──→ SaveQuestStateAsync()
                            │
                            └──→ Complete
```

## Performance Considerations

### Memory Usage
- **Quest Definitions**: Loaded once at startup, stored in ImmutableDictionary
- **Quest Instances**: One per player per quest (lightweight, mostly state)
- **Quest State**: Minimal footprint (flags dictionary + metadata)

### Database Load
- **Lazy Loading**: Quest state loaded only when needed
- **Write-through**: State saved after each significant change
- **Batch Operations**: Multiple flags updated in single save

### Scalability
- **Stateless**: Quest definitions are immutable and shared
- **Per-Player**: Each player has independent quest instances
- **Redis Ready**: Quest state can be cached in Redis for distributed servers

## Testing Strategy

### Unit Tests
- QuestState flag operations
- Action execution (mocked context)
- Condition evaluation (mocked context)
- JSON deserialization (factories)
- Quest validation logic

### Integration Tests
- Database persistence (TestContainers)
- Quest loading from files
- End-to-end quest flow
- State transitions
- Multiple players with same quest

### Performance Tests
- Load 100+ quests at startup
- 100 concurrent players with active quests
- Quest state save/load benchmarks

## Security Considerations

### Input Validation
- Quest JSON validated against schema
- NPC IDs, item IDs validated against game data
- State transitions validated at load time

### SQL Injection
- EF Core parameterized queries
- No raw SQL for quest operations

### Resource Limits
- Max quest file size limit
- Max quest states per player
- Timeout for quest action execution

## Future Enhancements (v2)

### Hot-Reload
- FileSystemWatcher for quest file changes
- Unregister/re-register triggers
- Preserve active quest state during reload

### Quest Editor GUI
- Visual quest designer
- Drag-and-drop state machine
- Real-time validation

### Advanced Features
- Quest prerequisites (chains)
- Time-based quests (daily/weekly)
- Randomized rewards
- Party quests
- Achievement integration
- Localization support

## References

- Metin2 Quest System: [Metin2Hub Quest Documentation](https://metin2hub.com/forums/quests-lua.18/)
- Design Document: `/Users/florian.liehr/.claude/plans/swift-popping-cocoa.md`
- Implementation Plan: See Quest Designer Guide and Developer Guide
