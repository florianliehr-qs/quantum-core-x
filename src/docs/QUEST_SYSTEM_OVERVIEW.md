# Quest System Overview

The QuantumCore quest system allows you to create interactive, story-driven quests using JSON files instead of writing C# code. This declarative approach makes quest design accessible to non-programmers while maintaining the full power of programmatic quests.

## Table of Contents

- [What is the Declarative Quest System?](#what-is-the-declarative-quest-system)
- [Key Features](#key-features)
- [Architecture](#architecture)
- [Quick Start](#quick-start)
- [Core Concepts](#core-concepts)
- [Related Documentation](#related-documentation)

## What is the Declarative Quest System?

The declarative quest system is a **JSON-based framework** for creating quests without writing code. You define:

- **Story dialogs** - Multi-page conversations with NPCs
- **Player choices** - Branching narratives based on decisions
- **Quest states** - Multi-stage quests with progression tracking
- **Conditions** - Requirements players must meet
- **Actions** - Rewards, item transactions, teleportation, etc.
- **Triggers** - Events that advance the quest (NPC clicks, item turn-ins, etc.)

## Key Features

### ✅ No Code Required
Write quests in JSON - no C# knowledge needed.

### ✅ Interactive Dialogs
Create multi-page conversations with NPCs, including player choices that affect outcomes.

### ✅ State Machines
Build complex multi-stage quests with clear progression paths.

### ✅ Rich Conditions
Gate quest progress with level requirements, item checks, time windows, guild membership, and more.

### ✅ Powerful Actions
Reward players, teleport them, spawn monsters, execute conditional logic, and more.

### ✅ Hot-Reload Support
Edit quest JSON files and restart the server - no recompilation needed.

### ✅ Database Persistence
Quest progress automatically saves to the database, surviving server restarts.

## Architecture

```
┌─────────────────────────────────────────────────┐
│           Quest JSON Files                      │
│         (data/quests/*.json)                    │
└────────────────┬────────────────────────────────┘
                 │ Loaded at startup
                 ▼
┌─────────────────────────────────────────────────┐
│      DeclarativeQuestProvider                   │
│  - Scans quest directory                        │
│  - Validates quest definitions                  │
│  - Stores in memory                             │
└────────────────┬────────────────────────────────┘
                 │ Provides quest definitions
                 ▼
┌─────────────────────────────────────────────────┐
│           QuestManager                          │
│  - Registers both C# and JSON quests           │
│  - Initializes quests for players              │
│  - Manages quest lifecycle                     │
└────────────────┬────────────────────────────────┘
                 │ Creates per-player instances
                 ▼
┌─────────────────────────────────────────────────┐
│         DeclarativeQuest                        │
│  - Registers event triggers                     │
│  - Executes actions via factories               │
│  - Evaluates conditions                         │
│  - Manages state transitions                    │
│  - Persists state to database                   │
└─────────────────────────────────────────────────┘
```

### Component Overview

| Component | Purpose |
|-----------|---------|
| **QuestDefinition** | In-memory representation of a quest from JSON |
| **DeclarativeQuestProvider** | Loads and validates quests at startup |
| **QuestActionFactory** | Creates action instances from JSON (15 action types) |
| **QuestConditionFactory** | Creates condition instances from JSON (15 condition types) |
| **DeclarativeQuest** | Runtime quest instance that executes the quest |
| **QuestState** | Player-specific quest progress (flags, current state, completion) |
| **DbQuestRepository** | Database persistence for quest state |

## Quick Start

### 1. Create a Quest JSON File

Create `data/quests/my_first_quest.json`:

```json
{
  "id": "my_first_quest",
  "name": "My First Quest",
  "version": "1.0.0",
  "description": "A simple introduction quest",

  "states": {
    "start": {
      "triggers": [
        {
          "type": "npc_click",
          "npc_id": 20354,
          "condition": {
            "type": "quest_not_started"
          },
          "actions": [
            {
              "type": "dialog",
              "pages": [
                {
                  "text": "Hello, adventurer! Welcome to QuantumCore!",
                  "next": true
                },
                {
                  "text": "Would you like some experience points?",
                  "choices": [
                    {
                      "text": "Yes please!",
                      "actions": [
                        { "type": "give_exp", "amount": 100 },
                        { "type": "complete_quest" }
                      ]
                    },
                    {
                      "text": "No thanks",
                      "action": "close"
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

### 2. Start the Server

The quest will be automatically loaded:

```
[INFO] Loaded quest my_first_quest (My First Quest) from my_first_quest.json
[INFO] Loaded 1 declarative quests
```

### 3. Test In-Game

- Click on NPC 20354
- The dialog appears
- Make a choice
- Receive rewards!

## Core Concepts

### States

Quests are **state machines**. A quest starts in the `"start"` state and can transition to other states based on player actions.

```json
"states": {
  "start": { ... },
  "gathering": { ... },
  "completed": { ... }
}
```

Every quest **must** have a `"start"` state.

### Triggers

Triggers define **when** something happens in a quest. They fire in response to player actions:

| Trigger Type | Fires When | Required Fields |
|--------------|------------|----------------|
| `npc_click` | Player clicks on an NPC | `npc_id` |
| `npc_give` | Player gives item to NPC | `npc_id`, optional `item_id` |

```json
{
  "type": "npc_click",
  "npc_id": 20354,
  "condition": { ... },
  "actions": [ ... ],
  "next_state": "gathering"
}
```

### Actions

Actions define **what happens** when a trigger fires. You can execute multiple actions in sequence:

```json
"actions": [
  { "type": "give_exp", "amount": 1000 },
  { "type": "give_gold", "amount": 5000 },
  { "type": "give_item", "item_id": 11001, "count": 1 },
  { "type": "complete_quest" }
]
```

See [QUEST_ACTIONS.md](QUEST_ACTIONS.md) for all 15 action types.

### Conditions

Conditions define **requirements** that must be met for a trigger to fire:

```json
"condition": {
  "type": "and",
  "conditions": [
    { "type": "level_min", "value": 10 },
    { "type": "quest_not_started" },
    { "type": "has_item", "item_id": 50001, "count": 10 }
  ]
}
```

See [QUEST_CONDITIONS.md](QUEST_CONDITIONS.md) for all 15 condition types.

### Quest Flags

Quest flags store **custom data** for tracking progress:

```json
"actions": [
  { "type": "set_quest_flag", "flag": "monsters_killed", "value": 0 },
  { "type": "inc_quest_flag", "flag": "monsters_killed", "amount": 1 }
]
```

Check flags with conditions:

```json
"condition": {
  "type": "quest_flag_gte",
  "flag": "monsters_killed",
  "value": 10
}
```

### Dialogs

Dialogs create **interactive conversations** with NPCs:

```json
{
  "type": "dialog",
  "pages": [
    {
      "text": "Welcome, traveler!",
      "next": true
    },
    {
      "text": "Can you help me?",
      "choices": [
        { "text": "Yes!", "next_state": "helping" },
        { "text": "No", "action": "close" }
      ]
    }
  ]
}
```

### State Transitions

Move between quest states using:

1. **Trigger next_state**:
```json
{
  "type": "npc_click",
  "npc_id": 20354,
  "next_state": "gathering"
}
```

2. **Choice next_state**:
```json
{
  "text": "I'll help!",
  "next_state": "gathering"
}
```

3. **SetStateAction**:
```json
{
  "type": "set_state",
  "state": "gathering"
}
```

### Lifecycle Hooks

Execute actions when entering/exiting states:

```json
"gathering": {
  "on_enter": [
    { "type": "set_quest_flag", "flag": "items", "value": 0 },
    { "type": "send_letter", "title": "Quest Started", "text": "Collect 10 items" }
  ],
  "on_exit": [
    { "type": "send_letter", "title": "Quest Complete!", "text": "Well done!" }
  ],
  "triggers": [ ... ]
}
```

## Quest Persistence

Quest progress is **automatically saved** to the database:

- **QuestState table**: Stores current state, completion status, and timestamps
- **Quest flags**: Stored as JSON (IntFlags, StringFlags, BoolFlags)
- **Auto-save**: Triggers after every action execution and state transition

Players can log out and resume quests exactly where they left off.

## Performance Considerations

### Quest Loading
- Quests are loaded **once at startup** and cached in memory
- Typical load time: <50ms for 100 quests
- No performance impact during gameplay

### Quest Execution
- Actions execute **sequentially** in the order defined
- Database writes are **batched** per trigger
- No noticeable lag for typical quests (<10 actions per trigger)

### Database Impact
- One DB write per trigger execution
- Quest state is serialized to JSON (typically <1KB)
- PostgreSQL handles quest persistence efficiently

## Validation

Quests are validated at startup:

✅ Required fields present (`id`, `name`, `states`)
✅ `start` state exists
✅ All state transitions reference existing states
✅ All action/condition types are registered
✅ JSON syntax is valid

Invalid quests are **logged as warnings** but don't crash the server.

## Error Handling

### Runtime Errors
- Actions that fail are **logged but don't stop quest execution**
- Example: Trying to give a non-existent item logs a warning
- Quest continues with remaining actions

### Missing Resources
- Missing NPCs: Trigger won't fire
- Missing items: Action logs warning and continues
- Missing conditions: Trigger condition evaluates to false

## Debugging

Enable debug logging:

```bash
LOG_LEVEL_GAME=Debug docker-compose -f docker-compose.dev.yml up
```

Quest execution logs:
```
[DEBUG] Quest my_quest - Trigger fired: npc_click in state start
[DEBUG] Quest my_quest - Executing action GiveExpAction
[DEBUG] Quest my_quest - Transitioned from 'start' to 'gathering'
```

## Related Documentation

- **[QUEST_ACTIONS.md](QUEST_ACTIONS.md)** - Complete reference for all 15 action types
- **[QUEST_CONDITIONS.md](QUEST_CONDITIONS.md)** - Complete reference for all 15 condition types
- **[QUEST_DESIGNER_GUIDE.md](QUEST_DESIGNER_GUIDE.md)** - Step-by-step guide for creating quests
- **[QUEST_MIGRATION_GUIDE.md](QUEST_MIGRATION_GUIDE.md)** - How to convert C# quests to JSON
- **[QUEST_EXAMPLES.md](QUEST_EXAMPLES.md)** - Example quest patterns and templates

## Next Steps

1. Read the [Quest Designer Guide](QUEST_DESIGNER_GUIDE.md) for a hands-on tutorial
2. Browse [example quests](QUEST_EXAMPLES.md) for common patterns
3. Check the [action reference](QUEST_ACTIONS.md) and [condition reference](QUEST_CONDITIONS.md)
4. Create your first quest!

---

**QuantumCore Quest System** - Making quest design accessible to everyone.
