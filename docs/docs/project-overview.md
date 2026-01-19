# Quantum Core X - Comprehensive Project Overview

## What is Quantum Core X?

Quantum Core X (QCX) is a **modern, open-source C# implementation of a Metin2 game server**. Metin2 is a popular MMORPG (Massively Multiplayer Online Role-Playing Game), and QCX provides a complete server infrastructure to run private Metin2 servers.

This is a fork of the original QuantumCore C# project, redesigned to be more maintainable, modern, and feature-complete while implementing missing features from the original game.

## Key Information

- **License:** Mozilla Public License 2.0
- **Repository:** [github.com/MeikelLP/quantum-core-x](https://github.com/MeikelLP/quantum-core-x)
- **Language:** C# 12+ with latest language features
- **Framework:** .NET 9.0
- **Status:** Active development (~30-40% feature complete)

## Technology Stack

### Core Framework
- **Language & Runtime:** C# 12+ with .NET 9.0
- **Language Features:** Implicit usings, nullable reference types, latest C# features
- **Architecture:** Layered, modular architecture with clean separation of concerns

### Networking & Communication
- **Networking:** Custom Core.Networking library with packet handling
- **Protocol:** Packet-based TCP/IP communication
- **Encryption:** XTEA encryption for packet security
- **Compression:** LZO compression for data handling
- **Packet Handlers:** 37+ implemented packet handlers
- **Code Generation:** Source generators for network packet serialization/deserialization

### Data & Persistence
- **ORM:** Entity Framework Core 9.0
- **Databases:** Multi-database support
  - PostgreSQL (via Npgsql.EntityFrameworkCore.PostgreSQL)
  - MySQL (via Pomelo.EntityFrameworkCore.MySql)
  - SQLite (for development/testing)
- **Caching:** Redis (via BeetleX.Redis) for distributed game world memory
- **In-Memory:** Custom Core.Caching for distributed state management

### Infrastructure & Deployment
- **Dependency Injection:** Microsoft.Extensions.DependencyInjection with keyed services
- **Logging:** Serilog with multiple sinks (console, file)
- **Configuration:** Microsoft.Extensions.Configuration
- **Monitoring:** Prometheus metrics support (via plugin)
- **Containerization:** Docker Compose for development environment
- **Security:** BCrypt.Net-Next for password hashing

### Plugin System
- **Framework:** Weikio plugin framework integrated with Microsoft DI
- **Discovery:** Automatic plugin discovery and registration
- **Types:** Multiple plugin interfaces for extensibility

### Testing & Quality
- **Testing Frameworks:** Standard .NET testing frameworks
- **Integration Testing:** TestContainers for database integration tests
- **Test Projects:** 7 comprehensive test projects with 100+ test cases
- **Benchmarks:** BenchmarkDotNet for performance testing

## Architecture Overview

### Layered Architecture

```
┌─────────────────────────────────────────────┐
│     Executables (Entry Points)              │
│  ┌──────────┬──────────┬────────┬────────┐ │
│  │  Game    │  Auth    │ Single │  Docs  │ │
│  │  Server  │  Server  │  Mode  │   Gen  │ │
│  └──────────┴──────────┴────────┴────────┘ │
└─────────────────────────────────────────────┘
                    │
┌─────────────────────────────────────────────┐
│     Libraries (Business Logic)              │
│  ┌──────────────┬─────────────┬──────────┐ │
│  │ Game.Server  │ Auth.Server │   Game   │ │
│  │              │             │ Commands │ │
│  └──────────────┴─────────────┴──────────┘ │
└─────────────────────────────────────────────┘
                    │
┌─────────────────────────────────────────────┐
│     Core Infrastructure                     │
│  ┌────────┬────────────┬──────────────────┐│
│  │  Core  │    Core    │  Core.Networking │││
│  │        │ Networking │    Generators    │││
│  │        │            │                  │││
│  └────────┴────────────┴──────────────────┘│
│  ┌──────────────────────────────────────┐  │
│  │       CorePluginAPI                  │  │
│  └──────────────────────────────────────┘  │
└─────────────────────────────────────────────┘
                    │
┌─────────────────────────────────────────────┐
│     Data Access Layer                       │
│  ┌───────────┬─────────────┬────────────┐  │
│  │   Auth    │    Game     │    Game    │  │
│  │Persistence│ Persistence │  Caching   │  │
│  └───────────┴─────────────┴────────────┘  │
│  ┌───────────┬─────────────────────────┐   │
│  │   Core    │    Core.Persistence     │   │
│  │  Caching  │                         │   │
│  └───────────┴─────────────────────────┘   │
└─────────────────────────────────────────────┘
                    │
┌─────────────────────────────────────────────┐
│     Plugin System                           │
│  ┌──────────────┬───────────────────────┐  │
│  │ ExamplePlugin│  PrometheusPlugin     │  │
│  └──────────────┴───────────────────────┘  │
└─────────────────────────────────────────────┘
```

### Server Architecture

#### Dual-Server Model
- **Auth Server** (port 11002): User authentication and account management
- **Game Server** (port 11003): Live game world management
- **Single Mode**: Combined auth+game for simplified deployment

#### Connection Flow
```
1. Client → Auth Server (port 11002)
2. Client authenticates, receives session token
3. Client → Game Server (port 11003) with token
4. Game Server validates token
5. Game Server manages all game interactions
```

### Game Server Performance
- **Tick Rate:** 100Hz (10ms per frame)
- **Game Loop:** Fixed timestep with spiral of death prevention
- **Spatial Partitioning:** QuadTree for efficient area queries
- **State Management:** Redis-based distributed caching

## Feature Status

### ✅ Fully Implemented (v0.1 - The Baseline)

#### Core Infrastructure
- Authentication & account system
- Database layer with multiple providers
- Network packet handling system
- Configuration management
- Plugin system

#### Player Systems
- Character creation, deletion, selection (4 slots)
- Movement system with position tracking
- Map transitions
- **Complete inventory system** (equipment, items, pickup/drop/move/use)
- Equipment management with slot system
- Level system with experience gain
- Stats system (HP, SP, STR, DEX, INT, CON)
- Gold/currency management

#### Combat & AI
- Basic melee combat (player attacks monsters)
- Monster AI behaviors:
  - SimpleBehaviour: Basic, ranged, aggressive, coward
  - StoneBehaviour: Metin stone spawning mobs on damage
- Damage calculation system
- Dynamic monster spawning with respawn

#### Items & Economy
- Item management with proto loading
- Drop system with configurable rates
- Ground items with pickup functionality
- NPC shops (buy/sell items)

#### Guild System (Basic)
- Guild creation and management
- Member ranks and permissions
- Guild news/notice board
- Guild experience investment system

#### Other Systems
- Chat system
- 57+ admin/GM commands
- Map loading and world management
- Skill books and skill learning
- QuickBar system

### ⚠️ Incomplete Features

#### Skills System (Major Gap)
- ❌ Active skill usage/execution (not implemented)
- ❌ Skill effects and damage
- ❌ Buff/debuff system
- ❌ Skill cooldowns

#### Quest System (WIP - See Quest System Documentation)
- ⚠️ Basic C# framework exists
- ❌ Declarative quest system (planned - see Quest System docs)
- ❌ Quest state persistence
- ❌ Lua scripting integration

#### Social Features (v0.4 - Not Started)
- ❌ Party system
- ❌ Player-to-player trading
- ❌ Duel system
- ❌ Warehouse system
- ❌ Private messaging/whispers
- ❌ Friend list
- ❌ Mail system

#### Advanced Systems
- ❌ Horse/mount system (v0.3 milestone)
- ❌ Polymorph/transformation (v0.5 milestone)
- ❌ Emote system
- ❌ Dungeons/instances
- ❌ PvP systems
- ❌ Guild wars
- ❌ Alignment system
- ❌ Premium/VIP system
- ❌ Multi-channel support

## Key Components Deep Dive

### Core.Networking

Custom networking layer providing:
- TCP/IP connection handling
- Connection pooling
- Phase-based connection management (LOGIN, GAME)
- Packet serialization/deserialization
- XTEA encryption support
- LZO compression support

**Example Packet Definition:**
```csharp
[Packet(0x2D, EDirection.OUTGOING)]
public partial class QuestScript
{
    public ushort PacketSize => (ushort) Source.Length;
    public byte Skin { get; set; }
    public ushort SourceSize { get; set; }
    public string Source { get; set; } = "";
}
```

### Game Server Loop

The game server runs at 100Hz with a fixed timestep:

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    const double targetTicksPerFrame = 100.0; // 100Hz
    const double targetDeltaTime = 1000.0 / targetTicksPerFrame; // 10ms

    while (!stoppingToken.IsCancellationRequested)
    {
        var tickStart = Stopwatch.GetTimestamp();

        await _world.UpdateAsync(elapsedMilliseconds);

        // Calculate sleep time to maintain 100Hz
        var elapsed = GetElapsedMilliseconds(tickStart);
        if (elapsed < targetDeltaTime)
        {
            await Task.Delay((int)(targetDeltaTime - elapsed), stoppingToken);
        }
    }
}
```

### World Management

The world is managed through a spatial partitioning system:

```csharp
public class Map
{
    private readonly QuadTree _quadTree; // Spatial partitioning
    private readonly Dictionary<uint, IEntity> _entities;

    public async Task UpdateAsync(double elapsedMilliseconds)
    {
        // Update all entities (monsters, NPCs, players)
        foreach (var entity in _entities.Values)
        {
            await entity.UpdateAsync(elapsedMilliseconds);
        }
    }

    public IEnumerable<IEntity> GetNearbyEntities(Vector2 position, float radius)
    {
        return _quadTree.Query(position, radius);
    }
}
```

### Data Loading Patterns

All game data uses the `ILoadable` pattern:

```csharp
public interface ILoadable
{
    Task LoadAsync(CancellationToken token = default);
}
```

**Examples:**
- **ItemManager**: Loads binary encrypted item_proto files (LZO+XTEA)
- **SkillManager**: Parses tab-separated skilltable.txt
- **MonsterManager**: Loads binary encrypted mob_proto files
- **DropProvider**: Parses group-based text files (common_drop_item.txt, etc.)
- **NpcShopProvider**: Loads JSON shops.json

All loaders:
- Auto-discovered via assembly scanning
- Registered as singletons
- Loaded in parallel at startup
- Store data in `ImmutableArray<T>` for thread-safety

### Plugin System

Plugins are discovered automatically from the `plugins/` directory:

```csharp
var pluginCatalog = new FolderPluginCatalog("plugins", cfg =>
{
    cfg.Implements<IServiceCollectionPlugin>();
    cfg.Implements<IGamePacketHandler>();
    cfg.Implements<IGameTickListener>();
    // ... other plugin types
});
```

**Available Plugin Types:**
- `IServiceCollectionPlugin` - Modify DI container
- `IGamePacketHandler` - Custom packet handlers
- `IConnectionLifetimeListener` - Connection lifecycle events
- `IGameEntityLifetimeListener` - Entity lifecycle events
- `IGameTickListener` - Game tick events

### Dependency Injection

The project uses keyed services for multi-mode support:

```csharp
// Register services for both Auth and Game modes
services.AddKeyedSingleton<IConnectionHandler, AuthConnectionHandler>("auth");
services.AddKeyedSingleton<IConnectionHandler, GameConnectionHandler>("game");

// Retrieve keyed service
var handler = serviceProvider.GetKeyedService<IConnectionHandler>("game");
```

## Development Setup

### Prerequisites
- .NET SDK 9.0
- Docker & Docker Compose
- JetBrains Rider (recommended, free since 2024) or Visual Studio

### Quick Start

```bash
# Clone repository
git clone https://github.com/MeikelLP/quantum-core-x.git
cd quantum-core-x

# Start infrastructure (database, Redis)
docker-compose up -d

# Build solution
dotnet build

# Run migrations
cd src/Executables/Game
dotnet ef database update

# Run servers
cd src/Executables/Auth
dotnet run  # Auth server on port 11002

cd src/Executables/Game
dotnet run  # Game server on port 11003
```

### Docker Compose Services
- **MariaDB**: Persistent data storage (port 3306)
- **Redis**: Distributed cache and live state (port 6379)

## File Structure

```
quantum-core-x/
├── src/
│   ├── Core/                         # Core framework
│   │   ├── Packets/                  # Packet definitions
│   │   ├── Events/                   # Event system
│   │   ├── Utils/                    # Utilities (QuadTree, Grid, etc.)
│   │   └── Constants/                # Game constants
│   │
│   ├── Core.Networking/              # Networking layer
│   │   ├── Connection.cs             # TCP connection handling
│   │   ├── ServerBase.cs             # Base server class
│   │   └── Encryption/               # XTEA, LZO
│   │
│   ├── Core.Networking.Generators/   # Source generators
│   │
│   ├── CorePluginAPI/                # Plugin interfaces
│   │   ├── IPlayerEntity.cs
│   │   ├── IItemManager.cs
│   │   └── ... (40+ interfaces)
│   │
│   ├── Libraries/
│   │   ├── Game.Server/              # Game server logic
│   │   │   ├── GameServer.cs         # Main game loop
│   │   │   ├── World/                # World management
│   │   │   ├── PacketHandlers/       # Packet handlers
│   │   │   ├── Services/             # Game services
│   │   │   ├── Quest/                # Quest system
│   │   │   ├── Skills/               # Skill system
│   │   │   └── Commands/             # Admin commands
│   │   │
│   │   ├── Auth.Server/              # Auth server logic
│   │   └── Game.Commands/            # Command system
│   │
│   ├── Data/
│   │   ├── Game.Persistence/         # Game database
│   │   ├── Auth.Persistence/         # Auth database
│   │   ├── Game.Caching/             # In-memory caching
│   │   └── Core.Caching/             # Redis caching
│   │
│   ├── Executables/
│   │   ├── Game/                     # Game server entry point
│   │   ├── Auth/                     # Auth server entry point
│   │   └── Single/                   # Combined mode
│   │
│   ├── Plugins/
│   │   ├── ExamplePlugin/
│   │   └── PrometheusPlugin/
│   │
│   └── Tests/
│       ├── Core.Tests/
│       ├── Game.Tests/
│       └── ... (7 test projects)
│
├── docs/                             # Documentation (Docusaurus)
├── docker-compose.yml
└── QuantumCore.sln
```

## Data Directory Structure

The `data/` directory (typically in the game server working directory) contains:

```
data/
├── item_proto              # Binary encrypted item definitions
├── mob_proto               # Binary encrypted monster definitions
├── skilltable.txt          # Tab-separated skill definitions
├── shops.json              # NPC shop configurations
├── common_drop_item.txt    # Common drop rates
├── mob_drop_item.txt       # Monster-specific drops
├── atlasinfo.txt           # Map information
└── quests/                 # Quest definitions (future)
    └── *.json
```

## Common Design Patterns

### ILoadable Pattern
All data loaders implement `ILoadable` for consistent initialization:

```csharp
public class ItemManager : ILoadable
{
    public async Task LoadAsync(CancellationToken token = default)
    {
        // Load items from file
    }
}

// Auto-discovered and loaded at startup
await Task.WhenAll(
    serviceProvider.GetServices<ILoadable>()
        .Select(x => x.LoadAsync(stoppingToken))
);
```

### Immutable Storage
Game data is stored in immutable collections for thread-safety:

```csharp
public ImmutableArray<ItemData> Items { get; private set; }
```

### Service Scoping
Connection-specific state uses scoped services:

```csharp
services.AddScoped<IPlayerEntity, PlayerEntity>();
```

### Event System
Game events use a custom event system:

```csharp
GameEventManager.RegisterNpcClickEvent(
    "Quest Name",
    npcId: 20354,
    callback: async (player) => { /* handler */ },
    condition: player => player.Level >= 5
);
```

## Performance Considerations

### Optimizations
- Fixed 100Hz tick rate with spiral of death prevention
- QuadTree spatial partitioning for O(log n) entity queries
- Redis for distributed state (horizontal scaling)
- Connection pooling
- ImmutableArray for zero-copy reads
- Source generators to reduce reflection overhead

### Monitoring
- Prometheus metrics via plugin
- Serilog structured logging
- Performance profiling with BenchmarkDotNet

## Roadmap

### v0.2 - Quests (Current)
- Declarative quest system (JSON-based)
- Quest state persistence
- Skill group reset
- Alignment system

### v0.3 - Metins, Horses, Mounts
- Mount system with customization
- Horse skills and mechanics
- Mount inventory

### v0.4 - Social Update
- Party system
- Duels
- Player-to-player trading
- Advanced guild features
- Warehouse system

### v0.5 - Poly, Emotes, Passives
- Polymorph/transformation system
- Emote system
- Passive skills

### v1.0 - Production
- i18n (internationalization)
- Dungeons
- Multi-channel support
- Feature flags
- Production stability

## Community & Contributing

- **GitHub**: [github.com/MeikelLP/quantum-core-x](https://github.com/MeikelLP/quantum-core-x)
- **Discord**: Active community (link in repo)
- **Contributing**: See CONTRIBUTING.md
- **Code of Conduct**: See CODE_OF_CONDUCT.md
- **License**: Mozilla Public License 2.0

## Technical Debt & Known Issues

Current limitations noted in codebase:
- 61 files with TODO/FIXME markers
- 14 files with NotImplementedException throws
- Multiple synchronous .Wait() calls that should be async
- Security concerns in authentication layer
- InMemoryRedisStore lacks persistence
- Help command not implemented
- RemoteMap is stub implementation (multi-server support)

## Resources

### Official Documentation
- User Setup Guide
- Developer Setup Guide
- Configuration Documentation
- Packet Format Documentation
- Command Reference
- Migration Guides

### External References
- [Metin2 Wiki](https://en-wiki.metin2.gameforge.com/)
- [.NET 9.0 Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [Redis Documentation](https://redis.io/docs/)

## Summary

Quantum Core X is a sophisticated, production-quality MMORPG server implementation demonstrating modern C# best practices and architectural patterns. While currently at ~30-40% feature completeness, it provides a solid foundation for:

- Learning game server architecture
- Running private Metin2 servers
- Contributing to open-source game development
- Experimenting with MMO systems design

The codebase emphasizes:
- **Maintainability**: Clean architecture, strong typing, comprehensive tests
- **Performance**: 100Hz tick rate, spatial partitioning, distributed caching
- **Extensibility**: Plugin system, dependency injection, event-driven design
- **Modernization**: Latest C# features, .NET 9.0, modern tooling

For detailed information about specific systems, see the respective documentation sections.
