namespace KMC.Forge.BTL.API.Contracts;

public record TopicId(string Name);

public interface IAgentRuntime
{
    public Task<object?> PublishMessage(object message, TopicId topic);
}