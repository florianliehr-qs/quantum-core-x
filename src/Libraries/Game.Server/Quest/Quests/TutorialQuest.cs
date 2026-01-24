using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Quest.Quests;

/// <summary>
/// Tutorial quest demonstrating the quest system features:
/// - NPC click trigger
/// - Monster kill tracking with flags
/// - Timer-based events
/// - Multiple dialog choices
/// - Item and gold rewards
/// - Quest state persistence
/// </summary>
[Quest]
public class TutorialQuest : Quest
{
    private readonly IQuestEventManager _eventManager;

    // Quest constants
    private const uint TUTORIAL_NPC = 20355;  // NPC vnum for tutorial giver
    private const uint WOLF_MOB = 101;        // Wolf mob vnum for kill tracking
    private const int REQUIRED_KILLS = 5;     // Number of wolves to kill
    private const uint REWARD_ITEM = 189;       // Reward item vnum (0 = no item reward)
    private const int REWARD_GOLD = 10000;    // Gold reward
    private const int REWARD_EXP = 5000;      // Experience reward

    // Flag names
    private const string FLAG_KILLS = "wolf_kills";
    private const string FLAG_ACCEPTED = "accepted";

    public TutorialQuest(QuestState state, IPlayerEntity player, IItemManager itemManager, IQuestEventManager eventManager)
        : base(state, player, itemManager)
    {
        _eventManager = eventManager;
    }

    public override void Init()
    {
        // Register NPC click event for this player only
        _eventManager.RegisterNpcClickEvent(
            "Tutorial Quest",
            TUTORIAL_NPC,
            OnTutorialNpcClick,
            p => p.Vid == Player.Vid
        );

        // Register kill event for wolves (only when quest is active)
        _eventManager.RegisterKillEvent(
            "Tutorial Quest - Wolf Kill",
            WOLF_MOB,
            OnWolfKill,
            (p, _) => p.Vid == Player.Vid && IsStarted && !IsCompleted
        );

        // If quest was already started, restore the UI state
        if (IsStarted && !IsCompleted)
        {
            SetTitle("Wolf Hunt");
            UpdateKillCounter();
            // Note: SendQuestInfo is called by QuestManager after Init
        }
    }

    private async Task OnTutorialNpcClick(IPlayerEntity player)
    {
        if (!IsStarted)
        {
            await ShowIntroDialog();
        }
        else if (!IsCompleted)
        {
            await ShowProgressDialog();
        }
        else
        {
            await ShowCompletedDialog();
        }
    }

    private async Task ShowIntroDialog()
    {
        SetSkin(API.Game.Types.Quest.QuestSkin.NORMAL);

        Text($"Greetings, {PlayerName}!");
        Text("I am the Tutorial Master.");
        Text("Would you like to learn about the quest system?");
        await WaitNext();

        Text("I have a simple task for you.");
        Text($"Hunt {REQUIRED_KILLS} wolves in the nearby forest.");
        Text("Return to me when you are done.");
        var choice = await Choice(false, "Accept Quest", "Decline");

        if (choice == 0) // Accept (client sends 0-indexed choice)
        {
            AcceptQuest();
        }
        else
        {
            Text("Come back when you're ready for adventure!");
            Done();
        }
    }

    private void AcceptQuest()
    {
        // Set up quest UI first (before SendQuestInfo is called)
        SetTitle("Wolf Hunt");

        // Start the quest (calls SendQuestInfo internally)
        StartQuest();
        SetFlag(FLAG_ACCEPTED, 1);
        SetFlag(FLAG_KILLS, 0);

        Text("Excellent! The hunt begins!");
        Text($"Kill {REQUIRED_KILLS} wolves and return to me.");
        Done();
    }

    private async Task ShowProgressDialog()
    {
        var kills = GetFlag(FLAG_KILLS);

        if (kills >= REQUIRED_KILLS)
        {
            await CompleteQuestDialog();
        }
        else
        {
            Text($"You have killed {kills} out of {REQUIRED_KILLS} wolves.");
            Text("");
            Text("Keep hunting! The wolves are in the forest to the east.");

            var choice = await Choice(false, "Show me on map", "I'll find them");

            if (choice == 0) // Show me on map (client sends 0-indexed choice)
            {
                // Add a map signal pointing to wolf spawn area
                AddMapSignal(958000, 273000);
                Text("I've marked the location on your map.");
            }
            else
            {
                Text("Good luck, hunter!");
            }

            Done();
        }
    }

    private async Task CompleteQuestDialog()
    {
        Text("Excellent work, hunter!");
        Text($"You have slain all {REQUIRED_KILLS} wolves.");
        Text("");
        Text("Here is your reward:");
        SetColor256(0, 255, 0);
        Text($"- {REWARD_EXP} Experience");
        Text($"- {REWARD_GOLD} Gold");
        await WaitNext();

        // Give rewards
        GiveExp(REWARD_EXP);
        GiveGold(REWARD_GOLD);

        // Give item reward if configured
        if (REWARD_ITEM != 0)
        {
            if (!GiveItem(REWARD_ITEM, 1))
            {
                Text("Your inventory is full! Make space and talk to me again.");
                Done();
                return;
            }
        }

        // Complete the quest
        CompleteQuest();
        ClearMapSignal();

        Text("You have completed the Tutorial Quest!");
        Text("");
        Text("You are now ready for greater challenges.");
        Done();

        SendQuestInfo();
    }

    private async Task ShowCompletedDialog()
    {
        Text("Thank you for completing my task, brave adventurer.");
        Text("");
        Text("Perhaps we shall meet again when new challenges arise.");
        Done();
    }

    private async Task OnWolfKill(IPlayerEntity player, uint mobVnum)
    {
        // Increment kill counter
        var kills = IncrementFlag(FLAG_KILLS);

        // Update the UI counter
        UpdateKillCounter();
        SendQuestInfo();

        // Notify player
        player.SendChatInfo($"Wolf Hunt: {kills}/{REQUIRED_KILLS}");

        if (kills >= REQUIRED_KILLS)
        {
            player.SendChatInfo("Quest objective complete! Return to the Tutorial Master.");
            ClearTimer("hint_timer");
        }
    }

    private void UpdateKillCounter()
    {
        var kills = GetFlag(FLAG_KILLS);
        SetCounter($"Wolves: {kills}/{REQUIRED_KILLS}", kills);
    }

    public override async Task OnTimer(string timerName)
    {
        if (timerName == "hint_timer" && IsStarted && !IsCompleted)
        {
            var kills = GetFlag(FLAG_KILLS);
            if (kills < REQUIRED_KILLS)
            {
                Player.SendChatInfo("Hint: The wolves can be found in the forest to the east!");
                // Set another hint timer
                SetTimer("hint_timer", 120);
            }
        }
    }

    protected override Task OnQuestButtonClick()
    {
        if (!IsStarted)
        {
            Text("Talk to the Tutorial Master to begin this quest.");
            Done();
        }
        else if (!IsCompleted)
        {
            var kills = GetFlag(FLAG_KILLS);
            SetSkin(API.Game.Types.Quest.QuestSkin.NORMAL);
            Text("Wolf Hunt - Progress");
            Text("");
            Text($"Wolves killed: {kills} / {REQUIRED_KILLS}");
            Text("");
            if (kills >= REQUIRED_KILLS)
            {
                SetColor256(0, 255, 0);
                Text("Objective complete!");
                Text("Return to the Tutorial Master for your reward.");
            }
            else
            {
                Text($"You need to kill {REQUIRED_KILLS - kills} more wolves.");
                Text("The wolves can be found in the forest to the east.");
            }
            Done();
        }
        else
        {
            Text("Quest Completed!");
            Text("");
            Text("You have already completed the Wolf Hunt.");
            Done();
        }
        return Task.CompletedTask;
    }
}
