using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using QuantumCore.Game.Quest.Conditions;

namespace QuantumCore.Game.Quest.Factories;

/// <summary>
/// Factory for creating quest condition instances from JSON definitions.
/// Maps condition type strings to concrete condition implementations.
/// </summary>
public class QuestConditionFactory
{
    private readonly ILogger<QuestConditionFactory> _logger;
    private readonly Dictionary<string, Type> _conditionTypes = new();

    public QuestConditionFactory(ILogger<QuestConditionFactory> logger)
    {
        _logger = logger;
        RegisterDefaultConditions();
    }

    /// <summary>
    /// Registers the default condition types provided by the framework.
    /// </summary>
    private void RegisterDefaultConditions()
    {
        RegisterCondition("quest_not_started", typeof(QuestNotStartedCondition));
        RegisterCondition("quest_flag_gte", typeof(QuestFlagGteCondition));
        RegisterCondition("quest_flag_eq", typeof(QuestFlagEqCondition));
        RegisterCondition("level_min", typeof(LevelMinCondition));
        RegisterCondition("level_max", typeof(LevelMaxCondition));
        RegisterCondition("class_check", typeof(ClassCheckCondition));
        RegisterCondition("has_item", typeof(HasItemCondition));
        RegisterCondition("and", typeof(AndCondition));
        RegisterCondition("or", typeof(OrCondition));
        RegisterCondition("not", typeof(NotCondition));
    }

    /// <summary>
    /// Registers a custom condition type.
    /// </summary>
    /// <param name="typeName">The type name used in JSON</param>
    /// <param name="conditionType">The concrete condition type</param>
    public void RegisterCondition(string typeName, Type conditionType)
    {
        if (!typeof(IQuestCondition).IsAssignableFrom(conditionType))
        {
            throw new ArgumentException($"Type {conditionType.Name} must implement IQuestCondition", nameof(conditionType));
        }

        _conditionTypes[typeName] = conditionType;
        _logger.LogDebug("Registered condition type {TypeName} -> {ConditionType}", typeName, conditionType.Name);
    }

    /// <summary>
    /// Creates a condition instance from a JSON object.
    /// </summary>
    /// <param name="conditionJson">The JSON object containing condition definition</param>
    /// <returns>An instantiated condition, or null if the condition type is unknown</returns>
    public IQuestCondition? CreateCondition(JsonObject conditionJson)
    {
        if (conditionJson["type"] is not JsonValue typeNode)
        {
            _logger.LogWarning("Condition JSON is missing required 'type' field");
            return null;
        }

        var typeName = typeNode.GetValue<string>();
        if (!_conditionTypes.TryGetValue(typeName, out var conditionType))
        {
            _logger.LogWarning("Unknown condition type: {TypeName}", typeName);
            return null;
        }

        try
        {
            // Special handling for composite conditions (and, or, not)
            // They contain nested conditions that need recursive processing
            IQuestCondition? condition = null;

            if (typeName == "and" || typeName == "or")
            {
                condition = CreateCompositeCondition(conditionJson, typeName);
            }
            else if (typeName == "not")
            {
                condition = CreateNotCondition(conditionJson);
            }
            else
            {
                // Simple condition - direct deserialization
                condition = JsonSerializer.Deserialize(conditionJson, conditionType, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                }) as IQuestCondition;
            }

            if (condition == null)
            {
                _logger.LogWarning("Failed to deserialize condition of type {TypeName}", typeName);
                return null;
            }

            _logger.LogTrace("Created condition of type {TypeName}", typeName);
            return condition;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize condition of type {TypeName}", typeName);
            return null;
        }
    }

    private IQuestCondition? CreateCompositeCondition(JsonObject conditionJson, string typeName)
    {
        if (conditionJson["conditions"] is not JsonArray conditionsArray)
        {
            _logger.LogWarning("{TypeName} condition is missing 'conditions' array", typeName);
            return null;
        }

        var conditions = new List<IQuestCondition>();
        foreach (var item in conditionsArray)
        {
            if (item is JsonObject nestedConditionJson)
            {
                var nestedCondition = CreateCondition(nestedConditionJson);
                if (nestedCondition != null)
                {
                    conditions.Add(nestedCondition);
                }
            }
        }

        if (conditions.Count == 0)
        {
            _logger.LogWarning("{TypeName} condition has no valid nested conditions", typeName);
            return null;
        }

        return typeName == "and"
            ? new AndCondition { Conditions = conditions }
            : new OrCondition { Conditions = conditions };
    }

    private IQuestCondition? CreateNotCondition(JsonObject conditionJson)
    {
        if (conditionJson["condition"] is not JsonObject nestedConditionJson)
        {
            _logger.LogWarning("Not condition is missing 'condition' field");
            return null;
        }

        var nestedCondition = CreateCondition(nestedConditionJson);
        if (nestedCondition == null)
        {
            _logger.LogWarning("Not condition has invalid nested condition");
            return null;
        }

        return new NotCondition { Condition = nestedCondition };
    }

    /// <summary>
    /// Creates multiple condition instances from a list of JSON objects.
    /// </summary>
    /// <param name="conditionsJson">List of JSON objects containing condition definitions</param>
    /// <returns>List of instantiated conditions (skips any that fail to deserialize)</returns>
    public List<IQuestCondition> CreateConditions(List<JsonObject> conditionsJson)
    {
        var conditions = new List<IQuestCondition>();
        foreach (var conditionJson in conditionsJson)
        {
            var condition = CreateCondition(conditionJson);
            if (condition != null)
            {
                conditions.Add(condition);
            }
        }
        return conditions;
    }
}
