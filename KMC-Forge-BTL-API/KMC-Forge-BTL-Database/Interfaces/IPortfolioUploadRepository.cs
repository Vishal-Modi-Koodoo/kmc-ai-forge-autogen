using KMC_Forge_BTL_Models.DBModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KMC_Forge_BTL_Database.Interfaces
{
    public interface IPortfolioUploadRepository
    {
        Task<PortfolioUploadResponse> CreateAsync(PortfolioUploadResponse portfolioUpload);
        Task<PortfolioUploadResponse> GetByIdAsync(string id);
        Task<PortfolioUploadResponse> GetByPortfolioIdAsync(string portfolioId);
        Task<List<PortfolioUploadResponse>> GetAllAsync();
        Task<List<PortfolioUploadResponse>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<bool> UpdateAsync(string id, PortfolioUploadResponse portfolioUpload);
        Task<bool> DeleteAsync(string id);
        Task<long> GetCountAsync();
    }
}
