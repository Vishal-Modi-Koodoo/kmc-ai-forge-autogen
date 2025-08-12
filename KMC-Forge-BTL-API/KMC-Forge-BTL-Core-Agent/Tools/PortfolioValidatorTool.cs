using AutoGen.Core;
using AutoGen.OpenAI;
using KMC_Forge_BTL_Configurations;

namespace KMC_Forge_BTL_Core_Agent.Tools
{
    public class PortfolioValidatorTool
    {
        private readonly MiddlewareStreamingAgent<OpenAIChatAgent> _pdfAnalyserAgent;
        private readonly AppConfiguration _config;
        public PortfolioValidatorTool(MiddlewareStreamingAgent<OpenAIChatAgent> pdfAnalyserAgent)
        {
           _pdfAnalyserAgent = pdfAnalyserAgent;
            _config = AppConfiguration.Instance; 
        }
    }
}