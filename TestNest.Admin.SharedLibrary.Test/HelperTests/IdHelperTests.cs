using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Exceptions;
using TestNest.Admin.SharedLibrary.Exceptions.Common;
using TestNest.Admin.SharedLibrary.Helpers;
using TestNest.Admin.SharedLibrary.StronglyTypeIds.Common;

namespace TestNest.Admin.SharedLibrary.Test.HelperTests;

public class IdHelperTests
{
    [Fact]
    public void ValidateAndCreateId_ValidGuidString_ReturnsSuccessResultWithId()
    {
        string validGuidString = Guid.NewGuid().ToString();

        Result<TestId> result = IdHelper.ValidateAndCreateId<TestId>(validGuidString);

        Assert.True(result.IsSuccess);
        Assert.Equal(Guid.Parse(validGuidString), result.Value!.Value);
    }

    [Fact]
    public void ValidateAndCreateId_InvalidGuidString_ReturnsFailureResultWithInvalidGuidError()
    {
        string invalidGuidString = "not-a-guid";

        Result<TestId> result = IdHelper.ValidateAndCreateId<TestId>(invalidGuidString);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        _ = Assert.Single(result.Errors);
        Assert.Equal(IdHelper.InvalidGuidFormatErrorCode, result.Errors[0].Code);
        Assert.Equal(IdValidationException.InvalidGuidFormat().Message, result.Errors[0].Message);
    }

    [Fact]
    public void ValidateAndCreateId_EmptyGuidString_ReturnsFailureResultWithNullIdErrorFromCreateMethod()
    {
        string emptyGuidString = Guid.Empty.ToString();

        Result<TestId> result = IdHelper.ValidateAndCreateId<TestId>(emptyGuidString);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        _ = Assert.Single(result.Errors);
        Assert.Equal(StronglyTypedIdException.ErrorCode.NullId.ToString(), result.Errors[0].Code);
        Assert.Equal(StronglyTypedIdException.NullId().Message, result.Errors[0].Message);
    }

    [Fact]
    public void ValidateAndCreateId_NonExistentCreateMethod_ReturnsFailureResultWithCreateMethodNotFoundError()
    {
        string validGuidString = Guid.NewGuid().ToString();

        Result<TestIdWithoutCreate> result = IdHelper.ValidateAndCreateId<TestIdWithoutCreate>(validGuidString);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Internal, result.ErrorType);
        _ = Assert.Single(result.Errors);
        Assert.Equal(IdHelper.CreateMethodNotFoundErrorCode, result.Errors[0].Code);
        Assert.Equal($"Create method not found for {nameof(TestIdWithoutCreate)}.", result.Errors[0].Message);
    }

    private sealed record TestId(Guid Value) : StronglyTypedId<TestId>(Value)
    {
        public static Result<TestId> Create(Guid value)
        {
            if (value == Guid.Empty)
            {
                var exception = StronglyTypedIdException.NullId();
                return Result<TestId>.Failure(ErrorType.Validation,
                    new Error(exception.Code.ToString(), exception.Message.ToString()));
            }
            return Result<TestId>.Success(new TestId(value));
        }
    }

    private sealed record TestIdWithoutCreate(Guid Value) : StronglyTypedId<TestIdWithoutCreate>(Value);
}
