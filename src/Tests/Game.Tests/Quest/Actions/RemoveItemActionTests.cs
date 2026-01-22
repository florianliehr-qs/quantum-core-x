using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Quest.Actions;
using Xunit;

namespace Game.Tests.Quest.Actions;

public class RemoveItemActionTests
{
    private QuestActionContext CreateContext(IPlayerEntity? player = null, QuestState? state = null)
    {
        return new QuestActionContext
        {
            Player = player ?? Substitute.For<IPlayerEntity>(),
            State = state ?? new QuestState { QuestId = "test_quest", PlayerId = Guid.NewGuid() },
            Services = Substitute.For<IServiceProvider>(),
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
    public async Task testExecuteAsync_givenItemInInventory_shouldDecrementCount()
    {
        var player = Substitute.For<IPlayerEntity>();
        player.Player.Returns(CreatePlayerData());
        var inventory = Substitute.For<IInventory>();
        var itemInstance = new ItemInstance { ItemId = 50001, Count = 10 };
        inventory.Items.Returns(_ => new List<ItemInstance> { itemInstance }.AsReadOnly());
        player.Inventory.Returns(inventory);

        var context = CreateContext(player: player);
        var action = new RemoveItemAction { ItemId = 50001, Count = 5 };

        await action.ExecuteAsync(context);

        itemInstance.Count.Should().Be(5);
    }

    [Fact]
    public async Task testExecuteAsync_givenItemInInventory_shouldCallSendInventory()
    {
        var player = Substitute.For<IPlayerEntity>();
        player.Player.Returns(CreatePlayerData());
        var inventory = Substitute.For<IInventory>();
        var itemInstance = new ItemInstance { ItemId = 50001, Count = 10 };
        inventory.Items.Returns(_ => new List<ItemInstance> { itemInstance }.AsReadOnly());
        player.Inventory.Returns(inventory);

        var context = CreateContext(player: player);
        var action = new RemoveItemAction { ItemId = 50001, Count = 3 };

        await action.ExecuteAsync(context);

        player.Received(1).SendInventory();
    }

    [Fact]
    public async Task testExecuteAsync_givenMultipleStacksOfItem_shouldRemoveFromMultipleStacks()
    {
        var player = Substitute.For<IPlayerEntity>();
        player.Player.Returns(CreatePlayerData());
        var inventory = Substitute.For<IInventory>();

        var item1 = new ItemInstance { ItemId = 50001, Count = 5 };
        var item2 = new ItemInstance { ItemId = 50001, Count = 10 };

        inventory.Items.Returns(_ => new List<ItemInstance> { item1, item2 }.AsReadOnly());
        player.Inventory.Returns(inventory);

        var context = CreateContext(player: player);
        var action = new RemoveItemAction { ItemId = 50001, Count = 12 };

        await action.ExecuteAsync(context);

        inventory.Received(1).RemoveItem(item1);
        item2.Count.Should().Be(3);
    }

    [Fact]
    public async Task testExecuteAsync_givenDefaultCount_shouldRemoveOneItem()
    {
        var player = Substitute.For<IPlayerEntity>();
        player.Player.Returns(CreatePlayerData());
        var inventory = Substitute.For<IInventory>();
        var itemInstance = new ItemInstance { ItemId = 11001, Count = 3 };
        inventory.Items.Returns(_ => new List<ItemInstance> { itemInstance }.AsReadOnly());
        player.Inventory.Returns(inventory);

        var context = CreateContext(player: player);
        var action = new RemoveItemAction { ItemId = 11001 };

        await action.ExecuteAsync(context);

        itemInstance.Count.Should().Be(2);
    }

    [Fact]
    public async Task testExecuteAsync_givenItemNotInInventory_shouldStillCallSendInventory()
    {
        var player = Substitute.For<IPlayerEntity>();
        player.Player.Returns(CreatePlayerData());
        var inventory = Substitute.For<IInventory>();
        inventory.Items.Returns(_ => new List<ItemInstance>().AsReadOnly());
        player.Inventory.Returns(inventory);

        var context = CreateContext(player: player);
        var action = new RemoveItemAction { ItemId = 99999, Count = 5 };

        await action.ExecuteAsync(context);

        player.Received(1).SendInventory();
    }
}
