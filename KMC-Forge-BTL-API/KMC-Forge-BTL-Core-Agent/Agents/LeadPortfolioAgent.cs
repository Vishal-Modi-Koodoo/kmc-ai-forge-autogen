using System.Threading.Tasks;
using KMC_Forge_BTL_Core_Agent.Agents;
using KMC_Forge_BTL_Models.PDFExtractorResponse;
using KMC_Forge_BTL_Models.DocumentIdentificationResponse;
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

        public async Task<DocumentProcessingResult> PortfolioCompletion(DocumentProcessingResult identificationResult)
        {
            // Process document through DocumentValidatorAgent (includes document type checking and extraction)
            return await _documentValidatorAgent.PortfolioCompletion(identificationResult);
        }

        public async Task<DocumentProcessingResult> IdentifyDocumentType(string filePath, string fileName, long fileSize)
        {
            // Process document through DocumentValidatorAgent (includes document type checking and extraction)
            return await _documentValidatorAgent.IdentifyDocumentType(filePath, fileName, fileSize);
        }

        public async Task<DocumentProcessingResult> ValidateCompanyHouseData(DocumentProcessingResult identificationResult)
        {
            // Process document through DocumentValidatorAgent (includes document type checking and extraction)
            return await _documentValidatorAgent.ValidateCompanyHouseData(identificationResult);
        }

        public async Task<CompanyInfo> StartProcessing(string filePath, string imagePath)
        {
            // Orchestrate the PDF extraction by calling DocumentValidatorAgent
            return await _documentValidatorAgent.ExtractDataFromFileAsync(filePath);
        }

        public async Task<string> StartDocumentRetrieval(string filePath)
        {
            // Orchestrate the file retrieval by calling DocumentValidatorAgent
            var result = await _documentValidatorAgent.ExtractFileFromBlobAsync(filePath);
            return result;
        }
    }
}