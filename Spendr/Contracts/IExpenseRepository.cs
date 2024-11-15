using Spendr.Models;

namespace Spendr.Contracts;

public interface IExpenseRepository {
    Task<Expense?> GetExpense(string id, string partitionKey);
    Task<List<Expense>> GetAllExpenses();
}