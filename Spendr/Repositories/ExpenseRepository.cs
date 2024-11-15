using System.Net;
using Microsoft.Azure.Cosmos;
using Spendr.Contracts;
using Spendr.Models;
using Spendr.Services;

namespace Spendr.Repositories;

public class ExpenseRepository : IExpenseRepository
{
    private readonly Container _container;
    private readonly ILogger<ExpenseRepository> _logger;

    public ExpenseRepository(ICosmosDBService cosmosDBService, ILogger<ExpenseRepository> logger)
    {
        _logger = logger;
        _container = cosmosDBService.GetOrCreateContainerAsync("Expenses", "/Username").Result;
    }

    public async Task<Expense?> GetExpense(string id, string partitionKey)
    {
        var response = await _container.ReadItemAsync<Expense>(
         id: id,
         partitionKey: new PartitionKey(partitionKey)
     );
        return response.Resource;
    }

    public async Task<List<Expense>> GetAllExpenses()
    {
        var query = _container.GetItemQueryIterator<Expense>("SELECT * FROM c");
        var results = new List<Expense>();

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response);
        }

        return results;
    }
}