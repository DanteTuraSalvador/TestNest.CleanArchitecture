# Database Migrations Guide

This guide explains how database migrations are handled in the CI/CD pipeline for TestNest.CleanArchitecture.

## Overview

Database migrations are automatically applied during the deployment process for each environment. The pipeline uses Entity Framework Core migrations to keep the database schema in sync with the application code.

## Migration Flow

### Development Environment
1. Build and test pass
2. Download artifacts
3. Install EF Core tools
4. Create database backup (placeholder)
5. Apply migrations
6. Deploy application
7. Run health checks

### Staging Environment
1. Build and test pass
2. Download artifacts
3. Install EF Core tools
4. Create database backup
5. Apply migrations with rollback on failure
6. Deploy application
7. Run smoke tests

### Production Environment
1. Build and test pass
2. Staging deployment succeeds
3. Approval gate (required)
4. Download artifacts
5. Install EF Core tools
6. **Create critical database backup**
7. **Validate migrations (generate SQL script)**
8. **Preview migration script**
9. Apply migrations with enhanced error handling
10. Deploy application
11. Run smoke tests
12. **Verify database integrity**
13. Send deployment notification

## Pipeline Variables

The following variables must be configured in Azure DevOps:

| Variable | Description | Example |
|----------|-------------|---------|
| `DevDatabaseConnectionString` | Dev database connection string | `Server=tcp:testnest-dev.database.windows.net,1433;Database=TestNest;...` |
| `StagingDatabaseConnectionString` | Staging database connection string | `Server=tcp:testnest-staging.database.windows.net,1433;Database=TestNest;...` |
| `ProductionDatabaseConnectionString` | Production database connection string | `Server=tcp:testnest-prod.database.windows.net,1433;Database=TestNest;...` |

**Security Note**: Store connection strings as **secret variables** in Azure DevOps Library or Azure Key Vault.

## Creating a New Migration

### Local Development

1. Make changes to your entity models
2. Create a migration:
   ```bash
   cd TestNest.Admin.Infrastructure
   dotnet ef migrations add YourMigrationName --startup-project ../TestNest.Admin.API
   ```

3. Review the generated migration files:
   - `Migrations/YYYYMMDDHHMMSS_YourMigrationName.cs`
   - `Migrations/YYYYMMDDHHMMSS_YourMigrationName.Designer.cs`

4. Test the migration locally:
   ```bash
   dotnet ef database update --startup-project ../TestNest.Admin.API
   ```

5. Commit and push:
   ```bash
   git add .
   git commit -m "Add migration: YourMigrationName"
   git push
   ```

### Pipeline Execution

Once pushed, the pipeline will:
- **Develop branch**: Auto-deploy to Dev with migration
- **Master branch**: Auto-deploy to Staging with migration, then await Production approval

## Migration Safety Features

### Backup Before Migration

The pipeline creates a backup before applying migrations:

**Dev**: Placeholder backup (implement for actual use)
**Staging**: Timestamped backup with name format: `staging-backup-YYYY-MM-DD-HHMMSS`
**Production**: Critical backup with validation and retention

### Migration Validation (Production Only)

Before applying migrations to Production:

1. **Generate SQL Script**: Creates an idempotent SQL script
2. **Preview Script**: Displays first 50 lines for review
3. **Manual Review**: Team can review the exact SQL that will be executed

### Error Handling

If a migration fails:

1. **Automatic Rollback**: Pipeline stops deployment
2. **Error Logging**: Detailed error messages logged
3. **Alert Team**: Deployment failure notifications sent
4. **Manual Intervention**: Database administrator contacted

## Rollback Strategy

### Automated Rollback (In Development)

The pipeline includes placeholders for automated rollback:
- Azure SQL point-in-time restore
- Migration rollback scripts
- Database snapshot restore

### Manual Rollback

Use the provided rollback script for manual intervention:

```powershell
cd scripts

# Staging rollback
.\rollback-migration.ps1 `
    -Environment "Staging" `
    -TargetMigration "20250110000000_PreviousMigration" `
    -ConnectionString "Server=..."

# Production rollback (requires confirmation)
.\rollback-migration.ps1 `
    -Environment "Production" `
    -TargetMigration "20250110000000_PreviousMigration" `
    -ConnectionString "Server=..." `
    -InfrastructureDll "path/to/Infrastructure.dll" `
    -StartupDll "path/to/API.dll"
```

**Rollback Process:**
1. Creates pre-rollback backup
2. Lists current migrations
3. Requires explicit confirmation
4. Rolls back to target migration
5. Verifies rollback success
6. Provides next steps guidance

## Best Practices

### Migration Design

1. **Keep migrations small**: One logical change per migration
2. **Test migrations**: Always test locally before pushing
3. **Idempotent scripts**: Ensure migrations can be rerun safely
4. **Data migrations**: Handle data transformations carefully
5. **Breaking changes**: Plan for zero-downtime deployments

### Naming Conventions

Use descriptive migration names:
- ✅ `AddUserProfileTable`
- ✅ `UpdateEmployeeEmailIndex`
- ✅ `RemoveObsoleteColumns`
- ❌ `Update1`
- ❌ `Fix`

### Review Checklist

Before merging migration PR:
- [ ] Migration tested locally
- [ ] No data loss in migration
- [ ] Indexes added for performance
- [ ] Foreign keys validated
- [ ] Rollback strategy documented
- [ ] Team reviewed SQL script

## Zero-Downtime Migrations

For breaking changes, use a multi-phase approach:

### Phase 1: Add (Backward Compatible)
```csharp
// Add new column, keep old column
migrationBuilder.AddColumn<string>("NewEmail", nullable: true);
```

### Phase 2: Migrate Data
```csharp
// Copy data from old to new column
migrationBuilder.Sql("UPDATE Users SET NewEmail = OldEmail");
```

### Phase 3: Deploy Application
- Deploy app version that uses NewEmail
- App can still handle OldEmail for backward compatibility

### Phase 4: Remove Old (Breaking Change)
```csharp
// After confirming new column works
migrationBuilder.DropColumn("OldEmail");
```

## Monitoring and Troubleshooting

### Migration Logs

View migration logs in Azure DevOps:
1. Go to Pipelines > Runs
2. Select the deployment run
3. Expand "Run Database Migrations" step
4. Review detailed logs

### Common Issues

#### Issue: "A connection was successfully established..."
**Solution**: Check connection string, firewall rules, and managed identity

#### Issue: "Migration '...' has already been applied"
**Solution**: This is normal - EF Core migrations are idempotent

#### Issue: "Cannot drop column '...' because it is referenced..."
**Solution**: Remove foreign keys first, then drop column

#### Issue: "Timeout expired"
**Solution**: Increase command timeout or optimize migration

### Database Health Checks

Post-migration verification:
- Application health endpoint returns 200 OK
- Critical tables exist
- Data integrity constraints validated
- Performance metrics within acceptable range

## Emergency Procedures

### Production Migration Failure

If Production migration fails:

1. **STOP**: Do not make additional changes
2. **Alert**: Contact database administrator immediately
3. **Assess**: Review error logs and determine impact
4. **Restore**: Consider point-in-time restore if needed
5. **Communicate**: Notify stakeholders of status
6. **Document**: Record incident details for postmortem

### Recovery Options

1. **Point-in-Time Restore** (Azure SQL):
   - Restore to just before migration
   - Minimal data loss (minutes)
   - Requires new database instance

2. **Manual Rollback**:
   - Use rollback-migration.ps1 script
   - Review migration history
   - Test in Staging first

3. **Database Snapshot** (SQL Server):
   - Instant restore capability
   - Requires pre-created snapshot
   - No data loss

## Future Enhancements

Planned improvements:

1. **Automated Backup Verification**:
   - Verify Azure SQL automated backups before migration
   - Validate backup integrity
   - Configure long-term retention

2. **Migration Testing**:
   - Automated migration testing in isolated environment
   - Performance impact analysis
   - Data validation post-migration

3. **Rollback Automation**:
   - Automated rollback on health check failure
   - One-click rollback from Azure DevOps
   - Rollback playbooks and runbooks

4. **Migration Approvals**:
   - Require DBA approval for schema changes
   - Migration review gate before Production
   - SQL script review in pull request

## References

- [Entity Framework Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Azure SQL Database Backup](https://learn.microsoft.com/en-us/azure/azure-sql/database/automated-backups-overview)
- [CI/CD Best Practices](https://learn.microsoft.com/en-us/azure/devops/pipelines/ecosystems/dotnet-core)
