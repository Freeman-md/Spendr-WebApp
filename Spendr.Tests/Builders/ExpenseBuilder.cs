using System;
using Spendr.Models;

namespace Spendr.Tests.Builders;

public class ExpenseBuilder
{
    private Expense _expense;

    public ExpenseBuilder()
    {
        // Default expense initialization
        _expense = new Expense
        {
            Username = Guid.NewGuid().ToString(),
            Description = "Default Expense Description",
            Amount = GenerateRandomAmount(100, 1000),
        };
    }

    public ExpenseBuilder WithId(string id) {
        _expense.Id = id;
        return this;
    }

    public ExpenseBuilder WithUsername(string username)
    {
        _expense.Username = username;
        return this;
    }

    public ExpenseBuilder WithDescription(string description) {
        _expense.Description = description;
        return this;
    }

    public ExpenseBuilder WithAmount(decimal amount) {
        _expense.Amount = amount;
        return this;
    }

    public Expense Build()
    {
        return _expense;
    }

    // Method to generate multiple expenses for testing
    public static List<Expense> BuildMany(int count)
    {
        var expenses = new List<Expense>();
        for (int i = 0; i < count; i++)
        {
            expenses.Add(new ExpenseBuilder()
                .WithUsername(Guid.NewGuid().ToString())
                .WithDescription(Guid.NewGuid().ToString())
                .WithAmount(GenerateRandomAmount(100, 1000))
                .Build());
        }
        return expenses;
    }

     public static decimal GenerateRandomAmount(decimal min, decimal max)
    {
        // Create a Random instance
        Random random = new Random();

        // Generate a random decimal within the range and round to 2 decimal places
        return Math.Round(
            (decimal)(random.NextDouble() * (double)(max - min) + (double)min),
            2
        );
    }
}

