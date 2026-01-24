using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Quest;

[Quest]
public class TestQuest : Quest
{
    private readonly IQuestEventManager _eventManager;

    public TestQuest(QuestState state, IPlayerEntity player, IItemManager itemManager, IQuestEventManager eventManager)
        : base(state, player, itemManager)
    {
        _eventManager = eventManager;
    }

    public override void Init()
    {
        // Register events using the injected event manager
        _eventManager.RegisterNpcClickEvent("Test Quest", 20354, Test, p => p.Vid == Player.Vid);
        _eventManager.RegisterNpcGiveEvent("Test Quest", 20016, (p, item) =>
        {
            TestGive(p, item);
            return Task.CompletedTask;
        }, (p, _) => p.Vid == Player.Vid);
    }

    private async Task Test(IPlayerEntity player)
    {
        Text("Hello World from QuantumCore!");
        Text("This is using the current work in progress");
        Text("Quest API.");
        Next();

        Text("This is the second page showing how to easily");
        Text("using await to wait for user response");
        var choice = await Choice(false, "1st option", "2nd option");

        Text($"You've chosen: {choice}");
        Done();
    }

    private void TestGive(IPlayerEntity player, ItemInstance item)
    {
        var proto = ItemManager.GetItem(item.ItemId);

        if (proto is null)
        {
            Text("Failure: Could not find item.");
        }
        else
        {
            Text($"Thanks for giving me the item {proto.TranslatedName}.");
            player.Inventory.PlaceItem(item);
        }

        Done();
    }
}
