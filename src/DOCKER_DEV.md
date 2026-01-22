# Docker Development Environment

Complete development setup with PostgreSQL for testing the quest system and other features.

## Quick Start

### 1. Setup Environment

```bash
# Copy environment template
cp .env.example .env

# Edit .env if needed (optional, defaults work fine)
nano .env
```

### 2. Start Services

```bash
# Start all services
docker-compose -f docker-compose.dev.yml up -d

# Or start with logs
docker-compose -f docker-compose.dev.yml up
```

### 3. Verify Services

```bash
# Check running containers
docker-compose -f docker-compose.dev.yml ps

# View logs
docker-compose -f docker-compose.dev.yml logs -f game
docker-compose -f docker-compose.dev.yml logs -f db-postgres
```

### 4. Apply Database Migrations

```bash
# Run migrations (first time setup)
docker-compose -f docker-compose.dev.yml exec game dotnet ef database update

# Or from host (if you have .NET SDK)
dotnet ef database update --project Data/Game.Persistence/Game.Persistence.csproj --startup-project Executables/Game/Game.csproj
```

## Services

| Service | Port | Description | URL |
|---------|------|-------------|-----|
| **Game Server** | 13001 | Main game server | `localhost:13001` |
| **Auth Server** | 11002 | Authentication service | `localhost:11002` |
| **PostgreSQL** | 5432 | Database | `localhost:5432` |
| **Redis** | 6379 | Cache/distributed memory | `localhost:6379` |
| **pgAdmin** | 5050 | PostgreSQL web UI (optional) | http://localhost:5050 |
| **Redis Commander** | 8081 | Redis web UI (optional) | http://localhost:8081 |

## Testing Quest Features

### 1. Hot-Reload Quest Files

Quest JSON files are mounted from `./data/quests/` directory:

```bash
# Edit quest file
nano data/quests/test_declarative_quest.json

# Restart game server to reload
docker-compose -f docker-compose.dev.yml restart game
```

### 2. View Quest Logs

```bash
# Follow game server logs
docker-compose -f docker-compose.dev.yml logs -f game

# Filter for quest-related logs
docker-compose -f docker-compose.dev.yml logs game | grep -i quest
```

### 3. Database Inspection

**Option A: Using pgAdmin (Web UI)**
1. Start with tools profile: `docker-compose -f docker-compose.dev.yml --profile tools up -d`
2. Open http://localhost:5050
3. Login with credentials from `.env` (default: admin@quantumcore.local / admin)
4. Add server:
   - Host: `db-postgres`
   - Port: `5432`
   - Database: `metin2`
   - Username: `metin2`
   - Password: `metin2`

**Option B: Using psql CLI**
```bash
# Connect to PostgreSQL
docker-compose -f docker-compose.dev.yml exec db-postgres psql -U metin2 -d metin2

# Query player quests
SELECT * FROM "PlayerQuests";

# Query quest state for specific player
SELECT * FROM "PlayerQuests" WHERE "PlayerId" = 1;
```

**Option C: From Host**
```bash
# If you have PostgreSQL client installed
psql -h localhost -p 5432 -U metin2 -d metin2
```

## Configuration

### Environment Variables

All configurable via `.env` file:

```bash
# Database
POSTGRES_USER=metin2          # Database username
POSTGRES_PASSWORD=metin2      # Database password
POSTGRES_DB=metin2           # Database name
POSTGRES_PORT=5432           # Exposed port

# Game Server
GAME_PORT=13001              # Game server port
LOG_LEVEL_GAME=Information   # Game log level
LOG_LEVEL_EF=Warning         # Entity Framework log level

# Auth Server
AUTH_PORT=11002              # Auth server port

# Redis
REDIS_PORT=6379              # Redis port

# Optional Tools
PGADMIN_PORT=5050           # pgAdmin port
PGADMIN_EMAIL=admin@...     # pgAdmin login email
PGADMIN_PASSWORD=admin      # pgAdmin login password
REDIS_COMMANDER_PORT=8081   # Redis Commander port
```

### Modify appsettings.json

```bash
# Edit game server config
nano Executables/Game/appsettings.json

# Restart to apply changes
docker-compose -f docker-compose.dev.yml restart game
```

## Common Tasks

### View All Logs

```bash
docker-compose -f docker-compose.dev.yml logs -f
```

### Restart Specific Service

```bash
docker-compose -f docker-compose.dev.yml restart game
docker-compose -f docker-compose.dev.yml restart db-postgres
```

### Rebuild After Code Changes

```bash
# Rebuild game server
docker-compose -f docker-compose.dev.yml build game

# Restart with new build
docker-compose -f docker-compose.dev.yml up -d game
```

### Clean Database and Start Fresh

```bash
# Stop all services
docker-compose -f docker-compose.dev.yml down

# Remove database volume
docker volume rm quantumcore-postgres-dev-data

# Start services (will create fresh database)
docker-compose -f docker-compose.dev.yml up -d

# Run migrations
docker-compose -f docker-compose.dev.yml exec game dotnet ef database update
```

### Access Redis CLI

```bash
docker-compose -f docker-compose.dev.yml exec cache redis-cli

# Inside redis-cli
> KEYS *
> GET some_key
> FLUSHALL  # Clear all data
```

### Database Backup

```bash
# Backup database
docker-compose -f docker-compose.dev.yml exec -T db-postgres pg_dump -U metin2 metin2 > backup.sql

# Restore database
docker-compose -f docker-compose.dev.yml exec -T db-postgres psql -U metin2 metin2 < backup.sql
```

## Optional Tools

Start with web-based management tools:

```bash
# Start with pgAdmin and Redis Commander
docker-compose -f docker-compose.dev.yml --profile tools up -d

# Or just add them to running stack
docker-compose -f docker-compose.dev.yml --profile tools up -d pgadmin redis-commander
```

## Troubleshooting

### Game Server Won't Start

```bash
# Check logs
docker-compose -f docker-compose.dev.yml logs game

# Check if database is ready
docker-compose -f docker-compose.dev.yml exec db-postgres pg_isready -U metin2

# Verify connection string
docker-compose -f docker-compose.dev.yml exec game printenv | grep Database
```

### Database Connection Issues

```bash
# Check PostgreSQL status
docker-compose -f docker-compose.dev.yml ps db-postgres

# View PostgreSQL logs
docker-compose -f docker-compose.dev.yml logs db-postgres

# Test connection from game container
docker-compose -f docker-compose.dev.yml exec game nc -zv db-postgres 5432
```

### Port Already in Use

```bash
# Change port in .env file
echo "GAME_PORT=13002" >> .env

# Restart services
docker-compose -f docker-compose.dev.yml up -d
```

### Out of Disk Space

```bash
# Clean up old containers and images
docker system prune -a

# Remove unused volumes (careful!)
docker volume prune
```

## Performance Tips

### For Development

- Use named volumes for better performance (already configured)
- Disable logging in appsettings.json for high-frequency operations
- Use Redis memory limits (already configured: 256MB with LRU eviction)

### For Testing Quest System

```bash
# Enable debug logging for quests
docker-compose -f docker-compose.dev.yml down
echo "LOG_LEVEL_GAME=Debug" >> .env
docker-compose -f docker-compose.dev.yml up -d
```

## Cleanup

### Stop All Services

```bash
docker-compose -f docker-compose.dev.yml down
```

### Remove All Data (Reset Everything)

```bash
# Stop and remove containers, networks, volumes
docker-compose -f docker-compose.dev.yml down -v

# Remove images too
docker-compose -f docker-compose.dev.yml down -v --rmi all
```

## Differences from Production

This development setup includes:
- ✅ PostgreSQL instead of MySQL/MariaDB
- ✅ Quest JSON files mounted for hot-reload
- ✅ Optional web-based management tools
- ✅ Debug-friendly logging
- ✅ Named networks for easy debugging
- ⚠️ Insecure default passwords
- ⚠️ No SSL/TLS
- ⚠️ No resource limits
- ⚠️ Not optimized for performance

**Do not use this setup in production!**

## Next Steps

1. Start services and verify they're running
2. Apply database migrations
3. Test quest system with example quest
4. Create your own quest JSON files in `data/quests/`
5. Monitor logs and database changes

Happy developing! 🚀
