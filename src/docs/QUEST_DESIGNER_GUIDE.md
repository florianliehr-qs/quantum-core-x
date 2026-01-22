# Quest Designer Guide

A step-by-step tutorial for creating your first QuantumCore quest. **No programming experience required!**

## Prerequisites

- Text editor (VS Code, Notepad++, or similar)
- Basic JSON knowledge (we'll explain as we go)
- Access to `data/quests/` directory

## Tutorial: Create Your First Quest

We'll create a simple quest where:
1. Player talks to NPC
2. NPC asks for help collecting items
3. Player collects items
4. Player returns for reward

### Step 1: Create the Quest File

Create `data/quests/my_first_quest.json`:

```json
{
  "id": "my_first_quest",
  "name": "The Merchant's Request",
  "version": "1.0.0",
  "description": "Help the merchant collect herbs",

  "states": {
    "start": {
    }
  }
}
```

**Explanation:**
- `id` - Unique identifier (lowercase, underscores)
- `name` - Display name
- `version` - Track quest changes
- `description` - Brief summary
- `states` - Quest stages (must have "start")

### Step 2: Add the First Trigger

Add an NPC click trigger in the "start" state:

```json
"start": {
  "triggers": [
    {
      "type": "npc_click",
      "npc_id": 20354,
      "condition": {
        "type": "quest_not_started"
      },
      "actions": []
    }
  ]
}
```

**Explanation:**
- `npc_click` - Fires when player clicks NPC 20354
- `quest_not_started` - Only shows for first-time players
- `actions` - What happens (we'll add this next)

### Step 3: Add Dialog

Add a dialog action:

```json
"actions": [
  {
    "type": "dialog",
    "pages": [
      {
        "text": "Hello! I'm a merchant in need of help.",
        "next": true
      },
      {
        "text": "Can you collect 5 herbs for me?",
        "choices": [
          {
            "text": "Sure, I'll help!",
            "next_state": "collecting"
          },
          {
            "text": "Not now",
            "action": "close"
          }
        ]
      }
    ]
  }
]
```

**Explanation:**
- Page 1: Shows text with "Next" button
- Page 2: Gives player a choice
- "Sure" → goes to "collecting" state
- "Not now" → closes dialog

### Step 4: Add the Collecting State

Add a new state after "start":

```json
"collecting": {
  "on_enter": [
    {
      "type": "set_quest_flag",
      "flag": "herbs_collected",
      "value": 0
    },
    {
      "type": "send_letter",
      "title": "The Merchant's Request",
      "text": "Collect 5 Herbs from the forest"
    }
  ],
  "triggers": []
}
```

**Explanation:**
- `on_enter` - Runs when entering this state
- Sets "herbs_collected" counter to 0
- Sends quest letter to player

### Step 5: Add Completion Trigger

Add a trigger for when player has collected enough:

```json
"triggers": [
  {
    "type": "npc_click",
    "npc_id": 20354,
    "condition": {
      "type": "quest_flag_gte",
      "flag": "herbs_collected",
      "value": 5
    },
    "actions": [
      {
        "type": "dialog",
        "pages": [
          {
            "text": "Excellent! Here's your reward.",
            "next": true
          }
        ]
      },
      {
        "type": "give_exp",
        "amount": 500
      },
      {
        "type": "give_gold",
        "amount": 1000
      },
      {
        "type": "complete_quest"
      }
    ]
  }
]
```

**Explanation:**
- Only fires when herbs_collected >= 5
- Shows thank you dialog
- Gives 500 EXP and 1000 gold
- Marks quest as complete

### Step 6: Complete Quest File

Your finished quest:

```json
{
  "id": "my_first_quest",
  "name": "The Merchant's Request",
  "version": "1.0.0",
  "description": "Help the merchant collect herbs",

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
                  "text": "Hello! I'm a merchant in need of help.",
                  "next": true
                },
                {
                  "text": "Can you collect 5 herbs for me?",
                  "choices": [
                    {
                      "text": "Sure, I'll help!",
                      "next_state": "collecting"
                    },
                    {
                      "text": "Not now",
                      "action": "close"
                    }
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
        {
          "type": "set_quest_flag",
          "flag": "herbs_collected",
          "value": 0
        },
        {
          "type": "send_letter",
          "title": "The Merchant's Request",
          "text": "Collect 5 Herbs from the forest"
        }
      ],
      "triggers": [
        {
          "type": "npc_click",
          "npc_id": 20354,
          "condition": {
            "type": "quest_flag_gte",
            "flag": "herbs_collected",
            "value": 5
          },
          "actions": [
            {
              "type": "dialog",
              "pages": [
                {
                  "text": "Excellent! Here's your reward.",
                  "next": true
                }
              ]
            },
            {
              "type": "give_exp",
              "amount": 500
            },
            {
              "type": "give_gold",
              "amount": 1000
            },
            {
              "type": "complete_quest"
            }
          ]
        }
      ]
    }
  }
}
```

### Step 7: Test Your Quest

1. Save the file
2. Restart the game server
3. Check logs: `Loaded quest my_first_quest`
4. Click NPC 20354 in-game
5. Test the dialog and rewards!

## Common Patterns

### Level Requirements

```json
"condition": {
  "type": "and",
  "conditions": [
    { "type": "quest_not_started" },
    { "type": "level_min", "value": 10 }
  ]
}
```

### Item Turn-In Quest

```json
{
  "type": "npc_give",
  "npc_id": 20016,
  "item_id": 50001,
  "actions": [
    { "type": "inc_quest_flag", "flag": "items_given", "amount": 1 }
  ]
}
```

### Multi-Choice Rewards

```json
{
  "text": "Choose your reward:",
  "choices": [
    {
      "text": "Gold",
      "actions": [
        { "type": "give_gold", "amount": 10000 }
      ]
    },
    {
      "text": "Experience",
      "actions": [
        { "type": "give_exp", "amount": 5000 }
      ]
    }
  ]
}
```

## Tips & Tricks

1. **Always use quest_not_started** in start triggers
2. **Initialize flags in on_enter** before using them
3. **Give rewards before complete_quest**
4. **Test with different player levels**
5. **Use descriptive flag names** (e.g., "monsters_killed" not "count1")

## Troubleshooting

**Quest doesn't load:**
- Check JSON syntax with a validator
- Ensure "start" state exists
- Check server logs for errors

**Trigger doesn't fire:**
- Verify NPC ID is correct
- Check condition is satisfied
- Look for typos in flag names

**Rewards not given:**
- Ensure actions execute before complete_quest
- Check item/exp/gold values are valid
- Verify player has inventory space

## Next Steps

- Check [QUEST_EXAMPLES.md](QUEST_EXAMPLES.md) for more patterns
- Read [QUEST_ACTIONS.md](QUEST_ACTIONS.md) for all actions
- Read [QUEST_CONDITIONS.md](QUEST_CONDITIONS.md) for all conditions

Happy quest creating! 🎉
