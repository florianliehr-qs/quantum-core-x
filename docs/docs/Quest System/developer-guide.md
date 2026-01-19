# Quest System Developer Guide

**For:** C# developers implementing and extending the quest system
**Level:** Intermediate to Advanced
**Prerequisites:** C# knowledge, familiarity with .NET, Entity Framework Core

## Introduction

This guide covers the technical implementation details for developers working on the quest system. You'll learn how to:

- Implement new quest actions
- Create new quest conditions
- Add new trigger types
- Extend the quest framework
- Write C# plugin quests for complex logic
- Test quest components

## Project Structure

```
src/
├── CorePluginAPI/Core/Models/
│   └── QuestState.cs                    # Quest state model
│
├── Data/Game.Persistence/
│   ├── Entities/
│   │   └── PlayerQuest.cs               # EF Core entity
│   ├── IDbQuestRepository.cs            # Repository interface
│   └── DbQuestRepository.cs             # Repository implementation
│
└── Libraries/Game.Server/Quest/
    ├── Quest.cs                          # Base quest class
    ├── QuestManager.cs                   # Quest lifecycle manager
    ├── QuestAttribute.cs                 # Quest discovery attribute
    │
    ├── DeclarativeQuestProvider.cs       # JSON quest loader
    ├── DeclarativeQuest.cs               # JSON quest runtime
    │
    ├── Actions/
    │   ├── IQuestAction.cs               # Action interface
    │   ├── QuestActionContext.cs         # Action execution context
    │   ├── DialogAction.cs               # Dialog implementation
    │   ├── GiveItemAction.cs             # Give item implementation
    │   └── ...                           # More actions
    │
    ├── Conditions/
    │   ├── IQuestCondition.cs            # Condition interface
    │   ├── QuestConditionContext.cs      # Condition evaluation context
    │   ├── AndCondition.cs               # Logical AND
    │   ├── LevelMinCondition.cs          # Level check
    │   └── ...                           # More conditions
    │
    └── Models/
        ├── QuestDefinition.cs            # JSON quest model
        ├── StateDefinition.cs            # State model
        ├── TriggerDefinition.cs          # Trigger model
        ├── ActionDefinition.cs           # Action model
        └── ConditionDefinition.cs        # Condition model
```

## Implementing a New Quest Action

### Step 1: Create the Action Class

**File:** `Libraries/Game.Server/Quest/Actions/TeleportAction.cs`

```csharp
using QuantumCore.API;
using QuantumCore.Game.Quest.Actions;

namespace QuantumCore.Game.Quest.Actions;

/// <summary>
/// Teleports the player to a specific location
/// </summary>
public class TeleportAction : IQuestAction
{
    /// <summary>
    /// Map ID to teleport to
    /// </summary>
    public uint MapId { get; set; }

    /// <summary>
    /// X coordinate
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Y coordinate
    /// </summary>
    public int Y { get; set; }

    public async Task ExecuteAsync(QuestActionContext context)
    {
        var player = context.Player;

        // Validate map exists
        var world = context.GetService<IWorld>();
        var map = world.GetMap(MapId);

        if (map == null)
        {
            throw new InvalidOperationException($"Map {MapId} not found");
        }

        // Validate coordinates
        if (!map.IsValidPosition(X, Y))
        {
            throw new InvalidOperationException(
                $"Invalid position ({X}, {Y}) on map {MapId}");
        }

        // Perform teleport
        await player.WarpAsync(MapId, X, Y);
    }
}
```

### Step 2: Register the Action in Factory

**File:** `Libraries/Game.Server/Quest/QuestActionFactory.cs`

```csharp
public class QuestActionFactory
{
    private readonly IServiceProvider _services;

    public QuestActionFactory(IServiceProvider services)
    {
        _services = services;
    }

    public IQuestAction CreateAction(ActionDefinition definition)
    {
        return definition.Type switch
        {
            "dialog" => DeserializeAction<DialogAction>(definition),
            "give_item" => DeserializeAction<GiveItemAction>(definition),
            "teleport" => DeserializeAction<TeleportAction>(definition),  // Add here!
            // ... other actions
            _ => throw new NotSupportedException(
                $"Action type '{definition.Type}' is not supported")
        };
    }

    private T DeserializeAction<T>(ActionDefinition definition) where T : IQuestAction
    {
        return JsonSerializer.Deserialize<T>(
            definition.Data.GetRawText(),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;
    }
}
```

### Step 3: Use in JSON Quest

```json
{
  "type": "teleport",
  "map_id": 1,
  "x": 4320,
  "y": 2765
}
```

### Step 4: Write Tests

**File:** `Tests/Game.Tests/Quest/Actions/TeleportActionTests.cs`

```csharp
using Xunit;
using Moq;
using QuantumCore.Game.Quest.Actions;

namespace QuantumCore.Game.Tests.Quest.Actions;

public class TeleportActionTests
{
    [Fact]
    public async Task test1_givenValidLocation_shouldTeleportPlayer()
    {
        // Arrange
        var action = new TeleportAction
        {
            MapId = 1,
            X = 100,
            Y = 200
        };

        var playerMock = new Mock<IPlayerEntity>();
        var worldMock = new Mock<IWorld>();
        var mapMock = new Mock<IMap>();

        mapMock.Setup(m => m.IsValidPosition(100, 200)).Returns(true);
        worldMock.Setup(w => w.GetMap(1)).Returns(mapMock.Object);

        var servicesMock = new Mock<IServiceProvider>();
        servicesMock.Setup(s => s.GetService(typeof(IWorld)))
            .Returns(worldMock.Object);

        var context = new QuestActionContext
        {
            Player = playerMock.Object,
            Services = servicesMock.Object
        };

        // Act
        await action.ExecuteAsync(context);

        // Assert
        playerMock.Verify(p => p.WarpAsync(1, 100, 200), Times.Once);
    }

    [Fact]
    public async Task test2_givenInvalidMap_shouldThrowException()
    {
        // Arrange
        var action = new TeleportAction
        {
            MapId = 999,  // Non-existent map
            X = 100,
            Y = 200
        };

        var worldMock = new Mock<IWorld>();
        worldMock.Setup(w => w.GetMap(999)).Returns((IMap?)null);

        var servicesMock = new Mock<IServiceProvider>();
        servicesMock.Setup(s => s.GetService(typeof(IWorld)))
            .Returns(worldMock.Object);

        var context = new QuestActionContext
        {
            Services = servicesMock.Object
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => action.ExecuteAsync(context));
    }
}
```

## Implementing a New Quest Condition

### Step 1: Create the Condition Class

**File:** `Libraries/Game.Server/Quest/Conditions/GuildMemberCondition.cs`

```csharp
using QuantumCore.API;
using QuantumCore.Game.Quest.Conditions;

namespace QuantumCore.Game.Quest.Conditions;

/// <summary>
/// Checks if player is a member of a guild
/// </summary>
public class GuildMemberCondition : IQuestCondition
{
    /// <summary>
    /// If true, player must be in a guild. If false, player must NOT be in a guild.
    /// </summary>
    public bool RequiresGuild { get; set; } = true;

    public bool Evaluate(QuestConditionContext context)
    {
        var player = context.Player;
        var hasGuild = player.Guild != null;

        return RequiresGuild == hasGuild;
    }
}
```

### Step 2: Register in Factory

**File:** `Libraries/Game.Server/Quest/QuestConditionFactory.cs`

```csharp
public class QuestConditionFactory
{
    public IQuestCondition CreateCondition(ConditionDefinition definition)
    {
        return definition.Type switch
        {
            "and" => DeserializeCondition<AndCondition>(definition),
            "level_min" => DeserializeCondition<LevelMinCondition>(definition),
            "guild_member" => DeserializeCondition<GuildMemberCondition>(definition),  // Add here!
            // ... other conditions
            _ => throw new NotSupportedException(
                $"Condition type '{definition.Type}' is not supported")
        };
    }

    private T DeserializeCondition<T>(ConditionDefinition definition)
        where T : IQuestCondition
    {
        return JsonSerializer.Deserialize<T>(
            definition.Data.GetRawText(),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;
    }
}
```

### Step 3: Use in JSON Quest

```json
{
  "condition": {
    "type": "guild_member",
    "requires_guild": true
  }
}
```

### Step 4: Write Tests

```csharp
[Fact]
public void test1_givenPlayerInGuild_shouldReturnTrue()
{
    // Arrange
    var condition = new GuildMemberCondition { RequiresGuild = true };
    var playerMock = new Mock<IPlayerEntity>();
    playerMock.Setup(p => p.Guild).Returns(Mock.Of<IGuild>());

    var context = new QuestConditionContext { Player = playerMock.Object };

    // Act
    var result = condition.Evaluate(context);

    // Assert
    Assert.True(result);
}

[Fact]
public void test2_givenPlayerNotInGuild_shouldReturnFalse()
{
    // Arrange
    var condition = new GuildMemberCondition { RequiresGuild = true };
    var playerMock = new Mock<IPlayerEntity>();
    playerMock.Setup(p => p.Guild).Returns((IGuild?)null);

    var context = new QuestConditionContext { Player = playerMock.Object };

    // Act
    var result = condition.Evaluate(context);

    // Assert
    Assert.False(result);
}
```

## Adding a New Trigger Type

### Step 1: Extend TriggerDefinition

**File:** `Libraries/Game.Server/Quest/Models/TriggerDefinition.cs`

```csharp
public class TriggerDefinition
{
    public string Type { get; set; } = "";
    public uint NpcId { get; set; }
    public uint MonsterId { get; set; }
    public uint ItemId { get; set; }
    public string ChatPattern { get; set; } = "";  // Add new property
    public ConditionDefinition? Condition { get; set; }
    public List<ActionDefinition> Actions { get; set; } = new();
}
```

### Step 2: Implement Trigger Handler

**File:** `Libraries/Game.Server/Quest/DeclarativeQuest.cs`

```csharp
private void RegisterTrigger(TriggerDefinition trigger)
{
    switch (trigger.Type)
    {
        case "npc_click":
            RegisterNpcClickTrigger(trigger);
            break;
        case "chat":  // Add new trigger type
            RegisterChatTrigger(trigger);
            break;
        // ... other triggers
    }
}

private void RegisterChatTrigger(TriggerDefinition trigger)
{
    var condition = CreateCondition(trigger.Condition);

    GameEventManager.RegisterChatEvent(
        $"{_definition.Id}:{State.CurrentState}",
        trigger.ChatPattern,
        async (player, message) =>
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
```

### Step 3: Extend GameEventManager

**File:** `Libraries/Game.Server/GameEventManager.cs`

```csharp
public static class GameEventManager
{
    private static readonly List<ChatEventRegistration> _chatEvents = new();

    public static void RegisterChatEvent(
        string eventName,
        string pattern,
        Func<IPlayerEntity, string, Task> callback,
        Func<IPlayerEntity, bool> condition)
    {
        _chatEvents.Add(new ChatEventRegistration
        {
            EventName = eventName,
            Pattern = new Regex(pattern, RegexOptions.IgnoreCase),
            Callback = callback,
            Condition = condition
        });
    }

    public static async Task OnChatMessage(IPlayerEntity player, string message)
    {
        foreach (var evt in _chatEvents.Where(e => e.Condition(player)))
        {
            if (evt.Pattern.IsMatch(message))
            {
                await evt.Callback(player, message);
            }
        }
    }

    private class ChatEventRegistration
    {
        public string EventName { get; init; } = "";
        public Regex Pattern { get; init; } = null!;
        public Func<IPlayerEntity, string, Task> Callback { get; init; } = null!;
        public Func<IPlayerEntity, bool> Condition { get; init; } = null!;
    }
}
```

## Writing C# Plugin Quests

For complex quests that require advanced logic, you can write C# plugin quests.

### Example: Complex Puzzle Quest

**File:** `Libraries/Game.Server/Quest/PuzzleQuest.cs`

```csharp
[Quest]
public class PuzzleQuest : Quest
{
    private readonly IItemManager _itemManager;
    private readonly ILogger<PuzzleQuest> _logger;
    private readonly Random _random = new();

    public PuzzleQuest(
        QuestState state,
        IPlayerEntity player,
        IItemManager itemManager,
        ILogger<PuzzleQuest> logger)
        : base(state, player)
    {
        _itemManager = itemManager;
        _logger = logger;
    }

    public override void Init()
    {
        GameEventManager.RegisterNpcClickEvent(
            "Puzzle Quest",
            20354,
            StartPuzzle,
            player => player.Vid == Player.Vid && player.Level >= 20
        );
    }

    private async Task StartPuzzle(IPlayerEntity player)
    {
        if (State.GetIntFlag("puzzle_solved") == 1)
        {
            Text("You already solved my puzzle!");
            Done();
            return;
        }

        Text("Welcome to my puzzle challenge!");
        Text("I'm thinking of a number between 1 and 100.");
        Next();

        await PlayPuzzleGame();
    }

    private async Task PlayPuzzleGame()
    {
        // Generate random number
        var secretNumber = _random.Next(1, 101);
        var attempts = 0;
        const int maxAttempts = 7;

        State.SetIntFlag("secret_number", secretNumber);

        while (attempts < maxAttempts)
        {
            Text($"Attempt {attempts + 1}/{maxAttempts}");
            Text("What's your guess?");

            // Show choices in ranges
            var guess = await Choice(false,
                "1-25",
                "26-50",
                "51-75",
                "76-100");

            // Convert choice to number range
            var guessNumber = guess switch
            {
                1 => _random.Next(1, 26),
                2 => _random.Next(26, 51),
                3 => _random.Next(51, 76),
                4 => _random.Next(76, 101),
                _ => 50
            };

            attempts++;

            if (guessNumber == secretNumber)
            {
                Text("Correct! You win!");
                State.SetIntFlag("puzzle_solved", 1);
                await RewardPlayer();
                return;
            }

            // Give hints
            if (guessNumber < secretNumber)
            {
                Text("Too low! Try higher.");
            }
            else
            {
                Text("Too high! Try lower.");
            }
            Next();
        }

        Text("Out of attempts! Better luck next time.");
        Done();
    }

    private async Task RewardPlayer()
    {
        // Complex reward calculation based on attempts
        var attempts = State.GetIntFlag("attempts", 0);
        var baseReward = 10000;
        var bonusMultiplier = Math.Max(1, 8 - attempts);
        var totalReward = baseReward * bonusMultiplier;

        _logger.LogInformation(
            "Player {Player} solved puzzle in {Attempts} attempts, reward: {Reward}",
            Player.Player.Id, attempts, totalReward);

        // Give rewards
        Player.GiveExperience((uint)totalReward);

        var legendaryItem = _itemManager.GetItem(99999);
        if (legendaryItem != null)
        {
            var item = _itemManager.CreateItem(legendaryItem);
            Player.Inventory.PlaceItem(item);
        }

        Text($"You earned {totalReward} experience and a legendary item!");
        Done();
    }
}
```

### When to Use C# Plugin Quests

Use C# for:
- Complex algorithms (puzzles, mini-games)
- Random generation (procedural content)
- Integration with external APIs
- Advanced database queries
- Performance-critical operations
- Complex state machines

Use JSON for:
- Simple dialog quests
- Fetch quests (collect X items)
- Kill quests (defeat X monsters)
- Delivery quests
- Linear progression quests

## Database Integration

### Adding Quest State Fields

If you need custom fields beyond the flexible flags system:

**1. Modify QuestState:**

```csharp
public class QuestState
{
    // Existing fields...
    public Dictionary<string, int> IntFlags { get; set; } = new();

    // Add custom typed properties
    public List<QuestObjective> Objectives { get; set; } = new();
}

public class QuestObjective
{
    public string Description { get; set; } = "";
    public int Current { get; set; }
    public int Required { get; set; }
    public bool IsCompleted => Current >= Required;
}
```

**2. Update Serialization:**

```csharp
internal class QuestStateData
{
    public Dictionary<string, int> IntFlags { get; set; } = new();
    public Dictionary<string, string> StringFlags { get; set; } = new();
    public Dictionary<string, bool> BoolFlags { get; set; } = new();
    public List<QuestObjective> Objectives { get; set; } = new();  // Add here
}
```

**3. Update Repository:**

```csharp
var questData = new QuestStateData
{
    IntFlags = state.IntFlags,
    StringFlags = state.StringFlags,
    BoolFlags = state.BoolFlags,
    Objectives = state.Objectives  // Include in serialization
};
```

### Query Quest Progress

```csharp
public async Task<List<QuestProgressDto>> GetQuestProgressAsync(uint playerId)
{
    var progress = await _db.PlayerQuests
        .Where(pq => pq.PlayerId == playerId && !pq.IsCompleted)
        .Select(pq => new QuestProgressDto
        {
            QuestId = pq.QuestId,
            CurrentState = pq.CurrentState,
            StartedAt = pq.StartedAt,
            ProgressPercent = CalculateProgress(pq.QuestDataJson)
        })
        .ToListAsync();

    return progress;
}
```

## Performance Optimization

### Caching Quest Definitions

Quest definitions are immutable and loaded at startup:

```csharp
public class DeclarativeQuestProvider : ILoadable
{
    // ImmutableDictionary for zero-copy reads
    public ImmutableDictionary<string, QuestDefinition> Quests { get; private set; }
        = ImmutableDictionary<string, QuestDefinition>.Empty;

    // Loaded once at startup, never modified
    public async Task LoadAsync(CancellationToken token = default)
    {
        // ... load quests
        Quests = questDict.ToImmutableDictionary();
    }
}
```

### Lazy Quest State Loading

Only load quest state when needed:

```csharp
public class LazyQuestState
{
    private readonly Lazy<Task<QuestState>> _stateLoader;

    public LazyQuestState(uint playerId, string questId, IDbQuestRepository repo)
    {
        _stateLoader = new Lazy<Task<QuestState>>(
            () => repo.GetQuestStateAsync(playerId, questId)
                ?? Task.FromResult(new QuestState { QuestId = questId }),
            LazyThreadSafetyMode.ExecutionAndPublication
        );
    }

    public Task<QuestState> GetStateAsync() => _stateLoader.Value;
}
```

### Batch State Updates

Instead of saving after every flag change:

```csharp
public class QuestStateBatch
{
    private readonly IDbQuestRepository _repository;
    private readonly Dictionary<string, QuestState> _pendingSaves = new();
    private Timer _flushTimer;

    public QuestStateBatch(IDbQuestRepository repository)
    {
        _repository = repository;
        _flushTimer = new Timer(FlushAsync, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
    }

    public void QueueSave(uint playerId, QuestState state)
    {
        var key = $"{playerId}:{state.QuestId}";
        _pendingSaves[key] = state;
    }

    private async void FlushAsync(object? state)
    {
        var toSave = _pendingSaves.ToList();
        _pendingSaves.Clear();

        foreach (var (key, questState) in toSave)
        {
            var playerId = uint.Parse(key.Split(':')[0]);
            await _repository.SaveQuestStateAsync(playerId, questState);
        }
    }
}
```

## Error Handling

### Quest Validation

Validate quests at load time:

```csharp
private List<string> ValidateQuest(QuestDefinition quest)
{
    var errors = new List<string>();

    // Required fields
    if (string.IsNullOrWhiteSpace(quest.Id))
        errors.Add("Quest ID is required");

    // Must have start state
    if (!quest.States.ContainsKey("start"))
        errors.Add("Quest must have a 'start' state");

    // Validate state transitions
    foreach (var (stateName, state) in quest.States)
    {
        ValidateStateTransitions(quest, stateName, state, errors);
    }

    // Validate NPC IDs exist
    foreach (var state in quest.States.Values)
    {
        foreach (var trigger in state.Triggers)
        {
            if (trigger.Type == "npc_click" && trigger.NpcId != 0)
            {
                if (!_npcManager.NpcExists(trigger.NpcId))
                {
                    errors.Add($"NPC {trigger.NpcId} does not exist");
                }
            }
        }
    }

    // Validate item IDs exist
    ValidateItemReferences(quest, errors);

    return errors;
}
```

### Runtime Error Handling

Handle errors gracefully during quest execution:

```csharp
private async Task ExecuteActions(List<ActionDefinition> actionDefs)
{
    foreach (var actionDef in actionDefs)
    {
        try
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
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex,
                "Quest {QuestId} action {ActionType} failed: {Message}",
                _definition.Id, actionDef.Type, ex.Message);

            // Notify player
            Player.SendChatMessage($"Quest error: {ex.Message}");

            // Don't crash the quest - continue with other actions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error in quest {QuestId} action {ActionType}",
                _definition.Id, actionDef.Type);

            // For unexpected errors, abort the quest
            throw;
        }
    }
}
```

## Debugging

### Logging

Add comprehensive logging:

```csharp
public class DeclarativeQuest : Quest
{
    private readonly ILogger<DeclarativeQuest> _logger;

    private async Task ExecuteActions(List<ActionDefinition> actionDefs)
    {
        _logger.LogDebug(
            "Executing {Count} actions for quest {QuestId}, state {State}",
            actionDefs.Count, _definition.Id, State.CurrentState);

        foreach (var actionDef in actionDefs)
        {
            _logger.LogTrace(
                "Executing action {ActionType} with data: {Data}",
                actionDef.Type, actionDef.Data.GetRawText());

            await action.ExecuteAsync(context);

            _logger.LogTrace("Action {ActionType} completed", actionDef.Type);
        }

        _logger.LogDebug("All actions executed successfully");
    }
}
```

### Debug Commands

Add admin commands for debugging:

```csharp
[Command("quest_state", "Shows current quest state")]
public class QuestStateCommand : IGameCommand
{
    public async Task ExecuteAsync(IPlayerEntity player, string[] args)
    {
        if (args.Length < 1)
        {
            player.SendChatMessage("Usage: /quest_state <quest_id>");
            return;
        }

        var questId = args[0];
        if (!player.Quests.TryGetValue(questId, out var quest))
        {
            player.SendChatMessage($"Quest {questId} not found");
            return;
        }

        var state = quest.State;
        player.SendChatMessage($"Quest: {questId}");
        player.SendChatMessage($"State: {state.CurrentState}");
        player.SendChatMessage($"Started: {state.StartedAt}");
        player.SendChatMessage($"Completed: {state.IsCompleted}");
        player.SendChatMessage("Int Flags:");
        foreach (var (key, value) in state.IntFlags)
        {
            player.SendChatMessage($"  {key} = {value}");
        }
    }
}

[Command("quest_reset", "Resets a quest")]
public class QuestResetCommand : IGameCommand
{
    private readonly IDbQuestRepository _questRepository;

    public async Task ExecuteAsync(IPlayerEntity player, string[] args)
    {
        if (args.Length < 1)
        {
            player.SendChatMessage("Usage: /quest_reset <quest_id>");
            return;
        }

        var questId = args[0];
        var newState = new QuestState
        {
            QuestId = questId,
            CurrentState = "start"
        };

        await _questRepository.SaveQuestStateAsync(player.Player.Id, newState);
        player.SendChatMessage($"Quest {questId} reset");
    }
}
```

## Testing Strategies

### Unit Testing Actions

```csharp
public class DialogActionTests
{
    [Fact]
    public async Task test1_givenMultiplePages_shouldDisplayInOrder()
    {
        // Arrange
        var action = new DialogAction
        {
            Pages = new List<DialogPage>
            {
                new() { Text = "Page 1", Next = true },
                new() { Text = "Page 2", Next = true }
            }
        };

        var questMock = new Mock<DeclarativeQuest>();
        var context = new QuestActionContext
        {
            Quest = questMock.Object
        };

        // Act
        await action.ExecuteAsync(context);

        // Assert
        questMock.Verify(q => q.Text("Page 1"), Times.Once);
        questMock.Verify(q => q.Text("Page 2"), Times.Once);
        questMock.Verify(q => q.Next(), Times.Exactly(2));
    }
}
```

### Integration Testing Quests

```csharp
public class QuestIntegrationTests : IClassFixture<GameServerFixture>
{
    private readonly GameServerFixture _fixture;

    public QuestIntegrationTests(GameServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task test1_givenSimpleQuest_shouldCompleteSuccessfully()
    {
        // Arrange
        var player = await _fixture.CreateTestPlayerAsync();
        var questManager = _fixture.GetService<IQuestManager>();

        // Act
        questManager.InitializePlayer(player);
        var quest = player.Quests["test_quest"];

        // Simulate NPC click
        await quest.OnNpcClick(20354);

        // Simulate player choice
        quest.Answer(1); // Select first option

        // Assert
        Assert.True(quest.State.IsCompleted);
        Assert.Equal("completed", quest.State.CurrentState);
    }
}
```

### End-to-End Testing

```csharp
[Fact]
public async Task test2_givenFetchQuest_shouldPersistProgressAcrossSessions()
{
    // Arrange
    var playerId = await _fixture.CreateTestPlayerIdAsync();

    // Session 1: Start quest
    {
        var player = await _fixture.LoadPlayerAsync(playerId);
        var quest = player.Quests["fetch_quest"];

        await quest.OnNpcClick(20354); // Accept quest
        quest.State.SetIntFlag("items_collected", 5);
        await _fixture.SavePlayerAsync(player);
    }

    // Session 2: Continue quest
    {
        var player = await _fixture.LoadPlayerAsync(playerId);
        var quest = player.Quests["fetch_quest"];

        // Verify progress persisted
        Assert.Equal(5, quest.State.GetIntFlag("items_collected"));

        // Complete quest
        quest.State.SetIntFlag("items_collected", 10);
        await quest.OnNpcClick(20354);
        await _fixture.SavePlayerAsync(player);
    }

    // Session 3: Verify completion
    {
        var player = await _fixture.LoadPlayerAsync(playerId);
        var quest = player.Quests["fetch_quest"];

        Assert.True(quest.State.IsCompleted);
    }
}
```

## Best Practices

### 1. Dependency Injection

Always use DI for dependencies:

```csharp
public class MyAction : IQuestAction
{
    // ❌ Bad: Direct instantiation
    private readonly ItemManager _itemManager = new ItemManager();

    // ✅ Good: Use context.GetService()
    public async Task ExecuteAsync(QuestActionContext context)
    {
        var itemManager = context.GetService<IItemManager>();
        // Use itemManager...
    }
}
```

### 2. Async/Await

Use async properly:

```csharp
// ❌ Bad: Blocking call
public Task ExecuteAsync(QuestActionContext context)
{
    var result = SomeAsyncMethod().Result; // Deadlock risk!
    return Task.CompletedTask;
}

// ✅ Good: Proper async
public async Task ExecuteAsync(QuestActionContext context)
{
    var result = await SomeAsyncMethod();
    // Process result...
}
```

### 3. Exception Handling

Handle exceptions appropriately:

```csharp
public async Task ExecuteAsync(QuestActionContext context)
{
    try
    {
        await RiskyOperation();
    }
    catch (SpecificException ex)
    {
        // Handle known errors
        _logger.LogWarning(ex, "Expected error occurred");
        throw new QuestExecutionException("User-friendly message", ex);
    }
    // Let unexpected exceptions propagate
}
```

### 4. Immutability

Keep quest definitions immutable:

```csharp
// ✅ Good: Immutable quest definition
public class QuestDefinition
{
    public string Id { get; init; } = "";  // init-only
    public ImmutableDictionary<string, StateDefinition> States { get; init; }
}

// ❌ Bad: Mutable quest definition
public class QuestDefinition
{
    public string Id { get; set; } = "";  // Can be changed!
    public Dictionary<string, StateDefinition> States { get; set; }
}
```

### 5. Logging

Use structured logging:

```csharp
// ✅ Good: Structured logging
_logger.LogInformation(
    "Quest {QuestId} completed by player {PlayerId} in {Duration}ms",
    questId, playerId, duration);

// ❌ Bad: String concatenation
_logger.LogInformation(
    "Quest " + questId + " completed by player " + playerId);
```

## Common Pitfalls

### 1. Forgetting to Save State

```csharp
// ❌ Bad: State change not saved
context.State.SetIntFlag("count", 10);
// State lost if server crashes!

// ✅ Good: Save after state change
context.State.SetIntFlag("count", 10);
await context.GetService<IDbQuestRepository>()
    .SaveQuestStateAsync(context.Player.Player.Id, context.State);
```

### 2. Race Conditions

```csharp
// ❌ Bad: Race condition
if (!_cache.ContainsKey(key))
{
    _cache[key] = await LoadData(); // Another thread might have added it!
}

// ✅ Good: Thread-safe operation
_cache.GetOrAdd(key, _ => LoadData().Result);
// Or use proper async locking (SemaphoreSlim)
```

### 3. Memory Leaks

```csharp
// ❌ Bad: Event handler not unregistered
GameEventManager.RegisterNpcClickEvent(...);
// If player disconnects, handler still registered!

// ✅ Good: Cleanup on player disconnect
public class QuestCleanup : IConnectionLifetimeListener
{
    public async Task OnDisconnectAsync(IConnection connection)
    {
        // Unregister all quest event handlers for this player
        GameEventManager.UnregisterPlayerEvents(connection.Player);
    }
}
```

## Migration Guide (Old Quest System → New)

### Before (Old C# Quest)

```csharp
[Quest]
public class OldQuest : Quest
{
    public override void Init()
    {
        GameEventManager.RegisterNpcClickEvent("Old Quest", 20354, Start);
    }

    private async Task Start(IPlayerEntity player)
    {
        Text("Hello!");
        Next();
        var choice = await Choice(false, "Yes", "No");
        Text($"You chose {choice}");
        Done();
    }
}
```

### After (New JSON Quest)

```json
{
  "id": "old_quest",
  "name": "Old Quest",
  "version": "2.0.0",
  "states": {
    "start": {
      "triggers": [
        {
          "type": "npc_click",
          "npc_id": 20354,
          "actions": [
            {
              "type": "dialog",
              "pages": [
                { "text": "Hello!", "next": true },
                {
                  "text": "Choose an option",
                  "choices": [
                    { "text": "Yes", "next_state": "yes_chosen" },
                    { "text": "No", "next_state": "no_chosen" }
                  ]
                }
              ]
            }
          ]
        }
      ]
    },
    "yes_chosen": {
      "triggers": [
        {
          "type": "npc_click",
          "npc_id": 20354,
          "actions": [
            {
              "type": "dialog",
              "pages": [{ "text": "You chose Yes", "next": true }]
            },
            { "type": "complete_quest" }
          ]
        }
      ]
    },
    "no_chosen": {
      "triggers": [
        {
          "type": "npc_click",
          "npc_id": 20354,
          "actions": [
            {
              "type": "dialog",
              "pages": [{ "text": "You chose No", "next": true }]
            },
            { "type": "complete_quest" }
          ]
        }
      ]
    }
  }
}
```

## Resources

- [Architecture Documentation](./architecture.md)
- [Designer Guide](./designer-guide.md)
- [Implementation Plan](/.claude/plans/swift-popping-cocoa.md)
- [Entity Framework Core Docs](https://learn.microsoft.com/en-us/ef/core/)
- [.NET Testing Best Practices](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

## Contributing

When adding new quest features:

1. Read the architecture documentation
2. Implement the feature
3. Write comprehensive tests
4. Update this guide with examples
5. Submit PR with clear description
6. Update designer guide if user-facing

Happy coding! 🚀
