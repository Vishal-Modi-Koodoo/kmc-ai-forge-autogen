using AutoGen.OpenAI;
using Azure.AI.OpenAI;

namespace KMC_Forge_BTL_Core_Agent.Agents.SubAgents
{
    public class PDFAnalyserAgent : OpenAIChatAgent
    {
        public PDFAnalyserAgent(AzureOpenAIClient openAIClient, string model, string analysisPrompt)
            : base(
                name: "pdf_analyzer",
                systemMessage: analysisPrompt,
                chatClient: openAIClient.GetChatClient(model))
        {
        }
    }
}
