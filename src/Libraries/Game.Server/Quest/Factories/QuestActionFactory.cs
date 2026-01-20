using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using QuantumCore.Game.Quest.Actions;

namespace QuantumCore.Game.Quest.Factories;

/// <summary>
/// Factory for creating quest action instances from JSON definitions.
/// Maps action type strings to concrete action implementations.
/// </summary>
public class QuestActionFactory
{
    private readonly ILogger<QuestActionFactory> _logger;
    private readonly Dictionary<string, Type> _actionTypes = new();

    public QuestActionFactory(ILogger<QuestActionFactory> logger)
    {
        _logger = logger;
        RegisterDefaultActions();
    }

    /// <summary>
    /// Registers the default action types provided by the framework.
    /// </summary>
    private void RegisterDefaultActions()
    {
        RegisterAction("set_quest_flag", typeof(SetQuestFlagAction));
        RegisterAction("inc_quest_flag", typeof(IncQuestFlagAction));
        RegisterAction("set_state", typeof(SetStateAction));
        RegisterAction("complete_quest", typeof(CompleteQuestAction));
        RegisterAction("give_item", typeof(GiveItemAction));
        RegisterAction("remove_item", typeof(RemoveItemAction));
        RegisterAction("give_exp", typeof(GiveExpAction));
        RegisterAction("give_gold", typeof(GiveGoldAction));
        RegisterAction("send_letter", typeof(SendLetterAction));
        RegisterAction("warp", typeof(WarpAction));
        RegisterAction("spawn_monster", typeof(SpawnMonsterAction));
        RegisterAction("condition", typeof(ConditionAction));
        RegisterAction("delay", typeof(DelayAction));
        RegisterAction("dialog", typeof(DialogAction));
    }

    /// <summary>
    /// Registers a custom action type.
    /// </summary>
    /// <param name="typeName">The type name used in JSON</param>
    /// <param name="actionType">The concrete action type</param>
    public void RegisterAction(string typeName, Type actionType)
    {
        if (!typeof(IQuestAction).IsAssignableFrom(actionType))
        {
            throw new ArgumentException($"Type {actionType.Name} must implement IQuestAction", nameof(actionType));
        }

        _actionTypes[typeName] = actionType;
        _logger.LogDebug("Registered action type {TypeName} -> {ActionType}", typeName, actionType.Name);
    }

    /// <summary>
    /// Creates an action instance from a JSON object.
    /// </summary>
    /// <param name="actionJson">The JSON object containing action definition</param>
    /// <returns>An instantiated action, or null if the action type is unknown</returns>
    public IQuestAction? CreateAction(JsonObject actionJson)
    {
        if (actionJson["type"] is not JsonValue typeNode)
        {
            _logger.LogWarning("Action JSON is missing required 'type' field");
            return null;
        }

        var typeName = typeNode.GetValue<string>();
        if (!_actionTypes.TryGetValue(typeName, out var actionType))
        {
            _logger.LogWarning("Unknown action type: {TypeName}", typeName);
            return null;
        }

        try
        {
            var action = JsonSerializer.Deserialize(actionJson, actionType, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            }) as IQuestAction;

            if (action == null)
            {
                _logger.LogWarning("Failed to deserialize action of type {TypeName}", typeName);
                return null;
            }

            _logger.LogTrace("Created action of type {TypeName}", typeName);
            return action;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize action of type {TypeName}", typeName);
            return null;
        }
    }

    /// <summary>
    /// Creates multiple action instances from a list of JSON objects.
    /// </summary>
    /// <param name="actionsJson">List of JSON objects containing action definitions</param>
    /// <returns>List of instantiated actions (skips any that fail to deserialize)</returns>
    public List<IQuestAction> CreateActions(List<JsonObject> actionsJson)
    {
        var actions = new List<IQuestAction>();
        foreach (var actionJson in actionsJson)
        {
            var action = CreateAction(actionJson);
            if (action != null)
            {
                actions.Add(action);
            }
        }
        return actions;
    }
}
