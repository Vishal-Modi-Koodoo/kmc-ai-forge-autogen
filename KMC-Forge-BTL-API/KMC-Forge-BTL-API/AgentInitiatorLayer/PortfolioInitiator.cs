using KMC_Forge_BTL_Core_Agent.Agents;
using KMC_Forge_BTL_Models.PDFExtractorResponse;
using Microsoft.Extensions.Configuration;

namespace KMC_AI_Forge_BTL_Agent.AgentInitiatorLayer;

public class PortfolioInitiator
{
    private readonly IConfiguration _configuration;

    public PortfolioInitiator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // public async Task<CompanyInfo> GetCompanyInfoAsync(string filePath){
    //     LeadPortfolioAgent leadPortfolioAgent = new LeadPortfolioAgent(_configuration);
    //     var companyInfo = await leadPortfolioAgent.StartProcessing(filePath, );
        
    //     return companyInfo;
    // }

    // public async Task<CompanyInfo> ValidatePortfolioFormAsync(string filePath)
    // {

    // }
}