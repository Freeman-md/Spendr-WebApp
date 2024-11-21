using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Spendr.Services;
using Xunit;

public class CosmosDBServiceIntegrationTests : TestConfig
{
    private readonly CosmosClient _cosmosClient;
    private readonly string _testDatabase;
    private readonly string _testContainer;
    private readonly ILogger<CosmosDBService> _logger;

    public CosmosDBServiceIntegrationTests() : base()
    {
        var endpoint = _config.GetValue<string>("CosmosDB:Endpoint");
        _testDatabase = _config.GetValue<string>("CosmosDB:Database");
        _testContainer = "Expenses";

        _cosmosClient = new CosmosClient(endpoint);
        _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<CosmosDBService>();
    }

    [Fact]
    public async Task GetOrCreateDatabaseAsync_ShouldCreateOrReturnDatabase()
    {
        // Arrange
        var cosmosDBService = new CosmosDBService(_cosmosClient, _testDatabase, _logger);

        // Act
        var database = await cosmosDBService.GetOrCreateDatabaseAsync();

        // Assert
        Assert.NotNull(database);
        Assert.Equal(_testDatabase, database.Id);
    }


    [Fact]
    public async Task GetOrCreateContainerAsync_ShouldCreateOrReturnContainer()
    {
        // Arrange
        var cosmosDBService = new CosmosDBService(_cosmosClient, _testDatabase, _logger);

        // Act
        var container = await cosmosDBService.GetOrCreateContainerAsync(_testContainer, "/Username");

        // Assert
        Assert.NotNull(container);
        Assert.Equal(_testContainer, container.Id);
    }

}
