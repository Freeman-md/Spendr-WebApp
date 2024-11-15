using Microsoft.Azure.Cosmos;

public class ExpenseRepository {
    private readonly Container _container;

    public ExpenseRepository(CosmosDBService cosmosDBService) {
        _container = cosmosDBService.GetDatabase().GetContainer("Expense");
    }

    
}