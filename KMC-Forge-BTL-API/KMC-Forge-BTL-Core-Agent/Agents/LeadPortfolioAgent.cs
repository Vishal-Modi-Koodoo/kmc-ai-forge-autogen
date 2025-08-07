using System.Threading.Tasks;
using KMC_Forge_BTL_Core_Agent.Agents;
using KMC_Forge_BTL_Models.PDFExtractorResponse;

namespace KMC_Forge_BTL_Core_Agent.Agents
{
    public class LeadPortfolioAgent
    {
        private readonly DocumentValidatorAgent _documentValidatorAgent;

        public LeadPortfolioAgent()
        {
            _documentValidatorAgent = new DocumentValidatorAgent();
        }

        public async Task<CompanyInfo> OrchestrateDocumentValidationAsync(string filePath)
        {
            // Orchestrate the PDF extraction by calling DocumentValidatorAgent
            var result = await _documentValidatorAgent.ExtractDataFromPdfAsync(filePath);
            return result;
        }

        public async Task<Stream> OrchestrateDocumentRetrievalAsync(string filePath)
        {
            // Orchestrate the file retrieval by calling DocumentValidatorAgent
            var result = await _documentValidatorAgent.ExtractFileFromBlobAsync(filePath);
            return result;
        }
    }
}