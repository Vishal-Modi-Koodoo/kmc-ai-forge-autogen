using System.Threading.Tasks;
using KMC_Forge_BTL_Core_Agent.Agents;

namespace KMC_Forge_BTL_Core_Agent.Agents
{
    public class LeadPortfolioAgent
    {
        private readonly DocumentValidatorAgent _documentValidatorAgent;

        public LeadPortfolioAgent()
        {
            _documentValidatorAgent = new DocumentValidatorAgent();
        }

        public async Task<string> OrchestrateDocumentValidationAsync(string filePath)
        {
            // Orchestrate the PDF extraction by calling DocumentValidatorAgent
            var result = await _documentValidatorAgent.ExtractDataFromPdfAsync(filePath);
            return result;
        }


    }
}