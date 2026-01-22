# Getting Started with Quest Development

Quick guide to start developing and testing the declarative quest system.

## Prerequisites

- Docker Desktop installed and running
- Git
- Text editor (VSCode recommended)

## 1. Quick Start (2 minutes)

```bash
# Clone repository (if not already)
cd /path/to/quantum-core-x/src

# Start development environment
./dev-start.sh

# Wait for services to start, then apply migrations
docker-compose -f docker-compose.dev.yml exec game dotnet ef database update
```

That's it! Your development environment is ready.

## 2. Test the Quest System (5 minutes)

### View Example Quest

```bash
# Check the example quest
cat data/quests/test_declarative_quest.json
```

### Check Quest Loading

```bash
# View game server logs
docker-compose -f docker-compose.dev.yml logs game

# Look for quest loading messages:
# "Loaded quest test_declarative_quest (Test Declarative Quest) from test_declarative_quest.json"
# "Loaded 1 declarative quests"
```

### Inspect Database

```bash
# Connect to PostgreSQL
docker-compose -f docker-compose.dev.yml exec db-postgres psql -U metin2 -d metin2

# Check PlayerQuests table exists
\dt

# Exit
\q
```

## 3. Create Your First Quest (10 minutes)

### Create Quest File

```bash
# Create new quest
nano data/quests/my_first_quest.json
```

**Simple Example:**

```json
{
  "id": "my_first_quest",
  "name": "My First Quest",
  "version": "1.0.0",
  "description": "A simple test quest",

  "states": {
    "start": {
      "triggers": [
        {
          "type": "npc_click",
          "npc_id": 20354,
          "condition": {
            "type": "level_min",
            "value": 1
          },
          "actions": [
            {
              "type": "give_exp",
              "amount": 100
            },
            {
              "type": "give_gold",
              "amount": 50
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

### Reload Quests

```bash
# Restart game server to load new quest
docker-compose -f docker-compose.dev.yml restart game

# Verify it loaded
docker-compose -f docker-compose.dev.yml logs game | grep "my_first_quest"
```

## 4. Development Workflow

### Edit → Test → Debug Loop

1. **Edit quest JSON** in `data/quests/`
2. **Restart game server**: `docker-compose -f docker-compose.dev.yml restart game`
3. **Check logs**: `docker-compose -f docker-compose.dev.yml logs -f game`
4. **Test in game** (connect with client)
5. **Inspect database** if needed

### View Quest State

```bash
# SQL to check player quests
docker-compose -f docker-compose.dev.yml exec db-postgres psql -U metin2 -d metin2 -c \
  "SELECT \"QuestId\", \"CurrentState\", \"IsCompleted\", \"QuestDataJson\" FROM \"PlayerQuests\";"
```

### Debug Quest Issues

```bash
# Enable debug logging
echo "LOG_LEVEL_GAME=Debug" >> .env
docker-compose -f docker-compose.dev.yml restart game

# Follow logs in real-time
docker-compose -f docker-compose.dev.yml logs -f game

# Filter for quest-related logs
docker-compose -f docker-compose.dev.yml logs game | grep -i "quest\|trigger\|action"
```

## 5. Quest System Reference

### Available Actions

| Action | Description | Example |
|--------|-------------|---------|
| `set_quest_flag` | Set a flag value | `{"type": "set_quest_flag", "flag": "count", "value": 5}` |
| `inc_quest_flag` | Increment flag | `{"type": "inc_quest_flag", "flag": "kills", "amount": 1}` |
| `give_item` | Give item | `{"type": "give_item", "item_id": 11001, "count": 1}` |
| `remove_item` | Remove item | `{"type": "remove_item", "item_id": 50001, "count": 5}` |
| `give_exp` | Give experience | `{"type": "give_exp", "amount": 1000}` |
| `give_gold` | Give gold | `{"type": "give_gold", "amount": 500}` |
| `send_letter` | Quest notification | `{"type": "send_letter", "title": "Quest", "text": "..."}` |
| `set_state` | Change state | `{"type": "set_state", "state": "completed"}` |
| `complete_quest` | Mark complete | `{"type": "complete_quest"}` |

### Available Conditions

| Condition | Description | Example |
|-----------|-------------|---------|
| `quest_not_started` | Quest not active | `{"type": "quest_not_started"}` |
| `level_min` | Min level check | `{"type": "level_min", "value": 10}` |
| `level_max` | Max level check | `{"type": "level_max", "value": 50}` |
| `quest_flag_gte` | Flag >= value | `{"type": "quest_flag_gte", "flag": "kills", "value": 10}` |
| `quest_flag_eq` | Flag == value | `{"type": "quest_flag_eq", "flag": "step", "value": 2}` |
| `has_item` | Has item | `{"type": "has_item", "item_id": 50001, "count": 5}` |
| `class_check` | Player class | `{"type": "class_check", "class": "Warrior"}` |
| `and` | All conditions | `{"type": "and", "conditions": [...]}` |
| `or` | Any condition | `{"type": "or", "conditions": [...]}` |
| `not` | Invert condition | `{"type": "not", "condition": {...}}` |

### Trigger Types

- `npc_click` - Player clicks NPC
- `npc_give` - Player gives item to NPC (Phase 4)
- `kill` - Player kills monster (Phase 4)
- `item_acquired` - Player gets item (Phase 4)

## 6. Common Issues

### Quest Not Loading

```bash
# Check for JSON errors
docker-compose -f docker-compose.dev.yml logs game | grep -i "error\|warning"

# Validate JSON
cat data/quests/my_quest.json | python3 -m json.tool
```

### Quest State Not Saving

```bash
# Check database connection
docker-compose -f docker-compose.dev.yml exec game printenv | grep Database

# Check if migrations were applied
docker-compose -f docker-compose.dev.yml exec db-postgres psql -U metin2 -d metin2 -c "\dt"
```

### Changes Not Reflected

```bash
# Ensure you restart the server
docker-compose -f docker-compose.dev.yml restart game

# Check if quest file is mounted
docker-compose -f docker-compose.dev.yml exec game ls -la /app/data/quests/
```

## 7. Advanced Topics

For more details, see:
- **[DOCKER_DEV.md](DOCKER_DEV.md)** - Complete Docker setup guide
- **Phase 2 Plan** - Action and condition system details
- **Phase 3 Plan** - Declarative quest system architecture

## 8. Stop Development Environment

```bash
# Stop services (keep data)
./dev-stop.sh

# Or stop and clean all data
./dev-stop.sh --clean
```

## Need Help?

- Check logs: `docker-compose -f docker-compose.dev.yml logs game`
- View running services: `docker-compose -f docker-compose.dev.yml ps`
- Full documentation: `DOCKER_DEV.md`
- Test examples: `Tests/Game.Tests/Quest/`

Happy quest development! 🗡️
