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
            var connectionString = KMC_Forge_BTL_Configurations.AppConfiguration.Instance.CosmosDBConnectionString;
            var databaseName = KMC_Forge_BTL_Configurations.AppConfiguration.Instance.CosmosDBDatabaseName;
            
            // Configure MongoDB client settings for Cosmos DB
            var settings = MongoClientSettings.FromConnectionString(connectionString);
            
            // Configure retry settings for Cosmos DB
            settings.MaxConnectionPoolSize = 100;
            settings.MinConnectionPoolSize = 10;
            settings.MaxConnectionIdleTime = TimeSpan.FromMinutes(10);
            settings.MaxConnectionLifeTime = TimeSpan.FromMinutes(30);
            
            // Configure server selection timeout
            settings.ServerSelectionTimeout = TimeSpan.FromSeconds(30);
            
            // Configure socket timeout
            settings.SocketTimeout = TimeSpan.FromSeconds(30);
            
            // Configure connection timeout
            settings.ConnectTimeout = TimeSpan.FromSeconds(30);
            
            // Configure retry writes (important for Cosmos DB)
            settings.RetryWrites = true;
            
            // Configure retry reads
            settings.RetryReads = true;
            
            // Configure write concern for Cosmos DB
            settings.WriteConcern = WriteConcern.WMajority;
            
            // Configure read preference
            settings.ReadPreference = ReadPreference.Primary;
            
            var client = new MongoClient(settings);
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
