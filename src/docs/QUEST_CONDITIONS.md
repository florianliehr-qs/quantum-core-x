# Quest Conditions Reference

Conditions define **requirements** that must be met for quest triggers to fire. This document provides a complete reference for all 15 available condition types.

## Table of Contents

- [Overview](#overview)
- [Quest State Conditions](#quest-state-conditions)
- [Player Conditions](#player-conditions)
- [Item & Currency Conditions](#item--currency-conditions)
- [Time Conditions](#time-conditions)
- [Logical Operators](#logical-operators)
- [Best Practices](#best-practices)

## Overview

### Basic Syntax

```json
"condition": {
  "type": "condition_name",
  "parameter1": "value1",
  "parameter2": "value2"
}
```

### Usage Patterns

Conditions can be used in:

1. **Trigger conditions** - Gate trigger execution
2. **ConditionAction** - Conditional quest logic

**Example:**
```json
{
  "type": "npc_click",
  "npc_id": 20354,
  "condition": {
    "type": "level_min",
    "value": 10
  },
  "actions": [ ... ]
}
```

---

## Quest State Conditions

### `quest_not_started`

Checks if the quest hasn't been started yet.

**Parameters:** None

**Returns:** `true` if quest is in initial state or hasn't been created

**Example:**
```json
{
  "type": "quest_not_started"
}
```

**Use Cases:**
- First-time quest triggers
- Prevent duplicate quest starts
- New player quests

---

### `quest_completed`

Checks if a specific quest has been completed.

**Parameters:**
- `quest_id` (string, required) - Quest ID to check

**Returns:** `true` if quest is marked as completed

**Example:**
```json
{
  "type": "quest_completed",
  "quest_id": "beginner_sword_quest"
}
```

**Use Cases:**
- Quest chains (unlock next quest)
- Prerequisites for advanced quests
- Story progression gates

**Note:** References other quests by their `id` field.

---

### `quest_flag_gte`

Checks if a quest flag value is greater than or equal to a target value.

**Parameters:**
- `flag` (string, required) - Flag name
- `value` (integer, required) - Minimum value

**Returns:** `true` if flag >= value

**Example:**
```json
{
  "type": "quest_flag_gte",
  "flag": "monsters_killed",
  "value": 10
}
```

**Use Cases:**
- Kill count requirements
- Item collection progress
- Score thresholds
- Multi-step quest progress

**Note:** If flag doesn't exist, it's treated as 0.

---

### `quest_flag_eq`

Checks if a quest flag exactly equals a specific value.

**Parameters:**
- `flag` (string, required) - Flag name
- `value` (integer, required) - Expected value

**Returns:** `true` if flag == value

**Example:**
```json
{
  "type": "quest_flag_eq",
  "flag": "puzzle_solution",
  "value": 42
}
```

**Use Cases:**
- Exact match requirements
- Puzzle solutions
- Specific quest branches
- Binary flags (0 or 1)

---

## Player Conditions

### `level_min`

Checks if player level is at least the specified value.

**Parameters:**
- `value` (integer, required) - Minimum level

**Returns:** `true` if player level >= value

**Example:**
```json
{
  "type": "level_min",
  "value": 20
}
```

**Use Cases:**
- Level-gated quests
- Endgame content
- Difficulty scaling
- Class promotion quests

---

### `level_max`

Checks if player level is at most the specified value.

**Parameters:**
- `value` (integer, required) - Maximum level

**Returns:** `true` if player level <= value

**Example:**
```json
{
  "type": "level_max",
  "value": 10
}
```

**Use Cases:**
- Beginner quests
- Tutorial content
- Level-restricted events
- Prevent over-leveled access

---

### `level_range`

Checks if player level is within a specified range (inclusive).

**Parameters:**
- `min` (integer, required) - Minimum level (inclusive)
- `max` (integer, required) - Maximum level (inclusive)

**Returns:** `true` if min <= player level <= max

**Example:**
```json
{
  "type": "level_range",
  "min": 20,
  "max": 30
}
```

**Use Cases:**
- Level-appropriate quests
- Bracket-based content
- Scaling quest chains
- Tournament tiers

---

### `class_check`

Checks if player has a specific class.

**Parameters:**
- `class` (string, required) - Class name (e.g., "WARRIOR", "ASSASSIN", "SURA", "SHAMAN")

**Returns:** `true` if player class matches

**Example:**
```json
{
  "type": "class_check",
  "class": "WARRIOR"
}
```

**Use Cases:**
- Class-specific quests
- Class story missions
- Class skill quests
- Class equipment quests

**Valid Classes:**
- `WARRIOR`
- `ASSASSIN`
- `SURA`
- `SHAMAN`

---

### `guild_check`

Checks if player is (or is not) in a guild.

**Parameters:**
- `in_guild` (boolean, optional, default: true) - If true, checks player IS in guild; if false, checks player IS NOT in guild

**Returns:** `true` if guild membership matches expectation

**Must be in guild:**
```json
{
  "type": "guild_check",
  "in_guild": true
}
```

**Must NOT be in guild:**
```json
{
  "type": "guild_check",
  "in_guild": false
}
```

**Use Cases:**
- Guild-exclusive quests
- Guild recruitment quests
- Solo player quests
- Guild vs. independent content

---

## Item & Currency Conditions

### `has_item`

Checks if player has a specific item in inventory.

**Parameters:**
- `item_id` (integer, required) - Item prototype ID
- `count` (integer, optional, default: 1) - Minimum quantity

**Returns:** `true` if player has >= count of the item

**Example:**
```json
{
  "type": "has_item",
  "item_id": 50001,
  "count": 10
}
```

**Use Cases:**
- Quest turn-in requirements
- Crafting prerequisites
- Key item checks
- Collection quests

**Note:** Counts all stacks in inventory.

---

### `gold_check`

Checks if player has at least a specific amount of gold.

**Parameters:**
- `amount` (integer, required) - Minimum gold required

**Returns:** `true` if player gold >= amount

**Example:**
```json
{
  "type": "gold_check",
  "amount": 100000
}
```

**Use Cases:**
- Purchase requirements
- Donation quests
- Wealth checks
- Economic gates

---

## Time Conditions

### `time_check`

Checks if current server time (hour) is within a specified range.

**Parameters:**
- `start_hour` (integer, required) - Start hour (0-23, inclusive)
- `end_hour` (integer, required) - End hour (0-23, inclusive)

**Returns:** `true` if current hour is in range

**Simple range:**
```json
{
  "type": "time_check",
  "start_hour": 9,
  "end_hour": 17
}
```

**Midnight crossing:**
```json
{
  "type": "time_check",
  "start_hour": 22,
  "end_hour": 2
}
```

**Use Cases:**
- Daily quests (available 9am-5pm)
- Night-only events
- Time-limited content
- Event windows

**Note:** Handles ranges that cross midnight correctly (e.g., 22:00 to 02:00).

---

## Logical Operators

### `and`

All conditions must be true.

**Parameters:**
- `conditions` (array, required) - Array of conditions to check

**Returns:** `true` if ALL conditions are true

**Example:**
```json
{
  "type": "and",
  "conditions": [
    { "type": "level_min", "value": 10 },
    { "type": "has_item", "item_id": 50001, "count": 5 },
    { "type": "quest_not_started" }
  ]
}
```

**Use Cases:**
- Multiple requirements
- Complex prerequisites
- Compound checks

**Short-circuit:** Stops evaluating as soon as one condition fails.

---

### `or`

At least one condition must be true.

**Parameters:**
- `conditions` (array, required) - Array of conditions to check

**Returns:** `true` if ANY condition is true

**Example:**
```json
{
  "type": "or",
  "conditions": [
    { "type": "class_check", "class": "WARRIOR" },
    { "type": "class_check", "class": "ASSASSIN" }
  ]
}
```

**Use Cases:**
- Alternative requirements
- Multiple valid paths
- Class groups

**Short-circuit:** Stops evaluating as soon as one condition succeeds.

---

### `not`

Inverts a condition result.

**Parameters:**
- `condition` (object, required) - Condition to invert

**Returns:** `true` if condition is false

**Example:**
```json
{
  "type": "not",
  "condition": {
    "type": "guild_check",
    "in_guild": true
  }
}
```

**Use Cases:**
- Exclude certain players
- Inverse requirements
- "Must NOT have" checks

---

## Complex Condition Examples

### Multi-Level Requirements

```json
{
  "type": "and",
  "conditions": [
    {
      "type": "or",
      "conditions": [
        { "type": "level_range", "min": 20, "max": 30 },
        { "type": "level_range", "min": 40, "max": 50 }
      ]
    },
    { "type": "quest_not_started" }
  ]
}
```

### Quest Chain Prerequisite

```json
{
  "type": "and",
  "conditions": [
    { "type": "quest_completed", "quest_id": "intro_quest" },
    { "type": "quest_completed", "quest_id": "training_quest" },
    {
      "type": "not",
      "condition": {
        "type": "quest_completed",
        "quest_id": "current_quest"
      }
    }
  ]
}
```

### Time & Level Gated Event

```json
{
  "type": "and",
  "conditions": [
    { "type": "level_min", "value": 30 },
    { "type": "time_check", "start_hour": 20, "end_hour": 22 },
    { "type": "guild_check", "in_guild": true }
  ]
}
```

### Collect OR Kill Quest

```json
{
  "type": "or",
  "conditions": [
    { "type": "quest_flag_gte", "flag": "monsters_killed", "value": 10 },
    { "type": "quest_flag_gte", "flag": "items_collected", "value": 5 }
  ]
}
```

---

## Best Practices

### 1. Order Matters for Performance

Place **fast** conditions first in `and` blocks:

✅ **Good:**
```json
{
  "type": "and",
  "conditions": [
    { "type": "level_min", "value": 10 },        // Fast check
    { "type": "quest_not_started" },              // Fast check
    { "type": "has_item", "item_id": 50001 }      // Slower inventory scan
  ]
}
```

❌ **Bad:**
```json
{
  "type": "and",
  "conditions": [
    { "type": "has_item", "item_id": 50001 },     // Slow check first
    { "type": "level_min", "value": 10 }
  ]
}
```

### 2. Use Specific Conditions

Prefer specific conditions over general ones:

✅ **Good:** `level_range` for range checks
```json
{ "type": "level_range", "min": 10, "max": 20 }
```

❌ **Bad:** Combining `and` with `level_min` and `level_max`
```json
{
  "type": "and",
  "conditions": [
    { "type": "level_min", "value": 10 },
    { "type": "level_max", "value": 20 }
  ]
}
```

### 3. Avoid Deep Nesting

Keep condition trees shallow for readability:

✅ **Good:** 2-3 levels deep
```json
{
  "type": "and",
  "conditions": [
    { "type": "level_min", "value": 10 },
    {
      "type": "or",
      "conditions": [
        { "type": "class_check", "class": "WARRIOR" },
        { "type": "class_check", "class": "ASSASSIN" }
      ]
    }
  ]
}
```

❌ **Bad:** 5+ levels deep (hard to read)

### 4. Document Complex Conditions

Add comments in your quest JSON:

```json
{
  // Requires: Level 20+, completed intro, and either has key OR killed boss
  "type": "and",
  "conditions": [
    { "type": "level_min", "value": 20 },
    { "type": "quest_completed", "quest_id": "intro" },
    {
      "type": "or",
      "conditions": [
        { "type": "has_item", "item_id": 60001 },
        { "type": "quest_flag_gte", "flag": "boss_killed", "value": 1 }
      ]
    }
  ]
}
```

### 5. Test Edge Cases

Always test:
- Missing quest flags (should default to 0)
- Players without guild
- Different class types
- Midnight crossing for time checks
- Empty inventories

---

## Summary Table

| Condition | Category | Purpose | Parameters |
|-----------|----------|---------|------------|
| `quest_not_started` | Quest State | Quest is new | None |
| `quest_completed` | Quest State | Quest is done | `quest_id` |
| `quest_flag_gte` | Quest State | Flag >= value | `flag`, `value` |
| `quest_flag_eq` | Quest State | Flag == value | `flag`, `value` |
| `level_min` | Player | Level >= X | `value` |
| `level_max` | Player | Level <= X | `value` |
| `level_range` | Player | Level in range | `min`, `max` |
| `class_check` | Player | Has class | `class` |
| `guild_check` | Player | In/not in guild | `in_guild` |
| `has_item` | Item/Currency | Has item | `item_id`, `count` |
| `gold_check` | Item/Currency | Has gold | `amount` |
| `time_check` | Time | Hour in range | `start_hour`, `end_hour` |
| `and` | Logical | All true | `conditions` |
| `or` | Logical | Any true | `conditions` |
| `not` | Logical | Invert | `condition` |

---

**Next:** [Quest Designer Guide](QUEST_DESIGNER_GUIDE.md)
