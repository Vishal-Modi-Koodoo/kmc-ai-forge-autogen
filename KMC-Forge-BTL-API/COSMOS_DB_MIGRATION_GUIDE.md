# Azure Cosmos DB Migration Guide

This guide will help you migrate from local MongoDB to Azure Cosmos DB with MongoDB API.

## Prerequisites

1. Azure subscription
2. Azure CLI installed (optional, but recommended)
3. .NET 9.0 SDK

## Step 1: Create Azure Cosmos DB Account

### Option A: Using Azure Portal

1. Go to [Azure Portal](https://portal.azure.com)
2. Click "Create a resource"
3. Search for "Azure Cosmos DB"
4. Select "Azure Cosmos DB" and click "Create"
5. Choose "MongoDB" as the API
6. Fill in the required details:
   - **Subscription**: Your Azure subscription
   - **Resource Group**: Create new or use existing
   - **Account Name**: `kmc-forge-btl-cosmos` (or your preferred name)
   - **Location**: Choose a region close to your application
   - **Capacity mode**: Choose "Provisioned throughput" for predictable costs or "Serverless" for development
   - **Apply Free Tier Discount**: Yes (if available)
7. Click "Review + create" and then "Create"

### Option B: Using Azure CLI

```bash
# Login to Azure
az login

# Create resource group (if not exists)
az group create --name kmc-forge-btl-rg --location eastus

# Create Cosmos DB account
az cosmosdb create \
  --name kmc-forge-btl-cosmos \
  --resource-group kmc-forge-btl-rg \
  --kind MongoDB \
  --capabilities EnableMongo \
  --locations regionName=eastus failoverPriority=0 isZoneRedundant=false \
  --default-consistency-level Session \
  --enable-free-tier true
```

## Step 2: Create Database and Collection

### Using Azure Portal

1. Go to your Cosmos DB account
2. Click on "Data Explorer"
3. Click "New Database"
4. Enter:
   - **Database id**: `KMCForgeBTL`
   - **Provision throughput**: Yes
   - **Throughput**: 400 RU/s (minimum for shared throughput)
5. Click "OK"
6. Click on the database you just created
7. Click "New Collection"
8. Enter:
   - **Collection id**: `PortfolioUploads`
   - **Storage capacity**: Fixed (10 GB)
   - **Throughput**: 400 RU/s
9. Click "OK"

### Using Azure CLI

```bash
# Create database
az cosmosdb mongodb database create \
  --account-name kmc-forge-btl-cosmos \
  --resource-group kmc-forge-btl-rg \
  --name KMCForgeBTL

# Create collection
az cosmosdb mongodb collection create \
  --account-name kmc-forge-btl-cosmos \
  --resource-group kmc-forge-btl-rg \
  --database-name KMCForgeBTL \
  --name PortfolioUploads \
  --throughput 400
```

## Step 3: Get Connection String

### Using Azure Portal

1. Go to your Cosmos DB account
2. Click on "Keys" in the left menu
3. Copy the "Primary Connection String"
4. The connection string will look like:
   ```
   mongodb://kmc-forge-btl-cosmos:YOUR_PRIMARY_KEY@kmc-forge-btl-cosmos.mongo.cosmos.azure.com:10255/?ssl=true&replicaSet=globaldb&retrywrites=false&maxIdleTimeMS=120000&appName=@kmc-forge-btl-cosmos@
   ```

### Using Azure CLI

```bash
az cosmosdb keys list \
  --name kmc-forge-btl-cosmos \
  --resource-group kmc-forge-btl-rg \
  --type connection-strings
```

## Step 4: Update Configuration

1. Replace `YOUR_COSMOS_DB_CONNECTION_STRING_HERE` in both configuration files:
   - `KMC-Forge-BTL-API/appsettings.json`
   - `KMC-Forge-BTL-API/appsettings.Development.json`

2. The configuration should look like this:
   ```json
   {
     "CosmosDB": {
       "ConnectionString": "mongodb://kmc-forge-btl-cosmos:YOUR_PRIMARY_KEY@kmc-forge-btl-cosmos.mongo.cosmos.azure.com:10255/?ssl=true&replicaSet=globaldb&retrywrites=false&maxIdleTimeMS=120000&appName=@kmc-forge-btl-cosmos@",
       "DatabaseName": "KMCForgeBTL",
       "CollectionName": "PortfolioUploads"
     }
   }
   ```

## Step 5: Test the Connection

1. Build and run your application:
   ```bash
   cd KMC-Forge-BTL-API
   dotnet build
   dotnet run
   ```

2. Test an API endpoint that uses the database to ensure the connection works.

## Step 6: Data Migration (if needed)

If you have existing data in your local MongoDB, you can migrate it using MongoDB tools:

### Export from local MongoDB
```bash
mongodump --db KMCForgeBTL --out ./backup
```

### Import to Cosmos DB
```bash
mongorestore --uri "YOUR_COSMOS_DB_CONNECTION_STRING" --db KMCForgeBTL ./backup/KMCForgeBTL
```

## Important Notes

### Connection String Security
- Never commit connection strings with keys to source control
- Use Azure Key Vault or environment variables for production
- Consider using managed identity for production applications

### Performance Considerations
- Cosmos DB uses Request Units (RU) for billing
- Monitor RU consumption in Azure Portal
- Consider using serverless mode for development/testing
- Use appropriate indexes for better performance

### Consistency Levels
- The application is configured to use Session consistency
- This provides strong consistency within a session
- Consider your application's consistency requirements

### Retry Policies
- The application includes retry policies configured for Cosmos DB
- These handle transient failures automatically
- Monitor retry patterns in application logs

## Troubleshooting

### Common Issues

1. **Connection Timeout**
   - Check if the connection string is correct
   - Verify network connectivity
   - Check if the Cosmos DB account is accessible

2. **Authentication Errors**
   - Verify the primary key is correct
   - Check if the key has expired
   - Ensure the account name is correct

3. **Performance Issues**
   - Monitor RU consumption
   - Check if you're hitting throughput limits
   - Consider scaling up the database

### Monitoring

1. Use Azure Monitor to track:
   - Request units consumed
   - Data usage
   - Availability
   - Latency

2. Set up alerts for:
   - High RU consumption
   - Connection failures
   - Data usage approaching limits

## Cost Optimization

1. **Use Serverless Mode** for development/testing
2. **Monitor RU Consumption** regularly
3. **Use Appropriate Indexes** to reduce RU usage
4. **Consider Autoscale** for variable workloads
5. **Use Free Tier** if available

## Next Steps

1. Test all database operations
2. Monitor performance and costs
3. Set up proper monitoring and alerting
4. Consider implementing data backup strategies
5. Review security best practices
