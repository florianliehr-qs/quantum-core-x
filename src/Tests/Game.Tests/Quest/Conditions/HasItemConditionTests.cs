using AwesomeAssertions;
using NSubstitute;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Quest.Conditions;
using Xunit;

namespace Game.Tests.Quest.Conditions;

public class HasItemConditionTests
{
    private QuestConditionContext CreateContext(IPlayerEntity? player = null, QuestState? state = null)
    {
        return new QuestConditionContext
        {
            Player = player ?? Substitute.For<IPlayerEntity>(),
            State = state ?? new QuestState { QuestId = "test_quest", PlayerId = Guid.NewGuid() },
            Services = Substitute.For<IServiceProvider>()
        };
    }

    [Fact]
    public void testEvaluate_givenPlayerHasExactCount_shouldReturnTrue()
    {
        var player = Substitute.For<IPlayerEntity>();
        var inventory = Substitute.For<IInventory>();
        var item = new ItemInstance { ItemId = 50001, Count = 10 };
        inventory.Items.Returns(_ => new List<ItemInstance> { item }.AsReadOnly());
        player.Inventory.Returns(inventory);

        var context = CreateContext(player: player);
        var condition = new HasItemCondition { ItemId = 50001, Count = 10 };

        var result = condition.Evaluate(context);

        result.Should().BeTrue();
    }

    [Fact]
    public void testEvaluate_givenPlayerHasMoreThanRequired_shouldReturnTrue()
    {
        var player = Substitute.For<IPlayerEntity>();
        var inventory = Substitute.For<IInventory>();
        var item = new ItemInstance { ItemId = 50001, Count = 15 };
        inventory.Items.Returns(_ => new List<ItemInstance> { item }.AsReadOnly());
        player.Inventory.Returns(inventory);

        var context = CreateContext(player: player);
        var condition = new HasItemCondition { ItemId = 50001, Count = 10 };

        var result = condition.Evaluate(context);

        result.Should().BeTrue();
    }

    [Fact]
    public void testEvaluate_givenPlayerHasLessThanRequired_shouldReturnFalse()
    {
        var player = Substitute.For<IPlayerEntity>();
        var inventory = Substitute.For<IInventory>();
        var item = new ItemInstance { ItemId = 50001, Count = 5 };
        inventory.Items.Returns(_ => new List<ItemInstance> { item }.AsReadOnly());
        player.Inventory.Returns(inventory);

        var context = CreateContext(player: player);
        var condition = new HasItemCondition { ItemId = 50001, Count = 10 };

        var result = condition.Evaluate(context);

        result.Should().BeFalse();
    }

    [Fact]
    public void testEvaluate_givenPlayerDoesNotHaveItem_shouldReturnFalse()
    {
        var player = Substitute.For<IPlayerEntity>();
        var inventory = Substitute.For<IInventory>();
        inventory.Items.Returns(_ => new List<ItemInstance>().AsReadOnly());
        player.Inventory.Returns(inventory);

        var context = CreateContext(player: player);
        var condition = new HasItemCondition { ItemId = 99999, Count = 1 };

        var result = condition.Evaluate(context);

        result.Should().BeFalse();
    }

    [Fact]
    public void testEvaluate_givenMultipleStacksOfItem_shouldSumCounts()
    {
        var player = Substitute.For<IPlayerEntity>();
        var inventory = Substitute.For<IInventory>();

        var item1 = new ItemInstance { ItemId = 50001, Count = 5 };
        var item2 = new ItemInstance { ItemId = 50001, Count = 7 };

        inventory.Items.Returns(_ => new List<ItemInstance> { item1, item2 }.AsReadOnly());
        player.Inventory.Returns(inventory);

        var context = CreateContext(player: player);
        var condition = new HasItemCondition { ItemId = 50001, Count = 10 };

        var result = condition.Evaluate(context);

        result.Should().BeTrue();
    }

    [Fact]
    public void testEvaluate_givenDefaultCount_shouldCheckForOneItem()
    {
        var player = Substitute.For<IPlayerEntity>();
        var inventory = Substitute.For<IInventory>();
        var item = new ItemInstance { ItemId = 11001, Count = 1 };
        inventory.Items.Returns(_ => new List<ItemInstance> { item }.AsReadOnly());
        player.Inventory.Returns(inventory);

        var context = CreateContext(player: player);
        var condition = new HasItemCondition { ItemId = 11001 };

        var result = condition.Evaluate(context);

        result.Should().BeTrue();
    }
}
