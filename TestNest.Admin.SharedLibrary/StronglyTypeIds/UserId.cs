using TestNest.Admin.SharedLibrary.StronglyTypeIds.Common;

namespace TestNest.Admin.SharedLibrary.StronglyTypeIds;

public sealed record UserId(Guid Value) : StronglyTypedId<UserId>(Value)
{
    public UserId() : this(Guid.NewGuid()) { }

    public static Result<UserId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            var exception = StronglyTypedIdException.NullId();
            return Result<UserId>.Failure(ErrorType.Validation,
                new Error(exception.Code.ToString(), exception.Message.ToString()));
        }

        return Result<UserId>.Success(new UserId(value));
    }

    public override string ToString() => Value.ToString();
}
