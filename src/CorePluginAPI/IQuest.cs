using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;

namespace QuantumCore.API;

/// <summary>
/// Interface for quest implementations.
/// </summary>
public interface IQuest
{
    /// <summary>
    /// The persistent state of this quest.
    /// </summary>
    QuestState State { get; }

    /// <summary>
    /// The player this quest belongs to.
    /// </summary>
    IPlayerEntity Player { get; }

    /// <summary>
    /// Initializes the quest, typically used to register event handlers.
    /// </summary>
    void Init();

    /// <summary>
    /// Called when the player answers a quest dialog (choice selection or next button).
    /// </summary>
    void Answer(byte answer);

    /// <summary>
    /// Called when the player provides text input to a quest dialog.
    /// </summary>
    void AnswerInput(string input);

    /// <summary>
    /// Called when the player answers a confirmation dialog.
    /// </summary>
    void AnswerConfirm(bool confirmed);

    /// <summary>
    /// Called when the player clicks a quest button.
    /// </summary>
    void OnButton(uint index);

    /// <summary>
    /// Called when the player selects an item for a quest.
    /// </summary>
    void OnSelectItem(uint itemId);

    /// <summary>
    /// Called when a quest timer triggers.
    /// </summary>
    Task OnTimer(string timerName);

    /// <summary>
    /// Sends the current quest info to the player's UI.
    /// </summary>
    void SendQuestInfo();

    /// <summary>
    /// Marks the state as dirty (needs saving).
    /// </summary>
    void MarkDirty();
}
