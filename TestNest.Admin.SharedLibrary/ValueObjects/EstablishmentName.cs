using System.Text.RegularExpressions;
using TestNest.Admin.SharedLibrary.Common.Guards;
using TestNest.Admin.SharedLibrary.ValueObjects.Common;

namespace TestNest.Admin.SharedLibrary.ValueObjects;

public sealed class EstablishmentName : ValueObject
{
    private static readonly Regex ValidPattern = new(@"^[\p{L}0-9\s&,.'-]+$", RegexOptions.Compiled);

    private static readonly Lazy<EstablishmentName> _empty = new(() => new EstablishmentName());
    public string Name { get; }

    public bool IsEmpty() => this == Empty();

    public static EstablishmentName Empty() => _empty.Value;

    private EstablishmentName() => Name = string.Empty;

    private EstablishmentName(string value) => Name = value;

    public static Result<EstablishmentName> Create(string name)
    {
        Result resultNull = Guard.AgainstNull(name, static () => EstablishmentNameException.NullEstablishmentName());
        if (!resultNull.IsSuccess)
        {
            return Result<EstablishmentName>.Failure(ErrorType.Validation, resultNull.Errors);
        }

        Result validationResult = ValidateEstablishmentName(name);
        return validationResult.IsSuccess
            ? Result<EstablishmentName>.Success(new EstablishmentName(name))
            : Result<EstablishmentName>.Failure(ErrorType.Validation, validationResult.Errors);
    }

    public Result<EstablishmentName> Update(string newName)
    {
        Result resultNull = Guard.AgainstNull(newName, static () => EstablishmentNameException.NullEstablishmentName());
        if (!resultNull.IsSuccess)
        {
            return Result<EstablishmentName>.Failure(ErrorType.Validation, resultNull.Errors);
        }

        return Create(newName);
    }

    private static Result ValidateEstablishmentName(string name) => Result.Combine(
            Guard.AgainstNullOrWhiteSpace(name, static () => EstablishmentNameException.EmptyName()),
            Guard.AgainstRange(name.Length, 3, 50, static () => EstablishmentNameException.InvalidLength()),
            Guard.AgainstCondition(name != null && !ValidPattern.IsMatch(name), static () => EstablishmentNameException.InvalidCharacters())
        );

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Name;
    }

    public override string ToString() => Name;
}
