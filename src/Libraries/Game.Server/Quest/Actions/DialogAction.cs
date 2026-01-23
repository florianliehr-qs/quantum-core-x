using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumCore.Game.Quest.Factories;

namespace QuantumCore.Game.Quest.Actions;

/// <summary>
/// Displays quest dialog with text pages and player choices.
/// Supports multi-page dialogs with branching based on player selection.
/// </summary>
public class DialogAction : QuestActionBase
{
    /// <summary>
    /// Array of dialog pages to show sequentially.
    /// </summary>
    [JsonPropertyName("pages")]
    public required List<DialogPage> Pages { get; init; }

    public override async Task ExecuteAsync(QuestActionContext context, CancellationToken cancellationToken = default)
    {
        if (context.Quest == null)
        {
            context.Logger.LogError("DialogAction requires Quest instance in context");
            return;
        }

        var quest = context.Quest as DeclarativeQuest;
        if (quest == null)
        {
            context.Logger.LogError("DialogAction only works with DeclarativeQuest instances");
            return;
        }

        foreach (var page in Pages)
        {
            await ShowDialogPage(page, quest, context, cancellationToken);
        }
    }

    private async Task ShowDialogPage(DialogPage page, DeclarativeQuest quest, QuestActionContext context, CancellationToken cancellationToken)
    {
        // Add text to dialog
        if (!string.IsNullOrWhiteSpace(page.Text))
        {
            quest.AddDialogText(page.Text);
        }

        // Handle Next button
        if (page.Next)
        {
            await quest.ShowNext();
            return;
        }

        // Handle Choices
        if (page.Choices != null && page.Choices.Count > 0)
        {
            var choiceTexts = page.Choices.Select(c => c.Text).ToArray();
            var hasCloseAction = page.Choices.Any(c => c.Action == "close");

            var selectedIndex = await quest.ShowChoice(choiceTexts, done: hasCloseAction);

            if (selectedIndex < page.Choices.Count)
            {
                var selectedChoice = page.Choices[selectedIndex];
                await HandleChoiceActions(selectedChoice, context, cancellationToken);
            }
            else
            {
                context.Logger.LogWarning("DialogAction: Player selected invalid choice index {Index}", selectedIndex);
            }

            return;
        }

        // No Next or Choices - close dialog
        quest.CloseDialog(silent: false);
    }

    private async Task HandleChoiceActions(DialogChoice choice, QuestActionContext context, CancellationToken cancellationToken)
    {
        // Execute choice-specific actions
        if (choice.Actions != null && choice.Actions.Count > 0)
        {
            var actionFactory = context.Services.GetRequiredService<QuestActionFactory>();
            var actions = actionFactory.CreateActions(choice.Actions);

            foreach (var action in actions)
            {
                await action.ExecuteAsync(context, cancellationToken);
            }
        }

        // Handle state transition
        if (!string.IsNullOrWhiteSpace(choice.NextState))
        {
            // Note: State transition will be handled by DeclarativeQuest after this action completes
            // We just set the quest flag to indicate which state to transition to
            context.State.SetIntFlag("__pending_state_transition", 1);
            context.State.SetStringFlag("__next_state", choice.NextState);
        }
    }
}

/// <summary>
/// Represents a single page in a quest dialog.
/// </summary>
public class DialogPage
{
    /// <summary>
    /// Text to display on this page.
    /// </summary>
    [JsonPropertyName("text")]
    public string Text { get; init; } = "";

    /// <summary>
    /// If true, shows a "Next" button to advance to next page.
    /// </summary>
    [JsonPropertyName("next")]
    public bool Next { get; init; }

    /// <summary>
    /// Array of choices for the player to select from.
    /// </summary>
    [JsonPropertyName("choices")]
    public List<DialogChoice>? Choices { get; init; }
}

/// <summary>
/// Represents a choice option in a dialog.
/// </summary>
public class DialogChoice
{
    /// <summary>
    /// Text displayed on the choice button.
    /// </summary>
    [JsonPropertyName("text")]
    public required string Text { get; init; }

    /// <summary>
    /// State to transition to after this choice is selected.
    /// </summary>
    [JsonPropertyName("next_state")]
    public string? NextState { get; init; }

    /// <summary>
    /// Actions to execute when this choice is selected.
    /// </summary>
    [JsonPropertyName("actions")]
    public List<JsonObject>? Actions { get; init; }

    /// <summary>
    /// Special action: "close" to close the dialog.
    /// </summary>
    [JsonPropertyName("action")]
    public string? Action { get; init; }
}
