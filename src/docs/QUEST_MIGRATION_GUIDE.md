# Quest Migration Guide: C# to JSON

This guide shows how to convert existing C# quests to the declarative JSON format.

## Why Migrate?

**Benefits of JSON Quests:**
- ✅ No recompilation needed (hot-reload)
- ✅ Non-programmers can create quests
- ✅ Easier to maintain and modify
- ✅ Better for version control (smaller diffs)
- ✅ Safer (no code execution vulnerabilities)

**When to keep C#:**
- Complex algorithmic logic
- Heavy integration with game systems
- Performance-critical quests
- Dynamic code generation

**Rule of Thumb:** If your quest is >90% dialogs, flags, and rewards → migrate to JSON.

## Migration Example: TestQuest

### Original C# Code

```csharp
[Quest]
public class TestQuest : Quest
{
    public override void Init()
    {
        GameEventManager.RegisterNpcClickEvent("Test Quest", 20354, Test,
            player => player.Vid == Player.Vid);
    }

    private async Task Test(IPlayerEntity player)
    {
        Text("Hello World from QuantumCore!");
        Text("This is using the current work in progress");
        Text("Quest API.");
        Next();

        Text("This is the second page showing how to easily");
        Text("using await to wait for user response");
        var choice = await Choice(false, "1st option", "2nd option");

        Text($"You've chosen: {choice}");
        Done();
    }
}
```

### Migrated JSON

```json
{
  "id": "test_quest",
  "name": "Test Quest",
  "version": "1.0.0",
  "description": "Test quest demonstrating dialog system",

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
                {
                  "text": "Hello World from QuantumCore![ENTER]This is using the current work in progress[ENTER]Quest API.",
                  "next": true
                },
                {
                  "text": "This is the second page showing how to easily[ENTER]using await to wait for user response",
                  "choices": [
                    {
                      "text": "1st option",
                      "actions": [
                        {
                          "type": "dialog",
                          "pages": [
                            { "text": "You've chosen: 0", "next": true }
                          ]
                        }
                      ]
                    },
                    {
                      "text": "2nd option",
                      "actions": [
                        {
                          "type": "dialog",
                          "pages": [
                            { "text": "You've chosen: 1", "next": true }
                          ]
                        }
                      ]
                    }
                  ]
                }
              ]
            }
          ]
        }
      ]
    }
  }
}
```

## Migration Mapping

### Event Registration

**C#:**
```csharp
GameEventManager.RegisterNpcClickEvent("Quest", 20354, Handler,
    player => player.Vid == Player.Vid);
```

**JSON:**
```json
{
  "type": "npc_click",
  "npc_id": 20354,
  "actions": [ ... ]
}
```

### Dialog - Text()

**C#:**
```csharp
Text("Hello!");
Text("Welcome!");
```

**JSON:**
```json
{
  "text": "Hello![ENTER]Welcome!"
}
```

### Dialog - Next()

**C#:**
```csharp
Text("Page 1");
Next();
```

**JSON:**
```json
{
  "text": "Page 1",
  "next": true
}
```

### Dialog - Choice()

**C#:**
```csharp
var choice = await Choice(false, "Option 1", "Option 2");
```

**JSON:**
```json
{
  "choices": [
    { "text": "Option 1", ... },
    { "text": "Option 2", ... }
  ]
}
```

### Dialog - Done()

**C#:**
```csharp
Done();
```

**JSON:**
- Dialog automatically closes after last page
- Or use `"action": "close"` in choice

### Quest Flags

**C#:**
```csharp
State.SetIntFlag("count", 0);
State.IncIntFlag("count", 1);
if (State.GetIntFlag("count") >= 10) { ... }
```

**JSON:**
```json
{ "type": "set_quest_flag", "flag": "count", "value": 0 }
{ "type": "inc_quest_flag", "flag": "count", "amount": 1 }

"condition": {
  "type": "quest_flag_gte",
  "flag": "count",
  "value": 10
}
```

### Rewards

**C#:**
```csharp
player.AddPoint(EPoint.EXPERIENCE, 1000);
player.SendPoints();

player.AddPoint(EPoint.GOLD, 5000);
player.SendPoints();

var item = _itemManager.CreateItem(11001, 1);
player.Inventory.PlaceItem(item);
player.SendInventory();
```

**JSON:**
```json
{ "type": "give_exp", "amount": 1000 }
{ "type": "give_gold", "amount": 5000 }
{ "type": "give_item", "item_id": 11001, "count": 1 }
```

### Conditions

**C#:**
```csharp
if (player.Player.Level >= 10) { ... }
```

**JSON:**
```json
"condition": {
  "type": "level_min",
  "value": 10
}
```

## Step-by-Step Migration Process

### 1. Analyze the C# Quest

- Identify all NPCs used
- List all dialog sequences
- Note all quest flags
- Document all rewards
- Map out state transitions

### 2. Create JSON Structure

```json
{
  "id": "quest_name",
  "name": "Display Name",
  "version": "1.0.0",
  "description": "Brief description",
  "states": {
    "start": {}
  }
}
```

### 3. Convert Event Handlers

**C# Init() method** → **JSON triggers array**

### 4. Convert Dialogs

- Each Text() → part of `"text"` field
- Each Next() → new page with `"next": true`
- Each Choice() → page with `"choices"` array
- Each Done() → automatic or `"action": "close"`

### 5. Convert Flags & Logic

- SetIntFlag → `set_quest_flag` action
- IncIntFlag → `inc_quest_flag` action
- GetIntFlag checks → `quest_flag_gte` condition
- if/else → `condition` action with then/else

### 6. Convert Rewards

- AddPoint(EXP) → `give_exp`
- AddPoint(GOLD) → `give_gold`
- Inventory.PlaceItem → `give_item`
- Inventory.RemoveItem → `remove_item`

### 7. Test the Migration

- Load quest in server
- Click through all dialogs
- Test all choices
- Verify all rewards
- Check quest completion

### 8. Remove C# Quest

Once verified:
```bash
git rm Libraries/Game.Server/Quest/OldQuest.cs
```

## Limitations & Workarounds

### Dynamic Text

**C# (not possible in JSON):**
```csharp
Text($"You've chosen: {choice}");
Text($"Hello {player.Name}!");
```

**Workaround:** Use separate choice actions with pre-defined text.

### Complex Calculations

**C# (not possible in JSON):**
```csharp
var reward = player.Level * 100 + questScore * 50;
player.AddPoint(EPoint.EXPERIENCE, reward);
```

**Workaround:**
- Use ConditionAction with level ranges
- Or keep quest in C# for complex logic

### Item Context

**C# (not directly in JSON):**
```csharp
var proto = _itemManager.GetItem(item.ItemId);
Text($"Thanks for the {proto.TranslatedName}!");
```

**Workaround:** Generic messages or separate triggers per item.

### Custom Services

**C# (not in JSON):**
```csharp
var customService = _serviceProvider.GetService<ICustomService>();
customService.DoSomething();
```

**Workaround:** Keep quest in C# or add new action type.

## Best Practices

1. **Migrate incrementally** - One quest at a time
2. **Keep backups** - Don't delete C# until JSON is tested
3. **Test thoroughly** - Click through all paths
4. **Document changes** - Note any behavior differences
5. **Version quests** - Use semantic versioning (1.0.0)

## Migration Checklist

- [ ] C# quest analyzed and documented
- [ ] JSON structure created
- [ ] All triggers converted
- [ ] All dialogs converted
- [ ] All flags converted
- [ ] All rewards converted
- [ ] All conditions converted
- [ ] Quest tested end-to-end
- [ ] No errors in server logs
- [ ] Behavior matches original
- [ ] C# file removed
- [ ] Migration documented

## Example: Complete Migration

**Before (C#):**
```csharp
[Quest]
public class SimpleQuest : Quest
{
    public override void Init()
    {
        GameEventManager.RegisterNpcClickEvent("Simple", 20354, async (player) =>
        {
            if (!State.HasIntFlag("started"))
            {
                Text("Help me collect 5 items!");
                var choice = await Choice(false, "Yes", "No");
                if (choice == 0)
                {
                    State.SetIntFlag("started", 1);
                    State.SetIntFlag("collected", 0);
                }
            }
            else if (State.GetIntFlag("collected") >= 5)
            {
                Text("Thank you!");
                player.AddPoint(EPoint.EXPERIENCE, 500);
                player.SendPoints();
                State.SetBoolFlag("completed", true);
                Done();
            }
        }, p => p.Vid == Player.Vid);
    }
}
```

**After (JSON):**
```json
{
  "id": "simple_quest",
  "name": "Simple Quest",
  "version": "1.0.0",
  "description": "Collect 5 items",
  "states": {
    "start": {
      "triggers": [
        {
          "type": "npc_click",
          "npc_id": 20354,
          "condition": { "type": "quest_not_started" },
          "actions": [
            {
              "type": "dialog",
              "pages": [
                {
                  "text": "Help me collect 5 items!",
                  "choices": [
                    { "text": "Yes", "next_state": "collecting" },
                    { "text": "No", "action": "close" }
                  ]
                }
              ]
            }
          ]
        }
      ]
    },
    "collecting": {
      "on_enter": [
        { "type": "set_quest_flag", "flag": "collected", "value": 0 }
      ],
      "triggers": [
        {
          "type": "npc_click",
          "npc_id": 20354,
          "condition": {
            "type": "quest_flag_gte",
            "flag": "collected",
            "value": 5
          },
          "actions": [
            {
              "type": "dialog",
              "pages": [{ "text": "Thank you!", "next": true }]
            },
            { "type": "give_exp", "amount": 500 },
            { "type": "complete_quest" }
          ]
        }
      ]
    }
  }
}
```

**Result:** Cleaner, more maintainable, and easier to modify!

---

**Need help?** Check [QUEST_SYSTEM_OVERVIEW.md](QUEST_SYSTEM_OVERVIEW.md) for reference.
