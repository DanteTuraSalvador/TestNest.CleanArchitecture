using TestNest.Admin.SharedLibrary.Common.Guards;
using TestNest.Admin.SharedLibrary.Common.Results;

namespace TestNest.Admin.SharedLibrary.Test.Common.Guards;

public class GuardTests
{
    private static Exception CreateTestException() => new InvalidOperationException("Test exception");

    [Fact]
    public void Against_WithValidCondition_ReturnsSuccess()
    {
        static bool validation() => true;

        Result result = Guard.Against(validation, CreateTestException);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Against_WithInvalidCondition_ReturnsFailure()
    {
        static bool validation() => false;

        Result result = Guard.Against(validation, CreateTestException);

        Assert.False(result.IsSuccess);

        Exceptions.Common.Error firstError = result.Errors[0];

        Assert.Equal("Test exception", firstError.Message);
    }

    [Fact]
    public void Against_WithThrowingValidation_ReturnsFailureWithWrappedException()
    {
        static bool validation() => throw new ArgumentException("Inner exception");

        Result result = Guard.Against(validation, CreateTestException);

        Assert.False(result.IsSuccess);

        Assert.Contains("Validation failed", result.Errors[0].Message);
    }

    [Fact]
    public void AgainstNull_WithNullValue_ReturnsFailure()
    {
        object? nullObject = null;

        Result result = Guard.AgainstNull(nullObject, CreateTestException);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void AgainstNull_WithNonNullValue_ReturnsSuccess()
    {
        object validObject = new();

        Result result = Guard.AgainstNull(validObject, CreateTestException);

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AgainstNullOrWhiteSpace_WithInvalidValues_ReturnsFailure(string value)
    {
        Result result = Guard.AgainstNullOrWhiteSpace(value, CreateTestException);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void AgainstNullOrWhiteSpace_WithValidValue_ReturnsSuccess()
    {
        string validString = "valid";

        Result result = Guard.AgainstNullOrWhiteSpace(validString, CreateTestException);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void AgainstLength_WithCorrectLength_ReturnsSuccess()
    {
        string value = "12345";
        int requiredLength = 5;

        Result result = Guard.AgainstLength(value, requiredLength, CreateTestException);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void AgainstLength_WithIncorrectLength_ReturnsFailure()
    {
        string value = "12345";
        int requiredLength = 4;

        Result result = Guard.AgainstLength(value, requiredLength, CreateTestException);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void AgainstRegex_WithMatchingPattern_ReturnsSuccess()
    {
        string value = "test@example.com";
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        Result result = Guard.AgainstRegex(value, pattern, CreateTestException);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void AgainstRegex_WithNonMatchingPattern_ReturnsFailure()
    {
        string value = "invalid-email";
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        Result result = Guard.AgainstRegex(value, pattern, CreateTestException);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void AgainstInvalidInput_WithValidInput_ReturnsSuccess()
    {
        int value = 10;
        static bool validation(int x) => x > 5;

        Result result = Guard.AgainstInvalidInput(value, validation, CreateTestException);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void AgainstInvalidInput_WithInvalidInput_ReturnsFailure()
    {
        int value = 3;
        static bool validation(int x) => x > 5;

        Result result = Guard.AgainstInvalidInput(value, validation, CreateTestException);

        Assert.False(result.IsSuccess);
    }

    [Theory]
    [InlineData(5, 1, 10)]
    [InlineData(1, 1, 10)]
    [InlineData(10, 1, 10)]
    public void AgainstRange_WithValueInRange_ReturnsSuccess(int value, int min, int max)
    {
        Result result = Guard.AgainstRange(value, min, max, CreateTestException);

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData(0, 1, 10)]
    [InlineData(11, 1, 10)]
    public void AgainstRange_WithValueOutOfRange_ReturnsFailure(int value, int min, int max)
    {
        Result result = Guard.AgainstRange(value, min, max, CreateTestException);

        Assert.False(result.IsSuccess);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public void AgainstNegative_WithNonNegativeValue_ReturnsSuccess(decimal value)
    {
        Result result = Guard.AgainstNegative(value, CreateTestException);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void AgainstNegative_WithNegativeValue_ReturnsFailure()
    {
        decimal value = -0.1m;

        Result result = Guard.AgainstNegative(value, CreateTestException);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void AgainstNonPositive_WithPositiveValue_ReturnsSuccess()
    {
        decimal value = 0.1m;

        Result result = Guard.AgainstNonPositive(value, CreateTestException);

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AgainstNonPositive_WithNonPositiveValue_ReturnsFailure(decimal value)
    {
        Result result = Guard.AgainstNonPositive(value, CreateTestException);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void AgainstEmptyCollection_WithNonEmptyCollection_ReturnsSuccess()
    {
        var collection = new List<int> { 1, 2, 3 };

        Result result = Guard.AgainstEmptyCollection(collection, CreateTestException);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void AgainstEmptyCollection_WithEmptyCollection_ReturnsFailure()
    {
        var collection = new List<int>();

        Result result = Guard.AgainstEmptyCollection(collection, CreateTestException);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void AgainstEmptyCollection_WithNullCollection_ReturnsFailure()
    {
        List<int>? collection = null;

        Result result = Guard.AgainstEmptyCollection(collection, CreateTestException);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void AgainstPastDate_WithFutureDate_ReturnsSuccess()
    {
        DateTime futureDate = DateTime.UtcNow.AddDays(1);

        Result result = Guard.AgainstPastDate(futureDate, CreateTestException);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void AgainstPastDate_WithPastDate_ReturnsFailure()
    {
        DateTime pastDate = DateTime.UtcNow.AddDays(-1);

        Result result = Guard.AgainstPastDate(pastDate, CreateTestException);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void AgainstFutureDate_WithPastDate_ReturnsSuccess()
    {
        DateTime pastDate = DateTime.UtcNow.AddDays(-1);

        Result result = Guard.AgainstFutureDate(pastDate, CreateTestException);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void AgainstFutureDate_WithFutureDate_ReturnsFailure()
    {
        DateTime futureDate = DateTime.UtcNow.AddDays(1);

        Result result = Guard.AgainstFutureDate(futureDate, CreateTestException);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void AgainstCondition_WithFalseCondition_ReturnsSuccess()
    {
        bool invalidCondition = false;

        Result result = Guard.AgainstCondition(invalidCondition, CreateTestException);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void AgainstCondition_WithTrueCondition_ReturnsFailure()
    {
        bool invalidCondition = true;

        Result result = Guard.AgainstCondition(invalidCondition, CreateTestException);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Aggregate_WithAllSuccessResults_ReturnsSuccess()
    {
        Result[] results =
        [
                Result.Success(),
                Result.Success(),
                Result.Success()
            ];

        Result result = Guard.Aggregate(results);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Aggregate_WithAnyFailure_ReturnsFailure()
    {
        Result[] results =
        [
                Result.Success(),
                Result.Failure(CreateTestException()),
                Result.Success()
            ];

        Result result = Guard.Aggregate(results);

        Assert.False(result.IsSuccess);
        _ = Assert.Single(result.Errors);
    }

    [Fact]
    public void Aggregate_WithMultipleFailures_ReturnsCombinedFailure()
    {
        Result[] results =
        [
                Result.Failure(CreateTestException()),
                Result.Failure(new ArgumentException("Arg exception")),
                Result.Success()
            ];

        Result result = Guard.Aggregate(results);

        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.Errors.Count);
    }
}
