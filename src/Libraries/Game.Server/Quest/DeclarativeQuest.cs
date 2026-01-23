using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Persistence;
using QuantumCore.Game.Quest.Actions;
using QuantumCore.Game.Quest.Conditions;
using QuantumCore.Game.Quest.Factories;
using QuantumCore.Game.Quest.Models;

namespace QuantumCore.Game.Quest;

/// <summary>
/// Runtime implementation of a declarative quest loaded from JSON.
/// Handles trigger registration, action execution, and state management.
/// </summary>
public class DeclarativeQuest : Quest
{
    private readonly QuestDefinition _definition;
    private readonly QuestActionFactory _actionFactory;
    private readonly QuestConditionFactory _conditionFactory;
    private readonly IServiceProvider _services;
    private readonly ILogger _logger;
    private readonly object _triggerToken;

    public DeclarativeQuest(
        QuestState state,
        IPlayerEntity player,
        QuestDefinition definition,
        QuestActionFactory actionFactory,
        QuestConditionFactory conditionFactory,
        IServiceProvider services,
        ILogger<DeclarativeQuest> logger) : base(state, player)
    {
        _definition = definition;
        _actionFactory = actionFactory;
        _conditionFactory = conditionFactory;
        _services = services;
        _logger = logger;
        _triggerToken = new object(); // Unique token for this quest instance
    }

    public override void Init()
    {
        // Execute on_enter actions for initial state if not already started
        if (State.CurrentState == "start" && State.StartedAt == default)
        {
            State.StartedAt = DateTime.UtcNow;
            ExecuteStateEnterActions("start").Wait();
        }

        // Register triggers for current state
        RegisterTriggersForCurrentState();
    }

    private void RegisterTriggersForCurrentState()
    {
        if (!_definition.States.TryGetValue(State.CurrentState, out var stateDefinition))
        {
            _logger.LogWarning("Quest {QuestId} - Current state '{StateName}' not found in definition",
                _definition.Id, State.CurrentState);
            return;
        }

        if (stateDefinition.Triggers == null || stateDefinition.Triggers.Count == 0)
        {
            return;
        }

        foreach (var trigger in stateDefinition.Triggers)
        {
            RegisterTrigger(trigger);
        }
    }

    private void RegisterTrigger(TriggerDefinition trigger)
    {
        switch (trigger.Type.ToLowerInvariant())
        {
            case "npc_click":
                if (trigger.NpcId.HasValue)
                {
                    GameEventManager.RegisterNpcClickEvent(
                        $"{_definition.Name} ({State.CurrentState})",
                        trigger.NpcId.Value,
                        async (player) => await HandleTriggerFired(trigger),
                        player => player.Vid == Player.Vid && EvaluateTriggerCondition(trigger),
                        _triggerToken);
                }
                else
                {
                    _logger.LogWarning("Quest {QuestId} - npc_click trigger missing npc_id", _definition.Id);
                }
                break;

            case "npc_give":
                if (trigger.NpcId.HasValue)
                {
                    GameEventManager.RegisterNpcGiveEvent(
                        $"{_definition.Name} ({State.CurrentState})",
                        trigger.NpcId.Value,
                        async (player, item) => await HandleTriggerFired(trigger),
                        (player, item) => player.Vid == Player.Vid &&
                                        (!trigger.ItemId.HasValue || item.ItemId == trigger.ItemId.Value) &&
                                        EvaluateTriggerCondition(trigger),
                        _triggerToken);
                }
                else
                {
                    _logger.LogWarning("Quest {QuestId} - npc_give trigger missing npc_id", _definition.Id);
                }
                break;

            default:
                _logger.LogWarning("Quest {QuestId} - Unknown trigger type: {TriggerType}",
                    _definition.Id, trigger.Type);
                break;
        }
    }

    private bool EvaluateTriggerCondition(TriggerDefinition trigger)
    {
        if (trigger.Condition == null)
        {
            return true;
        }

        var condition = _conditionFactory.CreateCondition(trigger.Condition);
        if (condition == null)
        {
            _logger.LogWarning("Quest {QuestId} - Failed to create condition for trigger", _definition.Id);
            return false;
        }

        var context = new QuestConditionContext
        {
            Player = Player,
            State = State,
            Services = _services
        };

        return condition.Evaluate(context);
    }

    private async Task HandleTriggerFired(TriggerDefinition trigger)
    {
        _logger.LogDebug("Quest {QuestId} - Trigger fired: {TriggerType} in state {StateName}",
            _definition.Id, trigger.Type, State.CurrentState);

        // Execute trigger actions
        await ExecuteActions(trigger.Actions);

        // Handle state transition
        if (!string.IsNullOrEmpty(trigger.NextState))
        {
            await TransitionToState(trigger.NextState);
        }

        // Save quest state
        using var scope = _services.CreateScope();
        var questRepository = scope.ServiceProvider.GetRequiredService<IDbQuestRepository>();
        await questRepository.SaveQuestStateAsync(Player.Player.Id, State);
    }

    private async Task ExecuteActions(List<JsonObject> actionsJson)
    {
        var actions = _actionFactory.CreateActions(actionsJson);

        var context = new QuestActionContext
        {
            Player = Player,
            State = State,
            Services = _services,
            Logger = _logger,
            Quest = this  // Pass quest instance for dialog support
        };

        foreach (var action in actions)
        {
            try
            {
                await action.ExecuteAsync(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Quest {QuestId} - Error executing action {ActionType}",
                    _definition.Id, action.GetType().Name);
            }
        }
    }

    #region Public Dialog Methods for Actions

    /// <summary>
    /// Adds text to the quest dialog script.
    /// </summary>
    public void AddDialogText(string text)
    {
        Text(text);
    }

    /// <summary>
    /// Shows a "Next" button and waits for player to click it.
    /// </summary>
    public async Task ShowNext()
    {
        await Next();
    }

    /// <summary>
    /// Shows multiple choice buttons and waits for player selection.
    /// </summary>
    /// <param name="options">Array of choice text strings</param>
    /// <param name="done">If true, closes dialog after choice</param>
    /// <returns>Zero-based index of the selected choice</returns>
    public async Task<byte> ShowChoice(string[] options, bool done = false)
    {
        return await Choice(done, options);
    }

    /// <summary>
    /// Closes the quest dialog.
    /// </summary>
    /// <param name="silent">If true, doesn't show final ENTER before closing</param>
    public void CloseDialog(bool silent = false)
    {
        Done(silent);
    }

    #endregion

    private async Task TransitionToState(string newState)
    {
        if (!_definition.States.ContainsKey(newState))
        {
            _logger.LogError("Quest {QuestId} - Cannot transition to non-existent state: {StateName}",
                _definition.Id, newState);
            return;
        }

        var oldState = State.CurrentState;

        // Execute on_exit actions for current state
        if (_definition.States.TryGetValue(oldState, out var oldStateDefinition))
        {
            if (oldStateDefinition.OnExit != null && oldStateDefinition.OnExit.Count > 0)
            {
                await ExecuteActions(oldStateDefinition.OnExit);
            }
        }

        // Unregister all triggers for the old state
        UnregisterAllTriggers();

        // Update state
        State.CurrentState = newState;
        _logger.LogInformation("Quest {QuestId} - Transitioned from '{OldState}' to '{NewState}'",
            _definition.Id, oldState, newState);

        // Execute on_enter actions for new state
        await ExecuteStateEnterActions(newState);

        // Register triggers for new state
        RegisterTriggersForCurrentState();
    }

    private void UnregisterAllTriggers()
    {
        GameEventManager.UnregisterNpcClickEventsByToken(_triggerToken);
        GameEventManager.UnregisterNpcGiveEventsByToken(_triggerToken);
    }

    private async Task ExecuteStateEnterActions(string stateName)
    {
        if (!_definition.States.TryGetValue(stateName, out var stateDefinition))
        {
            return;
        }

        if (stateDefinition.OnEnter != null && stateDefinition.OnEnter.Count > 0)
        {
            await ExecuteActions(stateDefinition.OnEnter);
        }
    }
}
