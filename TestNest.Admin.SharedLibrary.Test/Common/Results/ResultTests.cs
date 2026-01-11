using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Exceptions;
using TestNest.Admin.SharedLibrary.Exceptions.Common;

namespace TestNest.Admin.SharedLibrary.Test.Common.Results;

public class ResultTests
{
    [Fact]
    public void Success_ReturnsSuccessfulResultWithNoErrorTypeAndEmptyErrors()
    {
        var result = Result.Success();
        Assert.True(result.IsSuccess);
        Assert.Equal(ErrorType.None, result.ErrorType);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Failure_WithError_ReturnsFailedResultWithCorrectErrorTypeAndSingleError()
    {
        var error = new Error("100", "Test Error");
        var result = Result.Failure(ErrorType.Validation, error);
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        _ = Assert.Single(result.Errors);
        Assert.Equal(error, result.Errors[0]);
    }

    [Fact]
    public void Failure_WithErrorTypeNone_ThrowsResultException()
    {
        var error = new Error("100", "Test Error");
        ResultException exception = Assert.Throws<ResultException>(() => Result.Failure(ErrorType.None, error));
        Assert.Equal(typeof(Result), exception.ResultType);
        _ = Assert.Single(exception.Errors);
        Assert.Equal("Invalid error type.", exception.Errors[0]);
    }

    [Fact]
    public void Failure_WithErrorWithInvalidCodeOrMessage_ThrowsResultException()
    {
        ResultException exceptionNullCode = Assert.Throws<ResultException>(() => Result.Failure(ErrorType.Validation, new Error(null!, "Test")));
        Assert.Equal(typeof(Error), exceptionNullCode.ResultType);
        _ = Assert.Single(exceptionNullCode.Errors);
        Assert.Equal("Error code cannot be null or empty.", exceptionNullCode.Errors[0]);

        ResultException exceptionNullMessage = Assert.Throws<ResultException>(() => Result.Failure(ErrorType.Validation, new Error("100", null!)));
        Assert.Equal(typeof(Error), exceptionNullMessage.ResultType);
        _ = Assert.Single(exceptionNullMessage.Errors);
        Assert.Equal("Error message cannot be null or empty.", exceptionNullMessage.Errors[0]);
    }

    [Fact]
    public void Failure_WithMultipleErrors_ReturnsFailedResultWithCorrectErrorTypeAndMultipleErrors()
    {
        var error1 = new Error("200", "Error 1");
        var error2 = new Error("300", "Error 2");
        var errors = new List<Error> { error1, error2 };
        var result = Result.Failure(ErrorType.NotFound, errors);
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(error1, result.Errors);
        Assert.Contains(error2, result.Errors);
    }

    [Fact]
    public void Failure_WithMultipleErrorsAndErrorTypeNone_ThrowsResultException()
    {
        var error1 = new Error("200", "Error 1");
        var error2 = new Error("300", "Error 2");
        var errors = new List<Error> { error1, error2 };
        ResultException exception = Assert.Throws<ResultException>(() => Result.Failure(ErrorType.None, errors));
        Assert.Equal(typeof(Result), exception.ResultType);
        _ = Assert.Single(exception.Errors);
        Assert.Equal("Invalid error type.", exception.Errors[0]);
    }

    [Fact]
    public void Failure_WithNullErrors_ThrowsArgumentNullException() => Assert.Throws<ArgumentNullException>(() => Result.Failure(ErrorType.Internal, (IEnumerable<Error>)null!));

    [Fact]
    public void Failure_WithEmptyErrors_ThrowsResultException()
    {
        ResultException exception = Assert.Throws<ResultException>(() => Result.Failure(ErrorType.Conflict, Enumerable.Empty<Error>()));
        Assert.Equal(typeof(Result), exception.ResultType);
        _ = Assert.Single(exception.Errors);
        Assert.Equal("Error list cannot be empty.", exception.Errors[0]);
    }

    [Fact]
    public void Failure_WithMultipleErrorsHavingInvalidCodeOrMessage_ThrowsResultException()
    {
        var errorsWithNullCode = new List<Error> { new(null!, "Test"), new("100", "Valid") };
        ResultException exceptionNullCode = Assert.Throws<ResultException>(() => Result.Failure(ErrorType.Validation, errorsWithNullCode));
        Assert.Equal(typeof(Error), exceptionNullCode.ResultType);
        _ = Assert.Single(exceptionNullCode.Errors);
        Assert.Equal("Error code cannot be null or empty.", exceptionNullCode.Errors[0]);

        var errorsWithNullMessage = new List<Error> { new("100", null!), new("200", "Valid") };
        ResultException exceptionNullMessage = Assert.Throws<ResultException>(() => Result.Failure(ErrorType.Validation, errorsWithNullMessage));
        Assert.Equal(typeof(Error), exceptionNullMessage.ResultType);
        _ = Assert.Single(exceptionNullMessage.Errors);
        Assert.Equal("Error message cannot be null or empty.", exceptionNullMessage.Errors[0]);
    }

    [Fact]
    public void Failure_WithCodeAndMessage_ReturnsFailedResultWithSingleError()
    {
        var result = Result.Failure(ErrorType.Validation, "400", "Invalid Input");
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        _ = Assert.Single(result.Errors);
        Assert.Equal("400", result.Errors[0].Code);
        Assert.Equal("Invalid Input", result.Errors[0].Message);
    }

    [Fact]
    public void Combine_AllSuccess_ReturnsSuccess()
    {
        var result = Result.Combine(Result.Success(), Result.Success(), Result.Success());
        Assert.True(result.IsSuccess);
        Assert.Equal(ErrorType.None, result.ErrorType);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Combine_OneFailure_ReturnsFailureWithAggregateErrorTypeAndAllErrors()
    {
        var error = new Error("500", "Combined Error");
        var failureResult = Result.Failure(ErrorType.Invalid, error);
        var result = Result.Combine(Result.Success(), failureResult, Result.Success());
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Aggregate, result.ErrorType);
        _ = Assert.Single(result.Errors);
        Assert.Equal(error, result.Errors[0]);
    }

    [Fact]
    public void Combine_MultipleFailures_ReturnsFailureWithAggregateErrorTypeAndAllErrorsCombined()
    {
        var error1 = new Error("600", "Combined Error 1");
        var error2 = new Error("700", "Combined Error 2");
        var failureResult1 = Result.Failure(ErrorType.Validation, error1);
        var failureResult2 = Result.Failure(ErrorType.NotFound, error2);
        var result = Result.Combine(failureResult1, Result.Success(), failureResult2);
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Aggregate, result.ErrorType);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(error1, result.Errors);
        Assert.Contains(error2, result.Errors);
    }

    [Fact]
    public void Combine_EmptyArray_ReturnsSuccess()
    {
        var result = Result.Combine(Array.Empty<Result>());
        Assert.True(result.IsSuccess);
        Assert.Equal(ErrorType.None, result.ErrorType);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ToResultOfT_OnSuccess_ReturnsSuccessResultOfTWithValue()
    {
        var successResult = Result.Success();
        var resultOfT = successResult.ToResult("test value");
        Assert.True(resultOfT.IsSuccess);
        Assert.Equal("test value", resultOfT.Value);
        Assert.Equal(ErrorType.None, resultOfT.ErrorType);
        Assert.Empty(resultOfT.Errors);
    }

    [Fact]
    public void ToResultOfT_OnFailure_ReturnsFailureResultOfTWithSameErrorTypeAndErrors()
    {
        var error = new Error("800", "Failure Value");
        var failureResult = Result.Failure(ErrorType.Conflict, error);
        var resultOfT = failureResult.ToResult("some value");
        Assert.False(resultOfT.IsSuccess);
        Assert.Equal(ErrorType.Conflict, resultOfT.ErrorType);
        _ = Assert.Single(resultOfT.Errors);
        Assert.Equal(error, resultOfT.Errors[0]);
        Assert.Null(resultOfT.Value);
    }

    [Fact]
    public void Bind_OnSuccess_ExecutesBindAndReturnsItsResult()
    {
        var successResult = Result.Success();
        Result bindResult = successResult.Bind(() => Result.Failure(ErrorType.Validation, new Error("900", "Bind Failure")));
        Assert.False(bindResult.IsSuccess);
        Assert.Equal(ErrorType.Validation, bindResult.ErrorType);
        _ = Assert.Single(bindResult.Errors);
        Assert.Equal("900", bindResult.Errors[0].Code);
    }

    [Fact]
    public void Bind_OnFailure_DoesNotExecuteBindAndReturnsOriginalFailure()
    {
        var error = new Error("1000", "Original Failure");
        var failureResult = Result.Failure(ErrorType.NotFound, error);
        bool bindExecuted = false;
        Result bindResult = failureResult.Bind(() => { bindExecuted = true; return Result.Success(); });
        Assert.False(bindResult.IsSuccess);
        Assert.Equal(ErrorType.NotFound, bindResult.ErrorType);
        _ = Assert.Single(bindResult.Errors);
        Assert.Equal(error, bindResult.Errors[0]);
        Assert.False(bindExecuted);
    }

    [Fact]
    public void BindOfT_OnSuccess_ExecutesBindAndReturnsItsResult()
    {
        var successResult = Result.Success();
        Result<string> bindResult = successResult.Bind(() => Result<string>.Success("bound value"));
        Assert.True(bindResult.IsSuccess);
        Assert.Equal("bound value", bindResult.Value);
        Assert.Equal(ErrorType.None, bindResult.ErrorType);
        Assert.Empty(bindResult.Errors);
    }

    [Fact]
    public void BindOfT_OnFailure_DoesNotExecuteBindAndReturnsFailureResultOfT()
    {
        var error = new Error("1100", "Original Failure");
        var failureResult = Result.Failure(ErrorType.Conflict, error);
        bool bindExecuted = false;
        Result<string> bindResult = failureResult.Bind(() => { bindExecuted = true; return Result<string>.Success("bound value"); });
        Assert.False(bindResult.IsSuccess);
        Assert.Equal(ErrorType.Conflict, bindResult.ErrorType);
        _ = Assert.Single(bindResult.Errors);
        Assert.Equal(error, bindResult.Errors[0]);
        Assert.Null(bindResult.Value);
        Assert.False(bindExecuted);
    }

    [Fact]
    public void Map_OnSuccess_ExecutesMap()
    {
        var successResult = Result.Success();
        bool mapExecuted = false;
        Result<bool> mappedResult = successResult.Map(() => mapExecuted = true);
        Assert.True(mappedResult.IsSuccess);
        Assert.Equal(ErrorType.None, mappedResult.ErrorType);
        Assert.Empty(mappedResult.Errors);
        Assert.True(mapExecuted);
    }

    [Fact]
    public void Map_OnFailure_DoesNotExecuteMap()
    {
        var error = new Error("1200", "Failure");
        var failureResult = Result.Failure(ErrorType.Validation, error);
        bool mapExecuted = false;
        Result<bool> mappedResult = failureResult.Map(() => mapExecuted = true);
        Assert.False(mappedResult.IsSuccess);
        Assert.Equal(ErrorType.Validation, mappedResult.ErrorType);
        _ = Assert.Single(mappedResult.Errors);
        Assert.Equal(error, mappedResult.Errors[0]);
        Assert.False(mapExecuted);
    }

    [Fact]
    public void MapOfT_OnSuccess_ExecutesMapAndReturnsSuccessResultOfT()
    {
        var successResult = Result.Success();
        Result<string> mappedResult = successResult.Map(() => "mapped value");
        Assert.True(mappedResult.IsSuccess);
        Assert.Equal("mapped value", mappedResult.Value);
        Assert.Equal(ErrorType.None, mappedResult.ErrorType);
        Assert.Empty(mappedResult.Errors);
    }

    [Fact]
    public void MapOfT_OnFailure_DoesNotExecuteMapAndReturnsFailureResultOfT()
    {
        var error = new Error("1300", "Failure");
        var failureResult = Result.Failure(ErrorType.NotFound, error);
        bool mapExecuted = false;
        Result<string> mappedResult = failureResult.Map(() => { mapExecuted = true; return "mapped value"; });
        Assert.False(mappedResult.IsSuccess);
        Assert.Null(mappedResult.Value);
        Assert.Equal(ErrorType.NotFound, mappedResult.ErrorType);
        _ = Assert.Single(mappedResult.Errors);
        Assert.Equal(error, mappedResult.Errors[0]);
        Assert.False(mapExecuted);
    }

    // You can add similar tests for the async versions (BindAsync, MapAsync)

    [Fact]
    public void Deconstruct_SuccessResult_ReturnsCorrectValues()
    {
        var result = Result.Success();
        (bool isSuccess, ErrorType errorType, IReadOnlyList<Error> errors) = result;
        Assert.True(isSuccess);
        Assert.Equal(ErrorType.None, errorType);
        Assert.Empty(errors);
    }

    [Fact]
    public void Deconstruct_FailureResult_ReturnsCorrectValues()
    {
        var error = new Error("1400", "Deconstruct Error");
        var result = Result.Failure(ErrorType.Conflict, error);
        (bool isSuccess, ErrorType errorType, IReadOnlyList<Error> errors) = result;
        Assert.False(isSuccess);
        Assert.Equal(ErrorType.Conflict, errorType);
        _ = Assert.Single(errors);
        Assert.Equal(error, errors[0]);
    }

    [Fact]
    public void Failure_WithException_ReturnsFailedResultWithErrorBasedOnExceptionType()
    {
        var exception = new InvalidOperationException("Test Exception with no code");
        var result = Result.Failure(exception);
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        _ = Assert.Single(result.Errors);
        Assert.Equal("InvalidOperationException", result.Errors[0].Code);
        Assert.Equal("Test Exception with no code", result.Errors[0].Message);
    }

    // Assuming you have an IHasErrorCode interface and an exception implementing it
    public interface IHasErrorCode
    {
        string Code { get; }
        string Message { get; }
    }

    public class TestErrorCodeException(string code, string message) : Exception(message), Exceptions.Common.IHasErrorCode
    {
        public string Code { get; } = code;
        public override string Message { get; } = message;
    }

    [Fact]
    public void Failure_WithExceptionImplementingIHasErrorCode_ReturnsFailedResultWithErrorUsingErrorCode()
    {
        var exception = new TestErrorCodeException("ERR-001", "Test Exception with code");
        var result = Result.Failure(exception);
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        _ = Assert.Single(result.Errors);
        Assert.Equal("ERR-001", result.Errors[0].Code);
        Assert.Equal("Test Exception with code", result.Errors[0].Message);
    }
}
