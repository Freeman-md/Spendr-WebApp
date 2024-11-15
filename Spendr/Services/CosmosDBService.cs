using Microsoft.Azure.Cosmos;

public class CosmosDBService {
    private readonly CosmosClient _client;
    private readonly Database _database;

    public CosmosDBService(CosmosClient client, string databaseName) {
        _client = client;

        _database = client.GetDatabase(databaseName);
    }

        public Database GetDatabase() => _database;

}