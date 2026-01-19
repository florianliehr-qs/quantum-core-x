using AwesomeAssertions;
using QuantumCore.API.Core.Models;
using Xunit;

namespace Core.Tests;

public class QuestStateTests
{
    // IntFlag Tests

    [Fact]
    public void testIntFlags_givenNewQuestState_shouldBeEmpty()
    {
        var questState = new QuestState();

        questState.IntFlags.Count.Should().Be(0);
    }

    [Fact]
    public void testSetIntFlag_givenNewFlag_shouldStoreValue()
    {
        var questState = new QuestState();

        questState.SetIntFlag("kills", 5);

        questState.IntFlags["kills"].Should().Be(5);
    }

    [Fact]
    public void testGetIntFlag_givenExistingFlag_shouldReturnCorrectValue()
    {
        var questState = new QuestState();
        questState.SetIntFlag("ore_collected", 10);

        var result = questState.GetIntFlag("ore_collected");

        result.Should().Be(10);
    }

    [Fact]
    public void testGetIntFlag_givenNonExistentFlag_shouldReturnDefaultValue()
    {
        var questState = new QuestState();

        var result = questState.GetIntFlag("missing");

        result.Should().Be(0);
    }

    [Fact]
    public void testGetIntFlag_givenNonExistentFlagWithCustomDefault_shouldReturnCustomDefault()
    {
        var questState = new QuestState();

        var result = questState.GetIntFlag("missing", 42);

        result.Should().Be(42);
    }

    [Fact]
    public void testIncIntFlag_givenExistingFlag_shouldIncrementByOne()
    {
        var questState = new QuestState();
        questState.SetIntFlag("count", 5);

        questState.IncIntFlag("count");

        questState.GetIntFlag("count").Should().Be(6);
    }

    [Fact]
    public void testIncIntFlag_givenExistingFlagWithAmount_shouldIncrementByAmount()
    {
        var questState = new QuestState();
        questState.SetIntFlag("score", 100);

        questState.IncIntFlag("score", 25);

        questState.GetIntFlag("score").Should().Be(125);
    }

    [Fact]
    public void testIncIntFlag_givenNonExistentFlag_shouldSetToAmount()
    {
        var questState = new QuestState();

        questState.IncIntFlag("new_counter", 3);

        questState.GetIntFlag("new_counter").Should().Be(3);
    }

    [Fact]
    public void testDecIntFlag_givenExistingFlag_shouldDecrementByOne()
    {
        var questState = new QuestState();
        questState.SetIntFlag("lives", 3);

        questState.DecIntFlag("lives");

        questState.GetIntFlag("lives").Should().Be(2);
    }

    [Fact]
    public void testDecIntFlag_givenExistingFlagWithAmount_shouldDecrementByAmount()
    {
        var questState = new QuestState();
        questState.SetIntFlag("health", 100);

        questState.DecIntFlag("health", 20);

        questState.GetIntFlag("health").Should().Be(80);
    }

    [Fact]
    public void testDecIntFlag_givenNonExistentFlag_shouldSetToNegativeAmount()
    {
        var questState = new QuestState();

        questState.DecIntFlag("debt", 5);

        questState.GetIntFlag("debt").Should().Be(-5);
    }

    [Fact]
    public void testHasIntFlag_givenExistingFlag_shouldReturnTrue()
    {
        var questState = new QuestState();
        questState.SetIntFlag("flag", 1);

        var result = questState.HasIntFlag("flag");

        result.Should().BeTrue();
    }

    [Fact]
    public void testHasIntFlag_givenNonExistentFlag_shouldReturnFalse()
    {
        var questState = new QuestState();

        var result = questState.HasIntFlag("missing");

        result.Should().BeFalse();
    }

    [Fact]
    public void testRemoveIntFlag_givenExistingFlag_shouldReturnTrue()
    {
        var questState = new QuestState();
        questState.SetIntFlag("temp", 5);

        var result = questState.RemoveIntFlag("temp");

        result.Should().BeTrue();
    }

    [Fact]
    public void testRemoveIntFlag_givenNonExistentFlag_shouldReturnFalse()
    {
        var questState = new QuestState();

        var result = questState.RemoveIntFlag("missing");

        result.Should().BeFalse();
    }

    [Fact]
    public void testRemoveIntFlag_givenExistingFlag_shouldActuallyRemoveFlag()
    {
        var questState = new QuestState();
        questState.SetIntFlag("temp", 5);

        questState.RemoveIntFlag("temp");

        questState.HasIntFlag("temp").Should().BeFalse();
    }

    // StringFlag Tests

    [Fact]
    public void testStringFlags_givenNewQuestState_shouldBeEmpty()
    {
        var questState = new QuestState();

        questState.StringFlags.Count.Should().Be(0);
    }

    [Fact]
    public void testSetStringFlag_givenNewFlag_shouldStoreValue()
    {
        var questState = new QuestState();

        questState.SetStringFlag("location", "village");

        questState.StringFlags["location"].Should().Be("village");
    }

    [Fact]
    public void testGetStringFlag_givenExistingFlag_shouldReturnCorrectValue()
    {
        var questState = new QuestState();
        questState.SetStringFlag("npc_name", "Blacksmith");

        var result = questState.GetStringFlag("npc_name");

        result.Should().Be("Blacksmith");
    }

    [Fact]
    public void testGetStringFlag_givenNonExistentFlag_shouldReturnEmptyString()
    {
        var questState = new QuestState();

        var result = questState.GetStringFlag("missing");

        result.Should().Be("");
    }

    [Fact]
    public void testGetStringFlag_givenNonExistentFlagWithCustomDefault_shouldReturnCustomDefault()
    {
        var questState = new QuestState();

        var result = questState.GetStringFlag("missing", "default_value");

        result.Should().Be("default_value");
    }

    [Fact]
    public void testHasStringFlag_givenExistingFlag_shouldReturnTrue()
    {
        var questState = new QuestState();
        questState.SetStringFlag("tag", "important");

        var result = questState.HasStringFlag("tag");

        result.Should().BeTrue();
    }

    [Fact]
    public void testHasStringFlag_givenNonExistentFlag_shouldReturnFalse()
    {
        var questState = new QuestState();

        var result = questState.HasStringFlag("missing");

        result.Should().BeFalse();
    }

    [Fact]
    public void testRemoveStringFlag_givenExistingFlag_shouldReturnTrue()
    {
        var questState = new QuestState();
        questState.SetStringFlag("temp", "value");

        var result = questState.RemoveStringFlag("temp");

        result.Should().BeTrue();
    }

    [Fact]
    public void testRemoveStringFlag_givenNonExistentFlag_shouldReturnFalse()
    {
        var questState = new QuestState();

        var result = questState.RemoveStringFlag("missing");

        result.Should().BeFalse();
    }

    [Fact]
    public void testRemoveStringFlag_givenExistingFlag_shouldActuallyRemoveFlag()
    {
        var questState = new QuestState();
        questState.SetStringFlag("temp", "value");

        questState.RemoveStringFlag("temp");

        questState.HasStringFlag("temp").Should().BeFalse();
    }

    // BoolFlag Tests

    [Fact]
    public void testBoolFlags_givenNewQuestState_shouldBeEmpty()
    {
        var questState = new QuestState();

        questState.BoolFlags.Count.Should().Be(0);
    }

    [Fact]
    public void testSetBoolFlag_givenNewFlag_shouldStoreValue()
    {
        var questState = new QuestState();

        questState.SetBoolFlag("completed_tutorial", true);

        questState.BoolFlags["completed_tutorial"].Should().BeTrue();
    }

    [Fact]
    public void testGetBoolFlag_givenExistingFlag_shouldReturnCorrectValue()
    {
        var questState = new QuestState();
        questState.SetBoolFlag("found_secret", true);

        var result = questState.GetBoolFlag("found_secret");

        result.Should().BeTrue();
    }

    [Fact]
    public void testGetBoolFlag_givenNonExistentFlag_shouldReturnFalse()
    {
        var questState = new QuestState();

        var result = questState.GetBoolFlag("missing");

        result.Should().BeFalse();
    }

    [Fact]
    public void testGetBoolFlag_givenNonExistentFlagWithCustomDefault_shouldReturnCustomDefault()
    {
        var questState = new QuestState();

        var result = questState.GetBoolFlag("missing", true);

        result.Should().BeTrue();
    }

    [Fact]
    public void testHasBoolFlag_givenExistingFlag_shouldReturnTrue()
    {
        var questState = new QuestState();
        questState.SetBoolFlag("unlocked", true);

        var result = questState.HasBoolFlag("unlocked");

        result.Should().BeTrue();
    }

    [Fact]
    public void testHasBoolFlag_givenNonExistentFlag_shouldReturnFalse()
    {
        var questState = new QuestState();

        var result = questState.HasBoolFlag("missing");

        result.Should().BeFalse();
    }

    [Fact]
    public void testRemoveBoolFlag_givenExistingFlag_shouldReturnTrue()
    {
        var questState = new QuestState();
        questState.SetBoolFlag("temp", true);

        var result = questState.RemoveBoolFlag("temp");

        result.Should().BeTrue();
    }

    [Fact]
    public void testRemoveBoolFlag_givenNonExistentFlag_shouldReturnFalse()
    {
        var questState = new QuestState();

        var result = questState.RemoveBoolFlag("missing");

        result.Should().BeFalse();
    }

    [Fact]
    public void testRemoveBoolFlag_givenExistingFlag_shouldActuallyRemoveFlag()
    {
        var questState = new QuestState();
        questState.SetBoolFlag("temp", true);

        questState.RemoveBoolFlag("temp");

        questState.HasBoolFlag("temp").Should().BeFalse();
    }

    // Clear All Flags Tests

    [Fact]
    public void testClearAllFlags_givenMultipleFlags_shouldClearIntFlags()
    {
        var questState = new QuestState();
        questState.SetIntFlag("count", 5);
        questState.SetStringFlag("name", "test");
        questState.SetBoolFlag("active", true);

        questState.ClearAllFlags();

        questState.IntFlags.Count.Should().Be(0);
    }

    [Fact]
    public void testClearAllFlags_givenMultipleFlags_shouldClearStringFlags()
    {
        var questState = new QuestState();
        questState.SetIntFlag("count", 5);
        questState.SetStringFlag("name", "test");
        questState.SetBoolFlag("active", true);

        questState.ClearAllFlags();

        questState.StringFlags.Count.Should().Be(0);
    }

    [Fact]
    public void testClearAllFlags_givenMultipleFlags_shouldClearBoolFlags()
    {
        var questState = new QuestState();
        questState.SetIntFlag("count", 5);
        questState.SetStringFlag("name", "test");
        questState.SetBoolFlag("active", true);

        questState.ClearAllFlags();

        questState.BoolFlags.Count.Should().Be(0);
    }

    // State Property Tests

    [Fact]
    public void testCurrentState_givenNewQuestState_shouldDefaultToStart()
    {
        var questState = new QuestState();

        questState.CurrentState.Should().Be("start");
    }

    [Fact]
    public void testCurrentState_givenSetValue_shouldUpdateState()
    {
        var questState = new QuestState { CurrentState = "gathering" };

        questState.CurrentState.Should().Be("gathering");
    }

    [Fact]
    public void testIsCompleted_givenNewQuestState_shouldBeFalse()
    {
        var questState = new QuestState();

        questState.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void testIsCompleted_givenSetToTrue_shouldBeTrue()
    {
        var questState = new QuestState { IsCompleted = true };

        questState.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void testCompletedAt_givenSetValue_shouldStoreCompletionTime()
    {
        var completionTime = DateTime.UtcNow;
        var questState = new QuestState { CompletedAt = completionTime };

        questState.CompletedAt.Should().Be(completionTime);
    }

    [Fact]
    public void testStartedAt_givenSetValue_shouldStoreStartTime()
    {
        var startTime = DateTime.UtcNow;
        var questState = new QuestState { StartedAt = startTime };

        questState.StartedAt.Should().Be(startTime);
    }

    [Fact]
    public void testQuestId_givenSetValue_shouldStoreQuestIdentifier()
    {
        var questState = new QuestState { QuestId = "beginner_sword_quest" };

        questState.QuestId.Should().Be("beginner_sword_quest");
    }

    [Fact]
    public void testPlayerId_givenSetValue_shouldStorePlayerIdentifier()
    {
        var playerId = Guid.NewGuid();
        var questState = new QuestState { PlayerId = playerId };

        questState.PlayerId.Should().Be(playerId);
    }
}
