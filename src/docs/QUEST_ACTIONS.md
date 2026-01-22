# Quest Actions Reference

Actions define **what happens** when a quest trigger fires. This document provides a complete reference for all 15 available action types.

## Table of Contents

- [Overview](#overview)
- [Quest Management Actions](#quest-management-actions)
- [Player Reward Actions](#player-reward-actions)
- [Item Actions](#item-actions)
- [World Interaction Actions](#world-interaction-actions)
- [Dialog Actions](#dialog-actions)
- [Control Flow Actions](#control-flow-actions)
- [Action Execution Order](#action-execution-order)

## Overview

### Basic Syntax

```json
"actions": [
  {
    "type": "action_name",
    "parameter1": "value1",
    "parameter2": "value2"
  }
]
```

### Action Categories

| Category | Actions | Purpose |
|----------|---------|---------|
| **Quest Management** | `set_quest_flag`, `inc_quest_flag`, `set_state`, `complete_quest` | Track progress, manage quest lifecycle |
| **Player Rewards** | `give_exp`, `give_gold` | Award experience and currency |
| **Item Management** | `give_item`, `remove_item` | Add/remove items from inventory |
| **World Interaction** | `warp`, `spawn_monster` | Teleport players, spawn entities |
| **Dialog** | `dialog`, `send_letter` | Show conversations, send notifications |
| **Control Flow** | `condition`, `delay` | Conditional logic, timed events |

---

## Quest Management Actions

### `set_quest_flag`

Sets a quest flag to a specific value. Quest flags store custom quest data.

**Parameters:**
- `flag` (string, required) - Flag name
- `value` (integer, required) - Value to set

**Example:**
```json
{
  "type": "set_quest_flag",
  "flag": "monsters_killed",
  "value": 0
}
```

**Use Cases:**
- Initialize counters
- Track quest progress
- Store player choices
- Mark quest milestones

---

### `inc_quest_flag`

Increments a quest flag by a specified amount.

**Parameters:**
- `flag` (string, required) - Flag name
- `amount` (integer, optional, default: 1) - Amount to increment

**Example:**
```json
{
  "type": "inc_quest_flag",
  "flag": "monsters_killed",
  "amount": 1
}
```

**Use Cases:**
- Count monster kills
- Track item collection
- Increment progress counters

**Note:** If the flag doesn't exist, it starts at 0 before incrementing.

---

### `set_state`

Transitions the quest to a different state.

**Parameters:**
- `state` (string, required) - Name of the target state

**Example:**
```json
{
  "type": "set_state",
  "state": "gathering"
}
```

**Use Cases:**
- Progress quest to next stage
- Jump to specific quest state
- Reset quest to earlier state

**Note:** Triggers `on_exit` actions for current state and `on_enter` actions for new state.

---

### `complete_quest`

Marks the quest as completed.

**Parameters:** None

**Example:**
```json
{
  "type": "complete_quest"
}
```

**Effects:**
- Sets `IsCompleted = true`
- Sets `CompletedAt` timestamp
- Quest no longer triggers

**Use Cases:**
- End of quest sequence
- After final reward distribution

---

## Player Reward Actions

### `give_exp`

Awards experience points to the player.

**Parameters:**
- `amount` (integer, required) - Experience points to award

**Example:**
```json
{
  "type": "give_exp",
  "amount": 1000
}
```

**Effects:**
- Adds experience to player
- May trigger level up
- Updates client UI

**Use Cases:**
- Quest completion rewards
- Stage completion bonuses
- Achievement rewards

---

### `give_gold`

Awards gold (currency) to the player.

**Parameters:**
- `amount` (integer, required) - Gold to award

**Example:**
```json
{
  "type": "give_gold",
  "amount": 5000
}
```

**Effects:**
- Adds gold to player inventory
- Updates client UI

**Use Cases:**
- Quest rewards
- NPC trades
- Bounty payments

---

## Item Actions

### `give_item`

Adds an item to the player's inventory.

**Parameters:**
- `item_id` (integer, required) - Item prototype ID
- `count` (integer, optional, default: 1) - Number of items

**Example:**
```json
{
  "type": "give_item",
  "item_id": 11001,
  "count": 1
}
```

**Effects:**
- Creates item instance
- Places in first available inventory slot
- Updates client inventory

**Error Handling:**
- If inventory full: Item not given (logged as warning)
- If item_id invalid: Action fails (logged as warning)

**Use Cases:**
- Quest rewards
- Starting equipment
- Key items for progression

---

### `remove_item`

Removes items from the player's inventory.

**Parameters:**
- `item_id` (integer, required) - Item prototype ID
- `count` (integer, optional, default: 1) - Number of items to remove

**Example:**
```json
{
  "type": "remove_item",
  "item_id": 50001,
  "count": 10
}
```

**Effects:**
- Removes specified quantity from inventory
- Updates client inventory

**Error Handling:**
- If player doesn't have enough: Nothing happens (logged as warning)

**Use Cases:**
- Quest turn-ins (collect X items)
- Consume quest items
- Item sacrifices

---

## World Interaction Actions

### `warp`

Teleports the player to a specific location.

**Parameters:**
- `x` (integer, required) - Target X coordinate
- `y` (integer, required) - Target Y coordinate
- `map` (string, optional) - Target map name (if changing maps)

**Example:**
```json
{
  "type": "warp",
  "x": 10000,
  "y": 20000
}
```

**With map change:**
```json
{
  "type": "warp",
  "x": 5000,
  "y": 5000,
  "map": "metin2_map_a1"
}
```

**Effects:**
- Instantly moves player to coordinates
- Refreshes player visibility to other clients

**Error Handling:**
- If map doesn't exist: Logged as warning, no warp
- If map name ambiguous: Logged as warning, no warp

**Use Cases:**
- Quest start/end cinematics
- Teleporting to quest location
- Returning to town
- Dungeon entrances

---

### `spawn_monster`

Spawns a monster at a specific location.

**Parameters:**
- `monster_id` (integer, required) - Monster prototype ID (vnum)
- `x` (integer, optional) - Spawn X coordinate (defaults to player position)
- `y` (integer, optional) - Spawn Y coordinate (defaults to player position)
- `range` (integer, optional, default: 0) - Random spawn range in units

**Example:**
```json
{
  "type": "spawn_monster",
  "monster_id": 101,
  "x": 10000,
  "y": 20000,
  "range": 500
}
```

**Spawn at player:**
```json
{
  "type": "spawn_monster",
  "monster_id": 101
}
```

**Effects:**
- Creates monster entity
- Spawns at specified location (± random range)
- Monster is visible to all players

**Error Handling:**
- If monster_id invalid: Logged as warning, no spawn
- If player map is null: Logged as warning, no spawn

**Use Cases:**
- Boss encounters
- Quest-specific enemies
- Story events
- Ambushes

---

## Dialog Actions

### `dialog`

Displays an interactive dialog conversation with multiple pages and player choices.

**Parameters:**
- `pages` (array, required) - Array of dialog pages

**Page Structure:**
- `text` (string) - Dialog text to display
- `next` (boolean) - If true, shows "Next" button
- `choices` (array) - Array of choice options

**Choice Structure:**
- `text` (string, required) - Choice button text
- `next_state` (string, optional) - State to transition to
- `actions` (array, optional) - Actions to execute
- `action` (string, optional) - Special action ("close")

**Simple Dialog:**
```json
{
  "type": "dialog",
  "pages": [
    {
      "text": "Hello, adventurer!",
      "next": true
    }
  ]
}
```

**Dialog with Choices:**
```json
{
  "type": "dialog",
  "pages": [
    {
      "text": "Will you help me?",
      "choices": [
        {
          "text": "Yes!",
          "next_state": "helping"
        },
        {
          "text": "No",
          "action": "close"
        }
      ]
    }
  ]
}
```

**Multi-Page Dialog:**
```json
{
  "type": "dialog",
  "pages": [
    {
      "text": "Long ago, in a distant land...",
      "next": true
    },
    {
      "text": "A great evil arose...",
      "next": true
    },
    {
      "text": "Will you stop it?",
      "choices": [
        { "text": "I will!", "next_state": "quest_active" },
        { "text": "Not now", "action": "close" }
      ]
    }
  ]
}
```

**Nested Actions in Choices:**
```json
{
  "type": "dialog",
  "pages": [
    {
      "text": "Choose your reward:",
      "choices": [
        {
          "text": "Gold",
          "actions": [
            { "type": "give_gold", "amount": 10000 },
            { "type": "complete_quest" }
          ]
        },
        {
          "text": "Experience",
          "actions": [
            { "type": "give_exp", "amount": 5000 },
            { "type": "complete_quest" }
          ]
        }
      ]
    }
  ]
}
```

**Use Cases:**
- NPC conversations
- Story exposition
- Player decisions
- Quest briefings
- Reward selection

**Note:** Dialogs use the Quest system's async dialog protocol (packets 0x2D outgoing, 0x1D incoming).

---

### `send_letter`

Sends a quest notification letter to the player.

**Parameters:**
- `title` (string, required) - Letter title
- `text` (string, required) - Letter body text

**Example:**
```json
{
  "type": "send_letter",
  "title": "Quest Objective",
  "text": "Collect 10 Iron Ore from the forest"
}
```

**Effects:**
- Displays quest letter in client UI
- Stored as quest flag for retrieval

**Use Cases:**
- Quest objective reminders
- Progress updates
- Completion notifications

---

## Control Flow Actions

### `condition`

Executes different actions based on a condition evaluation.

**Parameters:**
- `condition` (object, required) - Condition to evaluate
- `then` (array, required) - Actions if condition is true
- `else` (array, optional) - Actions if condition is false

**Example:**
```json
{
  "type": "condition",
  "condition": {
    "type": "level_min",
    "value": 20
  },
  "then": [
    { "type": "give_item", "item_id": 12001, "count": 1 }
  ],
  "else": [
    { "type": "give_item", "item_id": 11001, "count": 1 }
  ]
}
```

**Complex Condition:**
```json
{
  "type": "condition",
  "condition": {
    "type": "and",
    "conditions": [
      { "type": "quest_flag_gte", "flag": "score", "value": 100 },
      { "type": "level_min", "value": 30 }
    ]
  },
  "then": [
    { "type": "give_exp", "amount": 10000 },
    { "type": "give_item", "item_id": 99001, "count": 1 }
  ]
}
```

**Use Cases:**
- Level-based rewards
- Branching quest logic
- Variable rewards based on performance
- Gated content

---

### `delay`

Waits for a specified duration before continuing.

**Parameters:**
- `seconds` (integer, required) - Duration in seconds
- `message` (string, optional) - Message to log

**Example:**
```json
{
  "type": "delay",
  "seconds": 5,
  "message": "Waiting for ritual to complete..."
}
```

**Effects:**
- Pauses action execution for specified time
- Uses async Task.Delay (non-blocking)

**Use Cases:**
- Timed quest events
- Dramatic pauses in cutscenes
- Cooldowns between actions
- Ritual/crafting timers

**Warning:** Long delays block quest execution. Use sparingly.

---

## Action Execution Order

Actions execute **sequentially** in the order they appear:

```json
"actions": [
  { "type": "dialog", "pages": [...] },          // 1. Shows dialog first
  { "type": "remove_item", "item_id": 50001 },   // 2. Then removes item
  { "type": "give_exp", "amount": 1000 },        // 3. Then gives exp
  { "type": "give_item", "item_id": 11001 },     // 4. Then gives reward
  { "type": "complete_quest" }                    // 5. Finally completes
]
```

### Error Handling

- If an action fails, it **logs a warning** and **continues** with remaining actions
- Quest execution is **fault-tolerant**
- Failed actions don't rollback previous actions

### Best Practices

1. **Dialog first**: Show dialog before making changes
2. **Remove before give**: Take quest items before giving rewards
3. **Rewards before completion**: Give rewards before `complete_quest`
4. **State transitions last**: Change state after all actions complete
5. **Avoid excessive delays**: Keep delays under 30 seconds

---

## Summary

| Action | Primary Use | Parameters |
|--------|-------------|------------|
| `set_quest_flag` | Initialize/update flags | `flag`, `value` |
| `inc_quest_flag` | Increment counters | `flag`, `amount` |
| `set_state` | Change quest state | `state` |
| `complete_quest` | Finish quest | None |
| `give_exp` | Award experience | `amount` |
| `give_gold` | Award currency | `amount` |
| `give_item` | Add inventory item | `item_id`, `count` |
| `remove_item` | Remove inventory item | `item_id`, `count` |
| `warp` | Teleport player | `x`, `y`, `map` |
| `spawn_monster` | Create monster | `monster_id`, `x`, `y`, `range` |
| `dialog` | Show conversation | `pages` |
| `send_letter` | Send notification | `title`, `text` |
| `condition` | Conditional logic | `condition`, `then`, `else` |
| `delay` | Wait duration | `seconds`, `message` |

---

**Next:** [Quest Conditions Reference](QUEST_CONDITIONS.md)
