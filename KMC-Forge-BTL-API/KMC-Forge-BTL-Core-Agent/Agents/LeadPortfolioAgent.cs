using System.Threading.Tasks;
using KMC_Forge_BTL_Core_Agent.Agents;
using KMC_Forge_BTL_Models.PDFExtractorResponse;
using Microsoft.Extensions.Configuration;

namespace KMC_Forge_BTL_Core_Agent.Agents
{
    public class LeadPortfolioAgent
    {
        private readonly DocumentValidatorAgent _documentValidatorAgent;

        public LeadPortfolioAgent(IConfiguration configuration)
        {
            _documentValidatorAgent = new DocumentValidatorAgent(configuration);
        }

        public async Task<CompanyInfo> StartProcessing(string filePath)
        {
            // Orchestrate the PDF extraction by calling DocumentValidatorAgent
            var result = await _documentValidatorAgent.ExtractDataFromPdfAsync(filePath);
            return result;
        }

        public async Task<string> OrchestrateDocumentRetrievalAsync(string filePath)
        {
            // Orchestrate the file retrieval by calling DocumentValidatorAgent
            var result = await _documentValidatorAgent.ExtractFileFromBlobAsync(filePath);
            return result;
        }
    }
}