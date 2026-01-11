namespace TestNest.Admin.SharedLibrary.Common.BaseEntity;

/// <summary>
/// Represents an entity that supports soft delete functionality.
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// Gets or sets a value indicating whether the entity is deleted.
    /// </summary>
    bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was deleted (UTC).
    /// </summary>
    DateTimeOffset? DeletedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the user who deleted the entity.
    /// </summary>
    string? DeletedBy { get; set; }
}
