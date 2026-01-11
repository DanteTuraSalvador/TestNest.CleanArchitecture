# Database Migration Rollback Script
# This script helps rollback database migrations in case of deployment failure

param(
    [Parameter(Mandatory=$true)]
    [string]$Environment,

    [Parameter(Mandatory=$true)]
    [string]$TargetMigration,

    [Parameter(Mandatory=$true)]
    [string]$ConnectionString,

    [Parameter(Mandatory=$false)]
    [string]$InfrastructureDll,

    [Parameter(Mandatory=$false)]
    [string]$StartupDll
)

Write-Host "========================================" -ForegroundColor Yellow
Write-Host "Database Migration Rollback Script" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Yellow
Write-Host ""
Write-Host "Environment: $Environment" -ForegroundColor Cyan
Write-Host "Target Migration: $TargetMigration" -ForegroundColor Cyan
Write-Host "Timestamp: $(Get-Date)" -ForegroundColor Cyan
Write-Host ""

# Confirm rollback action
Write-Host "WARNING: You are about to rollback the database!" -ForegroundColor Red
Write-Host "This action will:"
Write-Host "  1. Rollback migrations to: $TargetMigration"
Write-Host "  2. Potentially lose data from newer migrations"
Write-Host "  3. Affect the $Environment environment"
Write-Host ""

if ($Environment -eq "Production") {
    Write-Host "CRITICAL: This is a PRODUCTION environment rollback!" -ForegroundColor Red -BackgroundColor Yellow
    $confirmation = Read-Host "Type 'ROLLBACK PRODUCTION' to proceed"
    if ($confirmation -ne "ROLLBACK PRODUCTION") {
        Write-Host "Rollback cancelled by user" -ForegroundColor Yellow
        exit 0
    }
} else {
    $confirmation = Read-Host "Type 'YES' to proceed with rollback"
    if ($confirmation -ne "YES") {
        Write-Host "Rollback cancelled by user" -ForegroundColor Yellow
        exit 0
    }
}

Write-Host ""
Write-Host "Step 1: Creating backup before rollback..." -ForegroundColor Green

# Create backup timestamp
$backupTimestamp = Get-Date -Format "yyyy-MM-dd-HHmmss"
$backupName = "$Environment-prerollback-$backupTimestamp"

Write-Host "Backup name: $backupName"
Write-Host "TODO: Implement actual database backup"
Write-Host "- Use Azure SQL automated backup"
Write-Host "- Create manual backup before rollback"
Write-Host "- Store backup location for recovery"
Write-Host ""

Write-Host "Step 2: Listing current migrations..." -ForegroundColor Green

try {
    # List current migrations
    if ($InfrastructureDll -and $StartupDll) {
        dotnet ef migrations list --project $InfrastructureDll --startup-project $StartupDll --connection $ConnectionString
    } else {
        dotnet ef migrations list --connection $ConnectionString
    }
} catch {
    Write-Error "Failed to list migrations: $_"
    exit 1
}

Write-Host ""
Write-Host "Step 3: Rolling back to migration: $TargetMigration..." -ForegroundColor Green

try {
    # Execute rollback
    if ($InfrastructureDll -and $StartupDll) {
        dotnet ef database update $TargetMigration --project $InfrastructureDll --startup-project $StartupDll --connection $ConnectionString --verbose
    } else {
        dotnet ef database update $TargetMigration --connection $ConnectionString --verbose
    }

    if ($LASTEXITCODE -ne 0) {
        throw "Rollback failed with exit code $LASTEXITCODE"
    }

    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Rollback completed successfully!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Environment: $Environment" -ForegroundColor Cyan
    Write-Host "Rolled back to: $TargetMigration" -ForegroundColor Cyan
    Write-Host "Completed at: $(Get-Date)" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Next Steps:" -ForegroundColor Yellow
    Write-Host "1. Verify application functionality"
    Write-Host "2. Check database integrity"
    Write-Host "3. Review rollback logs"
    Write-Host "4. Document the incident"
    Write-Host "5. Fix the issue before redeploying"

} catch {
    Write-Error "CRITICAL: Rollback failed!"
    Write-Error $_.Exception.Message
    Write-Host ""
    Write-Host "Emergency Recovery Steps:" -ForegroundColor Red
    Write-Host "1. Do NOT make any more changes"
    Write-Host "2. Contact database administrator immediately"
    Write-Host "3. Consider restoring from backup: $backupName"
    Write-Host "4. Review error logs in detail"
    Write-Host "5. Have incident response team on standby"
    exit 1
}

# Example Usage:
# .\rollback-migration.ps1 -Environment "Staging" -TargetMigration "20250110000000_PreviousMigration" -ConnectionString "Server=..."
# .\rollback-migration.ps1 -Environment "Production" -TargetMigration "20250110000000_PreviousMigration" -ConnectionString "Server=..." -InfrastructureDll "path/to/Infrastructure.dll" -StartupDll "path/to/API.dll"
