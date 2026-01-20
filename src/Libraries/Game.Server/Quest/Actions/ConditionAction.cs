using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumCore.Game.Quest.Conditions;
using QuantumCore.Game.Quest.Factories;

namespace QuantumCore.Game.Quest.Actions;

/// <summary>
/// Evaluates a condition and executes different actions based on the result.
/// </summary>
public class ConditionAction : QuestActionBase
{
    /// <summary>
    /// Condition to evaluate.
    /// </summary>
    [JsonPropertyName("condition")]
    public required JsonObject ConditionJson { get; init; }

    /// <summary>
    /// Actions to execute if condition evaluates to true.
    /// </summary>
    [JsonPropertyName("then")]
    public List<JsonObject> ThenActions { get; init; } = new();

    /// <summary>
    /// Actions to execute if condition evaluates to false (optional).
    /// </summary>
    [JsonPropertyName("else")]
    public List<JsonObject>? ElseActions { get; init; }

    [JsonIgnore]
    private IQuestCondition? _condition;

    [JsonIgnore]
    private List<IQuestAction>? _thenActionInstances;

    [JsonIgnore]
    private List<IQuestAction>? _elseActionInstances;

    public override async Task ExecuteAsync(QuestActionContext context, CancellationToken cancellationToken = default)
    {
        // Lazy initialization of condition and actions
        if (_condition == null)
        {
            var conditionFactory = context.Services.GetRequiredService<QuestConditionFactory>();
            _condition = conditionFactory.CreateCondition(ConditionJson);

            if (_condition == null)
            {
                context.Logger.LogWarning("ConditionAction: Failed to create condition for quest {QuestId}",
                    context.State.QuestId);
                return;
            }
        }

        if (_thenActionInstances == null)
        {
            var actionFactory = context.Services.GetRequiredService<QuestActionFactory>();
            _thenActionInstances = actionFactory.CreateActions(ThenActions);
        }

        if (_elseActionInstances == null && ElseActions != null)
        {
            var actionFactory = context.Services.GetRequiredService<QuestActionFactory>();
            _elseActionInstances = actionFactory.CreateActions(ElseActions);
        }

        // Evaluate condition
        var conditionContext = new QuestConditionContext
        {
            Player = context.Player,
            State = context.State,
            Services = context.Services
        };

        var conditionResult = _condition.Evaluate(conditionContext);

        // Execute appropriate actions
        var actionsToExecute = conditionResult ? _thenActionInstances : _elseActionInstances;
        if (actionsToExecute != null)
        {
            foreach (var action in actionsToExecute)
            {
                await action.ExecuteAsync(context, cancellationToken);
            }
        }
    }
}
