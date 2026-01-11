# Audit Trail and Soft Delete Implementation

## Overview
This document describes the implementation of Feature 765 (Audit Trail) and Feature 766 (Soft Delete Strategy) for the TestNest.CleanArchitecture project.

## Implementation Summary

### 1. Interfaces Created

#### IAuditableEntity Interface
**Location:** `TestNest.Admin.SharedLibrary/Common/BaseEntity/IAuditableEntity.cs`

Properties:
- `CreatedOnUtc` (DateTimeOffset) - When the entity was created
- `CreatedBy` (string?) - Who created the entity
- `ModifiedOnUtc` (DateTimeOffset?) - When the entity was last modified
- `ModifiedBy` (string?) - Who last modified the entity

#### ISoftDeletable Interface
**Location:** `TestNest.Admin.SharedLibrary/Common/BaseEntity/ISoftDeletable.cs`

Properties:
- `IsDeleted` (bool) - Whether the entity is soft deleted
- `DeletedOnUtc` (DateTimeOffset?) - When the entity was deleted
- `DeletedBy` (string?) - Who deleted the entity

### 2. BaseEntity Updated

**Location:** `TestNest.Admin.SharedLibrary/Common/BaseEntity/BaseEntity.cs`

The `BaseEntity<TId>` class now implements both `IAuditableEntity` and `ISoftDeletable` interfaces, meaning **all entities in the system automatically inherit these capabilities**.

### 3. EF Core Interceptors

#### AuditableEntityInterceptor
**Location:** `TestNest.Admin.Infrastructure/Persistence/Interceptors/AuditableEntityInterceptor.cs`

Automatically populates audit fields:
- On **Insert**: Sets `CreatedOnUtc` and `CreatedBy`
- On **Update**: Sets `ModifiedOnUtc` and `ModifiedBy`
- Handles owned entities as well

#### SoftDeleteInterceptor
**Location:** `TestNest.Admin.Infrastructure/Persistence/Interceptors/SoftDeleteInterceptor.cs`

Converts hard deletes to soft deletes:
- Intercepts `EntityState.Deleted` operations
- Changes state to `EntityState.Modified`
- Sets `IsDeleted = true`, `DeletedOnUtc`, and `DeletedBy`

### 4. ApplicationDbContext Updates

**Location:** `TestNest.Admin.Infrastructure/Persistence/ApplicationDbContext.cs`

Added `ConfigureSoftDeleteFilter` method that:
- Automatically applies global query filter `IsDeleted == false` to all entities implementing `ISoftDeletable`
- Ensures soft-deleted entities are excluded from queries by default

### 5. InfrastructureRegistration Updates

**Location:** `TestNest.Admin.Infrastructure/InfrastructureRegistration.cs`

Registered interceptors with dependency injection:
```csharp
services.AddSingleton<AuditableEntityInterceptor>();
services.AddSingleton<SoftDeleteInterceptor>();
```

Added interceptors to DbContext:
```csharp
options.AddInterceptors(auditInterceptor, softDeleteInterceptor);
```

### 6. Repository Enhancements

#### IGenericRepository Interface
**Location:** `TestNest.Admin.Application/Contracts/Common/IGenericRepository.cs`

Added new methods:
- `RestoreAsync(TId id)` - Restores a soft-deleted entity
- `PermanentDeleteAsync(TId id)` - Permanently deletes an entity (hard delete)
- `GetAllIncludingDeletedAsync()` - Gets all entities including soft-deleted ones

#### GenericRepository Implementation
**Location:** `TestNest.Admin.Infrastructure/Persistence/Repositories/Common/GenericRepository.cs`

Implemented the new methods:

**RestoreAsync:**
- Uses `IgnoreQueryFilters()` to find soft-deleted entities
- Sets `IsDeleted = false` and clears delete audit fields
- Validates that entity is actually deleted before restoring

**PermanentDeleteAsync:**
- Uses `IgnoreQueryFilters()` to find entities (including soft-deleted)
- Bypasses the soft delete interceptor to perform hard delete
- Should be used with caution

**GetAllIncludingDeletedAsync:**
- Uses `IgnoreQueryFilters()` to return all entities
- Useful for admin views or audit purposes

### 7. Database Migration

**Location:** `TestNest.Admin.Infrastructure/Migrations/20260110000000_AddAuditAndSoftDeleteSupport.cs`

Adds the following columns to all entity tables:
- `CreatedOnUtc` (datetimeoffset, NOT NULL, default: current UTC time)
- `CreatedBy` (nvarchar(100), NULL)
- `ModifiedOnUtc` (datetimeoffset, NULL)
- `ModifiedBy` (nvarchar(100), NULL)
- `IsDeleted` (bit, NOT NULL, default: false)
- `DeletedOnUtc` (datetimeoffset, NULL)
- `DeletedBy` (nvarchar(100), NULL)

Also creates indexes on `IsDeleted` column for all tables to optimize query performance.

## Affected Tables

The migration affects all entity tables:
1. EmployeeRoles
2. Employees
3. Establishments
4. EstablishmentAddresses
5. EstablishmentContacts
6. EstablishmentPhones
7. EstablishmentMembers
8. EstablishmentSocialMedias
9. SocialMediaPlatforms
10. Users

## How It Works

### Audit Trail
1. When an entity is **created**, the interceptor automatically sets:
   - `CreatedOnUtc = DateTimeOffset.UtcNow`
   - `CreatedBy = "System"` (TODO: implement user from HttpContext)

2. When an entity is **updated**, the interceptor automatically sets:
   - `ModifiedOnUtc = DateTimeOffset.UtcNow`
   - `ModifiedBy = "System"` (TODO: implement user from HttpContext)

### Soft Delete
1. When `DeleteAsync(id)` is called, the interceptor:
   - Prevents the actual deletion
   - Sets `IsDeleted = true`
   - Sets `DeletedOnUtc = DateTimeOffset.UtcNow`
   - Sets `DeletedBy = "System"` (TODO: implement user from HttpContext)

2. All queries automatically filter out soft-deleted entities due to global query filter

3. To include soft-deleted entities, use:
   - `GetAllIncludingDeletedAsync()` - repository method
   - `.IgnoreQueryFilters()` - in custom queries

4. To restore a soft-deleted entity:
   - Call `RestoreAsync(id)`

5. To permanently delete (hard delete):
   - Call `PermanentDeleteAsync(id)` - use with caution!

## Testing Instructions

### 1. Build the Solution
```bash
dotnet build
```

### 2. Apply the Migration
```bash
dotnet ef database update --project TestNest.Admin.Infrastructure --startup-project TestNest.Admin.API
```

### 3. Test Audit Trail

**Create Test:**
```csharp
// Create a new employee
var employee = Employee.Create(...);
await repository.AddAsync(employee);
await unitOfWork.SaveChangesAsync();

// Verify CreatedOnUtc and CreatedBy are set
Assert.NotNull(employee.CreatedOnUtc);
Assert.NotNull(employee.CreatedBy);
```

**Update Test:**
```csharp
// Update the employee
var updatedEmployee = employee.WithEmail(newEmail);
await repository.UpdateAsync(updatedEmployee);
await unitOfWork.SaveChangesAsync();

// Verify ModifiedOnUtc and ModifiedBy are set
Assert.NotNull(updatedEmployee.ModifiedOnUtc);
Assert.NotNull(updatedEmployee.ModifiedBy);
```

### 4. Test Soft Delete

**Soft Delete Test:**
```csharp
// Delete the employee (soft delete)
await repository.DeleteAsync(employeeId);
await unitOfWork.SaveChangesAsync();

// Verify it's not returned in normal queries
var result = await repository.GetByIdAsync(employeeId);
Assert.True(result.IsFailure); // Not found

// Verify it's soft deleted
var allIncludingDeleted = await repository.GetAllIncludingDeletedAsync();
var deletedEmployee = allIncludingDeleted.Value.First(e => e.Id == employeeId);
Assert.True(deletedEmployee.IsDeleted);
Assert.NotNull(deletedEmployee.DeletedOnUtc);
Assert.NotNull(deletedEmployee.DeletedBy);
```

**Restore Test:**
```csharp
// Restore the soft-deleted employee
await repository.RestoreAsync(employeeId);
await unitOfWork.SaveChangesAsync();

// Verify it's returned in normal queries again
var result = await repository.GetByIdAsync(employeeId);
Assert.True(result.IsSuccess);
Assert.False(result.Value.IsDeleted);
```

**Permanent Delete Test:**
```csharp
// Permanently delete the employee
await repository.PermanentDeleteAsync(employeeId);
await unitOfWork.SaveChangesAsync();

// Verify it's completely removed
var allIncludingDeleted = await repository.GetAllIncludingDeletedAsync();
var exists = allIncludingDeleted.Value.Any(e => e.Id == employeeId);
Assert.False(exists);
```

### 5. Run Unit Tests
```bash
dotnet test
```

### 6. Run Integration Tests
Ensure the integration tests cover:
- Creating entities and verifying audit fields
- Updating entities and verifying modified fields
- Soft deleting entities
- Restoring soft-deleted entities
- Querying with and without soft-deleted entities

## TODO Items

### High Priority
1. **Implement User Context Service**
   - Create `ICurrentUserService` interface
   - Implement service to get current user from `HttpContext`
   - Update interceptors to use actual user instead of "System"

2. **Update API Controllers**
   - Add endpoints for restore operations
   - Add query parameter to include soft-deleted entities (e.g., `?includeDeleted=true`)
   - Add permanent delete endpoints (admin only)

### Medium Priority
3. **Add Authorization**
   - Ensure only admins can restore entities
   - Ensure only admins can permanently delete
   - Ensure only admins can view soft-deleted entities

4. **Add Audit Log Viewing**
   - Create API endpoints to view audit history
   - Add DTOs for audit information
   - Consider creating a dedicated audit log table for full history

### Low Priority
5. **Add Soft Delete to Responses**
   - Include `IsDeleted` flag in response DTOs
   - Add `DeletedOnUtc` and `DeletedBy` to response DTOs

6. **Performance Optimization**
   - Monitor query performance with soft delete filters
   - Consider partitioning for very large tables
   - Add composite indexes if needed

## Benefits

### Audit Trail Benefits
- **Compliance**: Meet regulatory requirements for data tracking
- **Debugging**: Track when and who made changes
- **Analytics**: Understand data creation and modification patterns
- **Security**: Audit user actions for security analysis

### Soft Delete Benefits
- **Data Recovery**: Easily restore accidentally deleted data
- **Referential Integrity**: Maintain relationships without cascade deletes
- **Audit Compliance**: Keep deleted data for audit purposes
- **User Experience**: "Undo delete" functionality
- **Performance**: Soft deletes are faster than hard deletes with complex relationships

## Architecture Compliance

This implementation maintains Clean Architecture principles:
- **Domain Layer**: No changes required, entities inherit from `BaseEntity`
- **Application Layer**: Interface changes only, no breaking changes
- **Infrastructure Layer**: All persistence logic contained here
- **API Layer**: No changes required, but can be enhanced with new endpoints

The implementation follows SOLID principles:
- **Single Responsibility**: Each interceptor handles one concern
- **Open/Closed**: Extended BaseEntity without modifying existing entities
- **Liskov Substitution**: All entities still work as before
- **Interface Segregation**: Separate interfaces for audit and soft delete
- **Dependency Inversion**: Using interfaces and DI

## Rollback Instructions

If you need to rollback this implementation:

1. Revert the migration:
```bash
dotnet ef database update 20250501103613_initial_migrations --project TestNest.Admin.Infrastructure --startup-project TestNest.Admin.API
```

2. Delete the migration file:
```bash
rm TestNest.Admin.Infrastructure/Migrations/20260110000000_AddAuditAndSoftDeleteSupport.cs
```

3. Revert code changes using git:
```bash
git checkout HEAD~1 -- TestNest.Admin.SharedLibrary/Common/BaseEntity/
git checkout HEAD~1 -- TestNest.Admin.Infrastructure/Persistence/
git checkout HEAD~1 -- TestNest.Admin.Application/Contracts/Common/IGenericRepository.cs
```

## Conclusion

The audit trail and soft delete implementation is complete and ready for testing. All entities in the system now automatically track creation, modification, and deletion information. The implementation is transparent to existing code and follows best practices for Clean Architecture.
