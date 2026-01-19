# Quest Designer Guide

**For:** Quest designers, content creators, non-programmers
**Level:** Beginner-friendly
**Goal:** Create engaging quests using JSON files without programming knowledge

## Introduction

Welcome! This guide will teach you how to create quests for Quantum Core X using JSON files. No programming experience required - if you can edit a text file, you can create quests!

### What You'll Learn

- How to structure a quest using JSON
- How to create dialog conversations
- How to give and take items
- How to create multi-stage quests
- How to add conditions (level requirements, item checks, etc.)
- Real-world quest examples

## Getting Started

### Prerequisites

- A text editor (VS Code, Notepad++, or even Notepad)
- The quest files are located in: `data/quests/`
- Each quest is a separate `.json` file

### Your First Quest

Let's create a simple "Hello World" quest where an NPC greets the player.

**File:** `data/quests/hello_world.json`

```json
{
  "id": "hello_world",
  "name": "Hello World",
  "version": "1.0.0",
  "description": "A simple greeting quest",

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
                  "text": "Hello, brave adventurer!",
                  "next": true
                },
                {
                  "text": "Welcome to our village!",
                  "next": true
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

**What this does:**
1. When player clicks NPC 20354
2. Shows two dialog pages
3. Player clicks "Next" to advance through pages

### Understanding the Structure

Every quest has these main parts:

```json
{
  "id": "unique_quest_id",           // Unique name for your quest
  "name": "Display Name",            // Name shown to players
  "version": "1.0.0",                // Version number
  "description": "What the quest is about",

  "states": {                        // Different stages of the quest
    "start": {                       // Every quest starts here
      "triggers": [                  // Things that activate quest actions
        // Triggers go here
      ]
    }
  }
}
```

## Quest States

Think of states as "chapters" in your quest. A quest always starts in the `"start"` state and can move to other states.

### Example: Multi-State Quest

```json
{
  "id": "delivery_quest",
  "name": "Delivery Quest",
  "version": "1.0.0",

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
                  "text": "Please deliver this letter to the blacksmith!",
                  "choices": [
                    { "text": "I'll do it!", "next_state": "delivering" },
                    { "text": "Not now.", "action": "close" }
                  ]
                }
              ]
            },
            { "type": "give_item", "item_id": 50001, "count": 1 }
          ]
        }
      ]
    },

    "delivering": {
      "triggers": [
        {
          "type": "npc_click",
          "npc_id": 20355,
          "actions": [
            {
              "type": "dialog",
              "pages": [
                { "text": "Ah, a letter for me! Thank you!", "next": true }
              ]
            },
            { "type": "remove_item", "item_id": 50001, "count": 1 },
            { "type": "give_exp", "amount": 1000 },
            { "type": "complete_quest" }
          ]
        }
      ]
    }
  }
}
```

**What happens:**
1. **Start state**: Player talks to NPC 20354, accepts quest, gets letter (item 50001)
2. **Delivering state**: Player talks to NPC 20355, gives letter, gets 1000 exp, quest completes

## Triggers

Triggers are events that start quest actions. Here are the available triggers:

### 1. NPC Click

When player clicks an NPC:

```json
{
  "type": "npc_click",
  "npc_id": 20354,
  "actions": [
    // Actions go here
  ]
}
```

### 2. Item Acquired

When player gets an item:

```json
{
  "type": "item_acquired",
  "item_id": 50001,
  "actions": [
    { "type": "set_quest_flag", "flag": "items_collected", "value": 1 }
  ]
}
```

### 3. Monster Kill

When player kills a monster:

```json
{
  "type": "kill",
  "monster_id": 101,
  "actions": [
    { "type": "inc_quest_flag", "flag": "kills", "amount": 1 }
  ]
}
```

### 4. Player Login

When player logs in:

```json
{
  "type": "login",
  "actions": [
    { "type": "dialog", "pages": [{ "text": "Welcome back!", "next": true }] }
  ]
}
```

## Actions

Actions are things that happen in response to triggers.

### Dialog Actions

Show dialog to players:

```json
{
  "type": "dialog",
  "pages": [
    {
      "text": "This is page 1",
      "next": true
    },
    {
      "text": "This is page 2 with choices",
      "choices": [
        { "text": "Option 1", "next_state": "state1" },
        { "text": "Option 2", "next_state": "state2" }
      ]
    }
  ]
}
```

**Dialog Page Types:**

1. **Next Page** - Player clicks "Next" to continue:
   ```json
   { "text": "Hello!", "next": true }
   ```

2. **Choice Page** - Player selects an option:
   ```json
   {
     "text": "What do you want?",
     "choices": [
       { "text": "Help me", "next_state": "helping" },
       { "text": "Nothing", "action": "close" }
     ]
   }
   ```

### Item Actions

**Give Item:**
```json
{ "type": "give_item", "item_id": 11001, "count": 1 }
```

**Remove Item:**
```json
{ "type": "remove_item", "item_id": 50001, "count": 10 }
```

### Reward Actions

**Give Experience:**
```json
{ "type": "give_exp", "amount": 1000 }
```

**Give Gold:**
```json
{ "type": "give_gold", "amount": 5000 }
```

### Quest State Actions

**Change State:**
```json
{ "type": "set_state", "state": "next_stage" }
```

**Complete Quest:**
```json
{ "type": "complete_quest" }
```

### Quest Flag Actions

Quest flags are like variables - they remember numbers or values.

**Set a Flag:**
```json
{ "type": "set_quest_flag", "flag": "items_collected", "value": 0 }
```

**Increase a Flag:**
```json
{ "type": "inc_quest_flag", "flag": "items_collected", "amount": 1 }
```

**Use Case:** Counting collected items

```json
"on_enter": [
  { "type": "set_quest_flag", "flag": "ore_collected", "value": 0 }
],
"triggers": [
  {
    "type": "item_acquired",
    "item_id": 50001,
    "actions": [
      { "type": "inc_quest_flag", "flag": "ore_collected", "amount": 1 }
    ]
  }
]
```

## Conditions

Conditions check if something is true before running actions.

### Level Checks

**Minimum Level:**
```json
{
  "type": "npc_click",
  "npc_id": 20354,
  "condition": {
    "type": "level_min",
    "value": 10
  },
  "actions": [
    // Only runs if player is level 10+
  ]
}
```

**Maximum Level:**
```json
{
  "condition": {
    "type": "level_max",
    "value": 20
  }
}
```

**Level Range:**
```json
{
  "condition": {
    "type": "level_range",
    "min": 10,
    "max": 20
  }
}
```

### Item Checks

**Has Item:**
```json
{
  "condition": {
    "type": "has_item",
    "item_id": 50001,
    "count": 10
  }
}
```

### Quest Checks

**Quest Not Started:**
```json
{
  "condition": {
    "type": "quest_not_started"
  }
}
```

**Quest Flag Check:**
```json
{
  "condition": {
    "type": "quest_flag_gte",
    "flag": "ore_collected",
    "value": 10
  }
}
```

### Combining Conditions

**AND (all must be true):**
```json
{
  "condition": {
    "type": "and",
    "conditions": [
      { "type": "level_min", "value": 10 },
      { "type": "has_item", "item_id": 50001, "count": 1 },
      { "type": "quest_not_started" }
    ]
  }
}
```

**OR (at least one must be true):**
```json
{
  "condition": {
    "type": "or",
    "conditions": [
      { "type": "level_min", "value": 20 },
      { "type": "has_item", "item_id": 99999, "count": 1 }
    ]
  }
}
```

**NOT (opposite):**
```json
{
  "condition": {
    "type": "not",
    "condition": { "type": "has_item", "item_id": 50001, "count": 1 }
  }
}
```

## State Lifecycle Hooks

### On Enter

Actions that run when entering a state:

```json
"gathering": {
  "on_enter": [
    { "type": "set_quest_flag", "flag": "started_gathering", "value": 1 },
    { "type": "send_letter", "title": "Quest Started", "text": "Collect 10 items" }
  ],
  "triggers": [ /* ... */ ]
}
```

### On Exit

Actions that run when leaving a state:

```json
"gathering": {
  "on_exit": [
    { "type": "clear_letter" }
  ],
  "triggers": [ /* ... */ ]
}
```

## Complete Quest Examples

### Example 1: Simple Fetch Quest

**Quest:** Collect 10 Iron Ore and return to NPC

```json
{
  "id": "iron_ore_collection",
  "name": "Iron Ore Collection",
  "version": "1.0.0",
  "description": "Collect iron ore for the blacksmith",

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
                  "text": "I need iron ore to forge weapons. Can you collect 10 Iron Ore for me?",
                  "choices": [
                    { "text": "Sure!", "next_state": "collecting" },
                    { "text": "Maybe later.", "action": "close" }
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
        { "type": "set_quest_flag", "flag": "ore_collected", "value": 0 },
        { "type": "send_letter", "title": "Iron Ore Collection", "text": "Collect 10 Iron Ore" }
      ],
      "triggers": [
        {
          "type": "item_acquired",
          "item_id": 50001,
          "actions": [
            { "type": "inc_quest_flag", "flag": "ore_collected", "amount": 1 }
          ]
        },
        {
          "type": "npc_click",
          "npc_id": 20354,
          "condition": {
            "type": "quest_flag_gte",
            "flag": "ore_collected",
            "value": 10
          },
          "actions": [
            {
              "type": "dialog",
              "pages": [
                { "text": "Perfect! Here's your reward.", "next": true }
              ]
            },
            { "type": "remove_item", "item_id": 50001, "count": 10 },
            { "type": "give_exp", "amount": 2000 },
            { "type": "give_gold", "amount": 10000 },
            { "type": "give_item", "item_id": 11001, "count": 1 },
            { "type": "complete_quest" }
          ]
        }
      ]
    }
  }
}
```

### Example 2: Kill Quest

**Quest:** Defeat 20 wolves

```json
{
  "id": "wolf_hunter",
  "name": "Wolf Hunter",
  "version": "1.0.0",
  "description": "Defeat wolves threatening the village",

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
                  "text": "Wolves are attacking our village! Please defeat 20 of them!",
                  "choices": [
                    { "text": "I'll help!", "next_state": "hunting" },
                    { "text": "Not interested.", "action": "close" }
                  ]
                }
              ]
            }
          ]
        }
      ]
    },

    "hunting": {
      "on_enter": [
        { "type": "set_quest_flag", "flag": "wolves_killed", "value": 0 },
        { "type": "send_letter", "title": "Wolf Hunt", "text": "Defeat 20 wolves" }
      ],
      "triggers": [
        {
          "type": "kill",
          "monster_id": 101,
          "actions": [
            { "type": "inc_quest_flag", "flag": "wolves_killed", "amount": 1 },
            {
              "type": "condition",
              "condition": {
                "type": "quest_flag_gte",
                "flag": "wolves_killed",
                "value": 20
              },
              "then": [
                { "type": "send_letter", "title": "Quest Complete!", "text": "Return to the village" }
              ]
            }
          ]
        },
        {
          "type": "npc_click",
          "npc_id": 20354,
          "condition": {
            "type": "quest_flag_gte",
            "flag": "wolves_killed",
            "value": 20
          },
          "actions": [
            {
              "type": "dialog",
              "pages": [
                { "text": "You saved our village! Thank you!", "next": true }
              ]
            },
            { "type": "give_exp", "amount": 5000 },
            { "type": "give_gold", "amount": 20000 },
            { "type": "complete_quest" }
          ]
        }
      ]
    }
  }
}
```

### Example 3: Branching Quest

**Quest:** Choose between two paths

```json
{
  "id": "moral_choice",
  "name": "A Moral Choice",
  "version": "1.0.0",
  "description": "Choose your path",

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
                  "text": "I found this mysterious artifact. What should I do with it?",
                  "choices": [
                    { "text": "Keep it for yourself", "next_state": "selfish_path" },
                    { "text": "Donate it to the museum", "next_state": "noble_path" }
                  ]
                }
              ]
            }
          ]
        }
      ]
    },

    "selfish_path": {
      "triggers": [
        {
          "type": "npc_click",
          "npc_id": 20354,
          "actions": [
            {
              "type": "dialog",
              "pages": [
                { "text": "You chose power over virtue...", "next": true }
              ]
            },
            { "type": "give_item", "item_id": 12001, "count": 1 },
            { "type": "give_exp", "amount": 3000 },
            { "type": "complete_quest" }
          ]
        }
      ]
    },

    "noble_path": {
      "triggers": [
        {
          "type": "npc_click",
          "npc_id": 20355,
          "actions": [
            {
              "type": "dialog",
              "pages": [
                { "text": "Thank you for your generous donation!", "next": true }
              ]
            },
            { "type": "give_gold", "amount": 50000 },
            { "type": "give_exp", "amount": 5000 },
            { "type": "complete_quest" }
          ]
        }
      ]
    }
  }
}
```

### Example 4: Multi-Stage Quest

**Quest:** Complex quest with multiple stages

```json
{
  "id": "legendary_sword",
  "name": "The Legendary Sword",
  "version": "1.0.0",
  "description": "Craft a legendary sword",

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
              { "type": "level_min", "value": 30 }
            ]
          },
          "actions": [
            {
              "type": "dialog",
              "pages": [
                {
                  "text": "I can forge you a legendary sword, but I need rare materials!",
                  "next": true
                },
                {
                  "text": "First, bring me 50 Iron Ore.",
                  "choices": [
                    { "text": "I'll get them!", "next_state": "collecting_iron" },
                    { "text": "Too difficult.", "action": "close" }
                  ]
                }
              ]
            }
          ]
        }
      ]
    },

    "collecting_iron": {
      "on_enter": [
        { "type": "set_quest_flag", "flag": "iron_collected", "value": 0 },
        { "type": "send_letter", "title": "Legendary Sword", "text": "Collect 50 Iron Ore" }
      ],
      "triggers": [
        {
          "type": "item_acquired",
          "item_id": 50001,
          "actions": [
            { "type": "inc_quest_flag", "flag": "iron_collected", "amount": 1 }
          ]
        },
        {
          "type": "npc_click",
          "npc_id": 20354,
          "condition": {
            "type": "quest_flag_gte",
            "flag": "iron_collected",
            "value": 50
          },
          "actions": [
            {
              "type": "dialog",
              "pages": [
                {
                  "text": "Good! Now I need 10 Dragon Scales. Defeat dragons to get them!",
                  "next": true
                }
              ]
            },
            { "type": "remove_item", "item_id": 50001, "count": 50 },
            { "type": "set_state", "state": "collecting_scales" }
          ]
        }
      ]
    },

    "collecting_scales": {
      "on_enter": [
        { "type": "set_quest_flag", "flag": "scales_collected", "value": 0 },
        { "type": "send_letter", "title": "Legendary Sword", "text": "Collect 10 Dragon Scales" }
      ],
      "triggers": [
        {
          "type": "item_acquired",
          "item_id": 50002,
          "actions": [
            { "type": "inc_quest_flag", "flag": "scales_collected", "amount": 1 }
          ]
        },
        {
          "type": "npc_click",
          "npc_id": 20354,
          "condition": {
            "type": "quest_flag_gte",
            "flag": "scales_collected",
            "value": 10
          },
          "actions": [
            {
              "type": "dialog",
              "pages": [
                {
                  "text": "Perfect! Let me forge this sword for you...",
                  "next": true
                },
                {
                  "text": "Here is your Legendary Sword! Use it well!",
                  "next": true
                }
              ]
            },
            { "type": "remove_item", "item_id": 50002, "count": 10 },
            { "type": "give_item", "item_id": 11999, "count": 1 },
            { "type": "give_exp", "amount": 50000 },
            { "type": "complete_quest" }
          ]
        }
      ]
    }
  }
}
```

## Tips and Best Practices

### 1. Quest IDs

- Use descriptive, unique IDs: `beginner_sword_quest` not `quest1`
- Use lowercase with underscores
- Never change IDs after deployment (breaks saved progress)

### 2. NPC IDs

- Find NPC IDs in the game data files
- Test with the correct NPC in game
- Document which NPC each ID represents

### 3. Item IDs

- Item IDs come from `item_proto` file
- Verify item IDs exist before using
- Test that items appear correctly

### 4. Quest Flags

- Use descriptive flag names: `ore_collected` not `flag1`
- Initialize flags in `on_enter` before using them
- Use `int` flags for counters, `bool` flags for true/false

### 5. Dialog Writing

- Keep dialog text concise (2-3 sentences per page)
- Use `next: true` for story pages
- Use `choices` for player decisions
- Maximum 4 choices per page (readability)

### 6. State Names

- Use descriptive names: `collecting`, `returning`, `completed`
- Every quest must have a `start` state
- Plan your state flow before writing

### 7. Testing

- Test the entire quest flow in-game
- Test with different character levels
- Test edge cases (what if player has no inventory space?)
- Have someone else test your quest

## Common Mistakes

### Mistake 1: Missing "start" State

❌ **Wrong:**
```json
"states": {
  "beginning": {  // Wrong! Must be "start"
    // ...
  }
}
```

✅ **Correct:**
```json
"states": {
  "start": {  // Correct!
    // ...
  }
}
```

### Mistake 2: Invalid State Transitions

❌ **Wrong:**
```json
{
  "text": "Go to next stage",
  "choices": [
    { "text": "OK", "next_state": "stage_typo" }  // State doesn't exist!
  ]
}
```

✅ **Correct:**
```json
{
  "text": "Go to next stage",
  "choices": [
    { "text": "OK", "next_state": "stage_2" }  // State exists
  ]
}

// And make sure you define it:
"states": {
  "start": { /* ... */ },
  "stage_2": { /* ... */ }  // Defined!
}
```

### Mistake 3: Forgetting to Initialize Flags

❌ **Wrong:**
```json
"collecting": {
  "triggers": [
    {
      "type": "item_acquired",
      "item_id": 50001,
      "actions": [
        { "type": "inc_quest_flag", "flag": "count", "amount": 1 }  // Not initialized!
      ]
    }
  ]
}
```

✅ **Correct:**
```json
"collecting": {
  "on_enter": [
    { "type": "set_quest_flag", "flag": "count", "value": 0 }  // Initialize first!
  ],
  "triggers": [
    {
      "type": "item_acquired",
      "item_id": 50001,
      "actions": [
        { "type": "inc_quest_flag", "flag": "count", "amount": 1 }
      ]
    }
  ]
}
```

### Mistake 4: JSON Syntax Errors

❌ **Wrong:**
```json
{
  "id": "my_quest",
  "name": "My Quest",  // Missing comma after this line!
  "states": { /* ... */ }
}
```

✅ **Correct:**
```json
{
  "id": "my_quest",
  "name": "My Quest",
  "states": { /* ... */ }
}
```

**Tip:** Use a JSON validator or an editor like VS Code that highlights errors!

## Quick Reference

### Available Actions

| Action | Purpose | Example |
|--------|---------|---------|
| `dialog` | Show conversation | `{ "type": "dialog", "pages": [...] }` |
| `give_item` | Give item to player | `{ "type": "give_item", "item_id": 11001, "count": 1 }` |
| `remove_item` | Take item from player | `{ "type": "remove_item", "item_id": 50001, "count": 10 }` |
| `give_exp` | Award experience | `{ "type": "give_exp", "amount": 1000 }` |
| `give_gold` | Award currency | `{ "type": "give_gold", "amount": 5000 }` |
| `set_quest_flag` | Set variable | `{ "type": "set_quest_flag", "flag": "name", "value": 0 }` |
| `inc_quest_flag` | Increase counter | `{ "type": "inc_quest_flag", "flag": "count", "amount": 1 }` |
| `set_state` | Change quest stage | `{ "type": "set_state", "state": "next_stage" }` |
| `complete_quest` | Finish quest | `{ "type": "complete_quest" }` |
| `send_letter` | Send quest update | `{ "type": "send_letter", "title": "...", "text": "..." }` |
| `clear_letter` | Clear quest update | `{ "type": "clear_letter" }` |

### Available Conditions

| Condition | Purpose | Example |
|-----------|---------|---------|
| `level_min` | Minimum level check | `{ "type": "level_min", "value": 10 }` |
| `level_max` | Maximum level check | `{ "type": "level_max", "value": 20 }` |
| `has_item` | Item possession check | `{ "type": "has_item", "item_id": 50001, "count": 10 }` |
| `quest_not_started` | Quest not begun | `{ "type": "quest_not_started" }` |
| `quest_flag_gte` | Flag >= value | `{ "type": "quest_flag_gte", "flag": "count", "value": 10 }` |
| `and` | All conditions true | `{ "type": "and", "conditions": [...] }` |
| `or` | Any condition true | `{ "type": "or", "conditions": [...] }` |
| `not` | Opposite of condition | `{ "type": "not", "condition": {...} }` |

### Available Triggers

| Trigger | Purpose | Example |
|---------|---------|---------|
| `npc_click` | Player clicks NPC | `{ "type": "npc_click", "npc_id": 20354 }` |
| `item_acquired` | Player gets item | `{ "type": "item_acquired", "item_id": 50001 }` |
| `kill` | Player kills monster | `{ "type": "kill", "monster_id": 101 }` |
| `login` | Player logs in | `{ "type": "login" }` |
| `levelup` | Player levels up | `{ "type": "levelup" }` |

## Getting Help

1. **Validate JSON**: Use [jsonlint.com](https://jsonlint.com) to check syntax
2. **Check Logs**: Server logs show quest loading errors
3. **Test In-Game**: Always test your quests before releasing
4. **Ask Community**: Discord server for help and feedback

## Next Steps

- Try modifying the example quests
- Create your own simple quest
- Learn about advanced features in the Developer Guide
- Join the community and share your quests!

Happy quest creation! 🎮
