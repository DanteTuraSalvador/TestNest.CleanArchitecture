﻿using TestNest.Admin.Domain.SocialMedias;
using TestNest.Admin.SharedLibrary.Common.BaseEntity;
using TestNest.Admin.SharedLibrary.Common.Guards;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Exceptions;
using TestNest.Admin.SharedLibrary.Exceptions.Common;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestNest.Admin.Domain.Establishments;

public sealed class EstablishmentSocialMedia : BaseEntity<EstablishmentSocialMediaId>
{
    // properties
    public EstablishmentSocialMediaId EstablishmentSocialMediaId => Id;

    public EstablishmentId EstablishmentId { get; private set; }
    public SocialMediaId SocialMediaId { get; private set; }
    public SocialMediaAccountName SocialMediaAccountName { get; private set; }

    // empty object
    private static readonly Lazy<EstablishmentSocialMedia> _empty = new(()
        => new EstablishmentSocialMedia());
    public static EstablishmentSocialMedia Empty() => _empty.Value;
    public bool IsEmpty() => this == Empty(); 
    

    // navigation properties
    public Establishment Establishment { get; private set; } = default!;

    public SocialMediaPlatform SocialMedia { get; private set; } = default!;

    // constructor
    private EstablishmentSocialMedia() : base(EstablishmentSocialMediaId.Empty())
        => (EstablishmentId, SocialMediaId, SocialMediaAccountName)
            = (EstablishmentId.Empty(), SocialMediaId.Empty(), SocialMediaAccountName.Empty());

    private EstablishmentSocialMedia(
        EstablishmentSocialMediaId establishmentSocialMediaId,
        EstablishmentId establishmentId,
        SocialMediaId socialMediaId,
        SocialMediaAccountName socialMediaAccountName)
      : base(establishmentSocialMediaId)
        => (EstablishmentId, SocialMediaId, SocialMediaAccountName)
            = (establishmentId, socialMediaId, socialMediaAccountName);

    // factory method
    public static Result<EstablishmentSocialMedia> Create(
        EstablishmentId establishmentId,
        SocialMediaId socialMediaId,
        SocialMediaAccountName socialMediaAccountName)
    {
        var result = Result.Combine(
            Guard.AgainstCondition(establishmentId == EstablishmentId.Empty(),
                static () => StronglyTypedIdException.NullId()),
            Guard.AgainstCondition(socialMediaId == SocialMediaId.Empty(),
                static () => StronglyTypedIdException.NullId()),
            Guard.AgainstCondition(socialMediaAccountName.IsEmpty(),
                static () => SocialMediaAccountNameException.EmptyAccountName())
        );

        return result.IsSuccess
            ? Result<EstablishmentSocialMedia>.Success(new EstablishmentSocialMedia(
                EstablishmentSocialMediaId.New(),
                establishmentId,
                socialMediaId,
                socialMediaAccountName))
            : Result<EstablishmentSocialMedia>.Failure(ErrorType.Validation, result.Errors);
    }

    // factory methods for updating properties
    public Result<EstablishmentSocialMedia> WithSocialMediaId(SocialMediaId newSocialMediaId)
    {
        Result result = Guard.AgainstCondition(newSocialMediaId == SocialMediaId.Empty(),
           static () => StronglyTypedIdException.NullId());

        return result.IsSuccess
            ? Result<EstablishmentSocialMedia>.Success(new EstablishmentSocialMedia(
                EstablishmentSocialMediaId,
                EstablishmentId,
                newSocialMediaId,
                SocialMediaAccountName))
            : Result<EstablishmentSocialMedia>.Failure(ErrorType.Validation, result.Errors);
    }

    public Result<EstablishmentSocialMedia> WithSocialMediaAccountName(SocialMediaAccountName newAccountName)
    {
        Result result = Guard.AgainstCondition(newAccountName.IsEmpty(),
            static () => SocialMediaAccountNameException.EmptyAccountName());

        return result.IsSuccess
            ? Result<EstablishmentSocialMedia>.Success(new EstablishmentSocialMedia(
                EstablishmentSocialMediaId,
                EstablishmentId,
                SocialMediaId,
                newAccountName))
            : Result<EstablishmentSocialMedia>.Failure(ErrorType.Validation, result.Errors);
    }

    public string GetFullSocialMediaLink()
      => string.IsNullOrWhiteSpace(SocialMedia?.SocialMediaName?.PlatformURL) ? string.Empty
          : $"{SocialMedia.SocialMediaName.PlatformURL}{SocialMediaAccountName.AccountName}";
}
