namespace TestNest.Admin.SharedLibrary.Common.BaseEntity;

/// <summary>
/// Represents an entity that tracks creation and modification audit information.
/// </summary>
public interface IAuditableEntity
{
    /// <summary>
    /// Gets or sets the date and time when the entity was created (UTC).
    /// </summary>
    DateTimeOffset CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the user who created the entity.
    /// </summary>
    string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was last modified (UTC).
    /// </summary>
    DateTimeOffset? ModifiedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the user who last modified the entity.
    /// </summary>
    string? ModifiedBy { get; set; }
}
