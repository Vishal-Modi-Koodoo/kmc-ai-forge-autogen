using KMC_AI_Forge_BTL_Agent.Contracts;

namespace KMC_AI_Forge_BTL_Agent.Services;

public class AgentRuntime : IAgentRuntime
{
    public async Task<object?> PublishMessage(object message, TopicId topic)
    {
        // TODO: Implement the actual message publishing logic here.
        await Task.CompletedTask;
        return null;
    }
}