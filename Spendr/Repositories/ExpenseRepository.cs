using Microsoft.Azure.Cosmos;
using YourApp.Models;

public class ExpenseRepository {
    private readonly Container _container;

    public ExpenseRepository(CosmosDBService cosmosDBService) {
        _container = cosmosDBService.GetDatabase().GetContainer("Expense");
    }

    public async Task<Expense> GetExpense(string id, string partitionKey) {
        ItemResponse<Expense> response = await _container.ReadItemAsync<Expense>(
            id: id,
            partitionKey: new PartitionKey(partitionKey)
        );

        return response.Resource;
    }
}