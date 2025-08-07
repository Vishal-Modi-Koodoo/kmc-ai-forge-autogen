using System.Threading.Tasks;
using KMC_Forge_BTL_Core_Agent.Agents;
using AutoGen.OpenAI;
using Azure.AI.OpenAI;
using KMC_Forge_BTL_Core_Agent.Utils;
using AutoGen;
using AutoGen.Core;

namespace KMC_Forge_BTL_Core_Agent.Tools
{
    public class PdfExtractionTool : OpenAIChatAgent
    {
        public PdfExtractionTool(AzureOpenAIClient openAIClient, string model, string analysisPrompt)
    : base(
        name: "pdf_analyzer",
        systemMessage: analysisPrompt,
      chatClient: openAIClient.GetChatClient(model))
        {

        }
        
    }
}