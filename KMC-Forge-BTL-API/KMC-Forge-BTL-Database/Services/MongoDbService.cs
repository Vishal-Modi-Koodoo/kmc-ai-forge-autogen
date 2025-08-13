using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;

namespace KMC_Forge_BTL_Database.Services
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;

        public MongoDbService(IConfiguration configuration)
        {
            var connectionString = KMC_Forge_BTL_Configurations.AppConfiguration.Instance.MongoDBConnectionString;
            var databaseName = KMC_Forge_BTL_Configurations.AppConfiguration.Instance.MongoDBDatabaseName;
            
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoDatabase GetDatabase()
        {
            return _database;
        }

        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return _database.GetCollection<T>(collectionName);
        }
    }
}
