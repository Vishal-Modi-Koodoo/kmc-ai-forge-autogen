using AutoGen.OpenAI;
using Azure.AI.OpenAI;

namespace KMC_Forge_BTL_Core_Agent.Agents.SubAgents
{
    public class DocumentAnalyserAgent : OpenAIChatAgent
    {
        public DocumentAnalyserAgent(AzureOpenAIClient openAIClient, string model, string analysisPrompt, string name)
            : base(
                name: name,
                systemMessage: analysisPrompt,
                chatClient: openAIClient.GetChatClient(model))
        {
        }
    }
}
