using Microsoft.Azure.Cosmos;
using Spendr.Contracts;

namespace Spendr.Services;

public class CosmosDBService : ICosmosDBService
{
    private readonly CosmosClient _client;
    private readonly string _databaseName;
    private readonly ILogger<ICosmosDBService> _logger;


    public CosmosDBService(CosmosClient client, string databaseName, ILogger<ICosmosDBService> logger)
    {
        _client = client;
        _databaseName = databaseName;
        _logger = logger;
    }

    public async Task<Database> GetOrCreateDatabaseAsync()
    {
        try
        {
            var response = await _client.CreateDatabaseIfNotExistsAsync(_databaseName);
            _logger.LogInformation("Database '{DatabaseName}' is ready.", _databaseName);
            return response.Database;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create or access the database '{DatabaseName}'.", _databaseName);
            throw;
        }
    }

    public async Task<Container> GetOrCreateContainerAsync(string containerName, string partitionKeyPath)
    {
        try
        {
            var database = await GetOrCreateDatabaseAsync();
            var response = await database.CreateContainerIfNotExistsAsync(new ContainerProperties
            {
                Id = containerName,
                PartitionKeyPath = partitionKeyPath
            });
            _logger.LogInformation("Container '{ContainerName}' is ready in database '{DatabaseName}'.", containerName, _databaseName);
            return response.Container;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create or access the container '{ContainerName}' in database '{DatabaseName}'.", containerName, _databaseName);
            throw;
        }
    }

}