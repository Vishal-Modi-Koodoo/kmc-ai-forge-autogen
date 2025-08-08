namespace KMC_AI_Forge_BTL_Agent.Contracts;

public record TopicId(string Name);

public interface IAgentRuntime
{
    public Task<object?> PublishMessage(object message, TopicId topic);
}