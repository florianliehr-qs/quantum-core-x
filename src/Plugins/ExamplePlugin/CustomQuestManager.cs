using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game.World;

namespace ExamplePlugin;

public class CustomQuestManager : IQuestManager
{
    private readonly ILogger<CustomQuestManager> _logger;

    public CustomQuestManager(ILogger<CustomQuestManager> logger)
    {
        _logger = logger;
    }
    
    public void Init()
    {
        _logger.LogInformation("Hello World from {Type}", nameof(CustomQuestManager));
    }

    public void InitializePlayer(IPlayerEntity player)
    {
    }

    public Task InitializePlayerAsync(IPlayerEntity player)
    {
        return Task.CompletedTask;
    }

    public Task SavePlayerQuestsAsync(IPlayerEntity player)
    {
        return Task.CompletedTask;
    }

    public void RegisterQuest(Type questType)
    {
    }
}