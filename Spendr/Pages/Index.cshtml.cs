using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Azure.Cosmos;
using Spendr.Contracts;
using Spendr.Models;

namespace Spendr.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IExpenseRepository _expenseRepository;

    public List<Expense> Expenses { get; private set; } = new List<Expense>();


    public IndexModel(ILogger<IndexModel> logger, IExpenseRepository expenseRepository)
    {
        _logger = logger;
        _expenseRepository = expenseRepository;
    }

    public async Task OnGet()
    {
        try
        {
            Expenses = await _expenseRepository.GetAllExpenses();
            _logger.LogInformation("Expenses successfully retrieved.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve expenses.");
            TempData["Error"] = "Unable to load expenses. Please try again later.";
        }
    }
}
