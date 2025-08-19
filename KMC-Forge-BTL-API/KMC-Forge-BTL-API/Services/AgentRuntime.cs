using KMC.Forge.BTL.API.Contracts;

namespace KMC.Forge.BTL.API.Services;

public class AgentRuntime : IAgentRuntime
{
    public async Task<object?> PublishMessage(object message, TopicId topic)
    {
        // TODO: Implement the actual message publishing logic here.
        await Task.CompletedTask;
        return null;
    }
}