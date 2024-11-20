using System;
using Microsoft.Extensions.Logging;
using Moq;
using Spendr.Contracts;
using Spendr.Models;
using Spendr.Pages;
using Spendr.Tests.Builders;

namespace Spendr.Tests.Pages;

public class IndexModelTests
{
    private readonly Mock<ILogger<IndexModel>> _logger;
    private readonly Mock<IExpenseRepository> _expenseRepositoryMock;

    public IndexModelTests()
    {
        _expenseRepositoryMock = new Mock<IExpenseRepository>();
        _logger = new Mock<ILogger<IndexModel>>();
    }

[   Fact]
    public async Task OnGetAsync_PopulatesExpenses()
    {
        #region Arrange
        List<Expense> expenses = ExpenseBuilder.BuildMany(3);
        _expenseRepositoryMock.Setup(repo => repo.GetAllExpenses())
                                .ReturnsAsync(expenses);

        IndexModel indexModel = new IndexModel(_logger.Object, _expenseRepositoryMock.Object);
        #endregion

        #region Act
        await indexModel.OnGet();
        #endregion

        #region Assert
        Assert.NotNull(indexModel.Expenses);
        Assert.Equal(expenses.Count, indexModel.Expenses.Count);
        #endregion
    }

}
