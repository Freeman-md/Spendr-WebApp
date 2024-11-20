using System;
using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Moq;
using Spendr.Contracts;
using Spendr.Models;
using Spendr.Repositories;
using Spendr.Tests.Builders;

namespace Spendr.Tests.Repositories;

public class ExpenseRepositoryTests
{
    private readonly Mock<Container> _container;
    private readonly Mock<ILogger<ExpenseRepository>> _logger;
    private readonly Mock<ICosmosDBService> _cosmosDBService;

    private readonly IExpenseRepository _expenseRepository;

    public ExpenseRepositoryTests()
    {
        _container = new Mock<Container>();
        _logger = new Mock<ILogger<ExpenseRepository>>();
        _cosmosDBService = new Mock<ICosmosDBService>();

        _cosmosDBService.Setup(x => x.GetOrCreateContainerAsync("Expenses", "/Username"))
                        .ReturnsAsync(_container.Object);

        _expenseRepository = new ExpenseRepository(_cosmosDBService.Object, _logger.Object);
    }

    [Fact]
    public async Task GetExpense_WithId_AndPartitionKey_ShouldReturnExpense()
    {
        #region Arrange
        Expense expense = new ExpenseBuilder().Build();
        Mock<ItemResponse<Expense>> mockResponse = new Mock<ItemResponse<Expense>>();
        mockResponse.Setup(x => x.Resource).Returns(expense);

        _container
            .Setup(container => container.ReadItemAsync<Expense>(It.IsAny<string>(), It.IsAny<PartitionKey>(), null, default))
            .ReturnsAsync(mockResponse.Object);
        #endregion

        #region Act
        Expense? retrievedExpense = await _expenseRepository.GetExpense(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        #endregion

        #region Assert
        Assert.NotNull(retrievedExpense);

        Assert.Equal(expense.Id, retrievedExpense.Id);
        Assert.Equal(expense.Username, retrievedExpense.Username);
        Assert.Equal(expense.Amount, retrievedExpense.Amount);
        Assert.Equal(expense.Description, retrievedExpense.Description);
        #endregion
    }

    [Fact]
    public async Task GetExpense_WithInvalidId_AndPartitionKey_ShouldReturnNull()
    {
        #region Arrange
        Mock<ItemResponse<Expense>> mockResponse = new Mock<ItemResponse<Expense>>();
        mockResponse.Setup(x => x.Resource).Returns((Expense)null!);

        _container
            .Setup(container => container.ReadItemAsync<Expense>(It.IsAny<string>(), It.IsAny<PartitionKey>(), null, default))
            .ReturnsAsync(mockResponse.Object);
        #endregion

        #region Act
        Expense? retrievedExpense = await _expenseRepository.GetExpense(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        #endregion

        #region Assert
        Assert.Null(retrievedExpense);
        #endregion
    }

    [Theory]
    [InlineData("Not Found", HttpStatusCode.NotFound)]
    [InlineData("Database issue", HttpStatusCode.InternalServerError)]
    [InlineData("Unauthorized", HttpStatusCode.Unauthorized)]
    public async Task GetExpense_ShouldThrowCosmosException_WithExpectedStatusCode(string errorMessage, HttpStatusCode statusCode)
    {
        #region Arrange
        var exception = new CosmosException(errorMessage, statusCode, 0, "", 0);

        _container
            .Setup(container => container.ReadItemAsync<Expense>(It.IsAny<string>(), It.IsAny<PartitionKey>(), null, default))
            .ThrowsAsync(exception);
        #endregion

        #region Act & Assert
        var ex = await Assert.ThrowsAsync<CosmosException>(() =>
            _expenseRepository.GetExpense(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()));

        Assert.Equal(statusCode, ex.StatusCode);
        Assert.Equal(errorMessage, ex.Message);
        #endregion
    }

    [Fact]
    public async Task GetAllExpenses_ShouldReturnExpenses()
    {
        #region Arrange
        List<Expense> expenses = ExpenseBuilder.BuildMany(3);

        var mockFeedResponse = new Mock<FeedResponse<Expense>>();
        mockFeedResponse.Setup(x => x.GetEnumerator()).Returns(expenses.GetEnumerator());

        var mockFeedIterator = new Mock<FeedIterator<Expense>>();
        mockFeedIterator
            .SetupSequence(x => x.HasMoreResults)
            .Returns(true)
            .Returns(false);

        mockFeedIterator
            .Setup(x => x.ReadNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockFeedResponse.Object);

        _container
            .Setup(container => container.GetItemQueryIterator<Expense>("SELECT * FROM c", null, null))
            .Returns(mockFeedIterator.Object);
        #endregion

        #region Act
        var result = await _expenseRepository.GetAllExpenses();
        #endregion

        #region Assert
        Assert.NotNull(result);
        Assert.Equal(expenses.Count, result.Count);
        #endregion
    }

    [Fact]
    public async Task GetAllExpenses_WithLargeNumberOfItems_ShouldHandleCorrectly()
    {
        #region Arrange
        int TOTAL_NUMBER_OF_EXPENSES_CREATED = 1000;
        List<Expense> expenses = ExpenseBuilder.BuildMany(TOTAL_NUMBER_OF_EXPENSES_CREATED);

        var mockFeedResponse = new Mock<FeedResponse<Expense>>();
        mockFeedResponse.Setup(x => x.GetEnumerator()).Returns(expenses.GetEnumerator());

        var mockFeedIterator = new Mock<FeedIterator<Expense>>();
        mockFeedIterator
            .SetupSequence(x => x.HasMoreResults)
            .Returns(true)
            .Returns(false);

        mockFeedIterator
            .Setup(x => x.ReadNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockFeedResponse.Object);

        _container
            .Setup(container => container.GetItemQueryIterator<Expense>("SELECT * FROM c", null, null))
            .Returns(mockFeedIterator.Object);
        #endregion

        #region Act
        var result = await _expenseRepository.GetAllExpenses();
        #endregion

        #region Assert
        Assert.NotNull(result);
        Assert.Equal(TOTAL_NUMBER_OF_EXPENSES_CREATED, result.Count);
        #endregion
    }


    [Fact]
    public async Task GetAllExpenses_WhenNoExpenses_ShouldReturnEmptyList()
    {
        #region Arrange
        List<Expense> expenses = new List<Expense>();

        Mock<FeedResponse<Expense>> mockFeedResponse = new Mock<FeedResponse<Expense>>();
        mockFeedResponse.Setup(x => x.GetEnumerator()).Returns(expenses.GetEnumerator());

        Mock<FeedIterator<Expense>> mockFeedIterator = new Mock<FeedIterator<Expense>>();
        mockFeedIterator
            .SetupSequence(x => x.HasMoreResults)
            .Returns(true)
            .Returns(false);

        mockFeedIterator
            .Setup(x => x.ReadNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockFeedResponse.Object);

        _container
            .Setup(x => x.GetItemQueryIterator<Expense>("SELECT * FROM c", null, null))
            .Returns(mockFeedIterator.Object);
        #endregion

        #region Act
        List<Expense> retrievedExpenses = await _expenseRepository.GetAllExpenses();
        #endregion

        #region Assert
        Assert.NotNull(retrievedExpenses);
        Assert.Empty(retrievedExpenses);
        #endregion
    }

    [Fact]
    public async Task GetAllExpenses_WhenPartialSuccess_ShouldThrowError()
    {
        #region Arrange
        var expenses = ExpenseBuilder.BuildMany(3);

        var mockFeedResponse = new Mock<FeedResponse<Expense>>();
        mockFeedResponse.Setup(x => x.GetEnumerator()).Returns(expenses.GetEnumerator());

        var mockFeedIterator = new Mock<FeedIterator<Expense>>();
        mockFeedIterator
            .SetupSequence(x => x.HasMoreResults)
            .Returns(true)
            .Returns(true)
            .Returns(false);

        mockFeedIterator
            .SetupSequence(x => x.ReadNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockFeedResponse.Object) // First batch succeeds
            .ThrowsAsync(new CosmosException("Failed to fetch next batch", HttpStatusCode.InternalServerError, 0, "", 0)); // Second batch fails

        _container
            .Setup(container => container.GetItemQueryIterator<Expense>("SELECT * FROM c", null, null))
            .Returns(mockFeedIterator.Object);
        #endregion

        #region Act & Assert
        var ex = await Assert.ThrowsAsync<CosmosException>(() => _expenseRepository.GetAllExpenses());

        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
        #endregion
    }

}
