using KMC_Forge_BTL_Database.Interfaces;
using KMC_Forge_BTL_Models.DBModels;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KMC_Forge_BTL_Database.Repositories
{
    public class PortfolioUploadRepository : IPortfolioUploadRepository
    {
        private readonly IMongoCollection<PortfolioUploadResponse> _collection;

        public PortfolioUploadRepository(IMongoDatabase database)
        {
            var collectionName = KMC_Forge_BTL_Configurations.AppConfiguration.Instance.CosmosDBCollectionName;
            _collection = database.GetCollection<PortfolioUploadResponse>(collectionName);
        }

        public async Task<PortfolioUploadResponse> CreateAsync(PortfolioUploadResponse portfolioUpload)
        {
            portfolioUpload.CreatedAt = DateTime.UtcNow;
            portfolioUpload.UpdatedAt = DateTime.UtcNow;
            await _collection.InsertOneAsync(portfolioUpload);
            return portfolioUpload;
        }

        public async Task<PortfolioUploadResponse> GetByIdAsync(string id)
        {
            return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<PortfolioUploadResponse> GetByPortfolioIdAsync(string portfolioId)
        {
            return await _collection.Find(x => x.PortfolioId == portfolioId).FirstOrDefaultAsync();
        }

        public async Task<List<PortfolioUploadResponse>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<List<PortfolioUploadResponse>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var filter = Builders<PortfolioUploadResponse>.Filter.Gte(x => x.CreatedAt, startDate) &
                        Builders<PortfolioUploadResponse>.Filter.Lte(x => x.CreatedAt, endDate);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<bool> UpdateAsync(string id, PortfolioUploadResponse portfolioUpload)
        {
            portfolioUpload.UpdatedAt = DateTime.UtcNow;
            var result = await _collection.ReplaceOneAsync(x => x.Id == id, portfolioUpload);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _collection.DeleteOneAsync(x => x.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<long> GetCountAsync()
        {
            return await _collection.CountDocumentsAsync(_ => true);
        }
    }
}
