using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Persistence.Entities;
using QuantumCore.Game.Quest.Actions;
using QuantumCore.Game.World;
using Xunit;

namespace Game.Tests.Quest.Actions;

public class GiveItemActionTests
{
    private QuestActionContext CreateContext(IPlayerEntity? player = null, QuestState? state = null, IItemManager? itemManager = null)
    {
        var services = Substitute.For<IServiceProvider>();
        if (itemManager != null)
        {
            services.GetService(typeof(IItemManager)).Returns(itemManager);
        }

        return new QuestActionContext
        {
            Player = player ?? Substitute.For<IPlayerEntity>(),
            State = state ?? new QuestState { QuestId = "test_quest", PlayerId = Guid.NewGuid() },
            Services = services,
            Logger = NullLogger.Instance
        };
    }

    private PlayerData CreatePlayerData()
    {
        return new PlayerData
        {
            Id = 1,
            Name = "TestPlayer",
            Level = 1
        };
    }

    [Fact]
    public async Task testExecuteAsync_givenValidItemId_shouldCallPlaceItem()
    {
        var player = Substitute.For<IPlayerEntity>();
        player.Player.Returns(CreatePlayerData());
        var inventory = Substitute.For<IInventory>();
        player.Inventory.Returns(inventory);

        var itemManager = Substitute.For<IItemManager>();
        var itemProto = new ItemData { Id = 50001, Name = "Test Item" };
        var itemInstance = Substitute.For<ItemInstance>();
        itemManager.GetItem(50001).Returns(itemProto);
        itemManager.CreateItem(itemProto, 10).Returns(itemInstance);

        var context = CreateContext(player: player, itemManager: itemManager);
        var action = new GiveItemAction { ItemId = 50001, Count = 10 };

        await action.ExecuteAsync(context);

        await inventory.Received(1).PlaceItem(itemInstance);
    }

    [Fact]
    public async Task testExecuteAsync_givenValidItemId_shouldCallSendInventory()
    {
        var player = Substitute.For<IPlayerEntity>();
        player.Player.Returns(CreatePlayerData());
        var inventory = Substitute.For<IInventory>();
        player.Inventory.Returns(inventory);

        var itemManager = Substitute.For<IItemManager>();
        var itemProto = new ItemData { Id = 50001, Name = "Test Item" };
        var itemInstance = Substitute.For<ItemInstance>();
        itemManager.GetItem(50001).Returns(itemProto);
        itemManager.CreateItem(itemProto, 5).Returns(itemInstance);

        var context = CreateContext(player: player, itemManager: itemManager);
        var action = new GiveItemAction { ItemId = 50001, Count = 5 };

        await action.ExecuteAsync(context);

        player.Received(1).SendInventory();
    }

    [Fact]
    public async Task testExecuteAsync_givenDefaultCount_shouldGiveOneItem()
    {
        var player = Substitute.For<IPlayerEntity>();
        player.Player.Returns(CreatePlayerData());
        var inventory = Substitute.For<IInventory>();
        player.Inventory.Returns(inventory);

        var itemManager = Substitute.For<IItemManager>();
        var itemProto = new ItemData { Id = 11001, Name = "Sword" };
        var itemInstance = Substitute.For<ItemInstance>();
        itemManager.GetItem(11001).Returns(itemProto);
        itemManager.CreateItem(itemProto, (byte)1).Returns(itemInstance);

        var context = CreateContext(player: player, itemManager: itemManager);
        var action = new GiveItemAction { ItemId = 11001 };

        await action.ExecuteAsync(context);

        itemManager.Received(1).CreateItem(itemProto, (byte)1);
    }

    [Fact]
    public async Task testExecuteAsync_givenInvalidItemId_shouldNotCallPlaceItem()
    {
        var player = Substitute.For<IPlayerEntity>();
        player.Player.Returns(CreatePlayerData());
        var inventory = Substitute.For<IInventory>();
        player.Inventory.Returns(inventory);

        var itemManager = Substitute.For<IItemManager>();
        itemManager.GetItem(99999).Returns((ItemData?)null);

        var context = CreateContext(player: player, itemManager: itemManager);
        var action = new GiveItemAction { ItemId = 99999, Count = 1 };

        await action.ExecuteAsync(context);

        await inventory.DidNotReceive().PlaceItem(Arg.Any<ItemInstance>());
    }

    [Fact]
    public async Task testExecuteAsync_givenItemCreationFails_shouldNotCallPlaceItem()
    {
        var player = Substitute.For<IPlayerEntity>();
        player.Player.Returns(CreatePlayerData());
        var inventory = Substitute.For<IInventory>();
        player.Inventory.Returns(inventory);

        var itemManager = Substitute.For<IItemManager>();
        var itemProto = new ItemData { Id = 50001, Name = "Test Item" };
        itemManager.GetItem(50001).Returns(itemProto);
        itemManager.CreateItem(itemProto, 1).Returns((ItemInstance?)null);

        var context = CreateContext(player: player, itemManager: itemManager);
        var action = new GiveItemAction { ItemId = 50001, Count = 1 };

        await action.ExecuteAsync(context);

        await inventory.DidNotReceive().PlaceItem(Arg.Any<ItemInstance>());
    }
}
