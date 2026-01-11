# Azure Pipelines Setup Guide

This guide explains how to configure the CI/CD pipeline for TestNest.CleanArchitecture.

## Pipeline Overview

The pipeline consists of the following stages:

1. **Build and Test** - Compiles the solution and runs all unit tests with code coverage
2. **Code Quality Analysis** - Performs static code analysis
3. **Deploy to Dev** - Deploys to Development environment (triggered by `develop` branch)
4. **Deploy to Staging** - Deploys to Staging environment (triggered by `master` branch)
5. **Deploy to Production** - Deploys to Production environment (requires approval)

## Prerequisites

1. Azure DevOps organization and project
2. Azure subscription with App Services for each environment
3. Service connection to Azure in Azure DevOps

## Azure DevOps Configuration

### 1. Create Environments

Environments in Azure DevOps allow you to configure approval gates and deployment history.

1. Navigate to **Pipelines > Environments** in Azure DevOps
2. Create the following environments:

#### TestNest-Dev
- **Name**: `TestNest-Dev`
- **Description**: Development environment for TestNest application
- **Approvals**: None (auto-deploy)

#### TestNest-Staging
- **Name**: `TestNest-Staging`
- **Description**: Staging environment for TestNest application
- **Approvals**:
  - Add approvers (recommended: Tech Lead, QA Lead)
  - Configure approval timeout: 24 hours
  - Enable "Allow approvers to approve their own runs": No

#### TestNest-Production
- **Name**: `TestNest-Production`
- **Description**: Production environment for TestNest application
- **Approvals**:
  - Add approvers (recommended: Product Owner, Tech Lead, DevOps Lead)
  - Configure approval timeout: 7 days
  - Enable "Allow approvers to approve their own runs": No
  - Enable "Require all approvers to approve": Yes

### 2. Create Service Connection

1. Navigate to **Project Settings > Service connections**
2. Click **New service connection**
3. Select **Azure Resource Manager**
4. Choose **Service principal (automatic)**
5. Select your Azure subscription
6. Name it: `AzureServiceConnection`
7. Grant access permission to all pipelines
8. Click **Save**

### 3. Configure Pipeline Variables

Navigate to **Pipelines > Library** and create a variable group named `TestNest-Pipeline-Variables`:

#### Required Variables

| Variable Name | Description | Example Value |
|--------------|-------------|---------------|
| `AzureServiceConnection` | Name of the Azure service connection | `AzureServiceConnection` |
| `DevWebAppName` | Azure App Service name for Dev | `testnest-api-dev` |
| `StagingWebAppName` | Azure App Service name for Staging | `testnest-api-staging` |
| `ProductionWebAppName` | Azure App Service name for Production | `testnest-api-prod` |

#### Optional Variables

| Variable Name | Description | Default Value |
|--------------|-------------|---------------|
| `buildConfiguration` | Build configuration | `Release` |
| `dotnetVersion` | .NET SDK version | `9.x` |

### 4. Link Variable Group to Pipeline

1. Navigate to **Pipelines > Pipelines**
2. Select your pipeline
3. Click **Edit**
4. Click the three dots (...) and select **Triggers**
5. Go to **Variables** tab
6. Click **Variable groups**
7. Link the `TestNest-Pipeline-Variables` group

## Azure App Services Setup

Create three Azure App Services (one for each environment):

### Development Environment
```bash
az webapp create \
  --name testnest-api-dev \
  --resource-group testnest-rg \
  --plan testnest-plan-dev \
  --runtime "DOTNETCORE:8.0"
```

### Staging Environment
```bash
az webapp create \
  --name testnest-api-staging \
  --resource-group testnest-rg \
  --plan testnest-plan-staging \
  --runtime "DOTNETCORE:8.0"
```

### Production Environment
```bash
az webapp create \
  --name testnest-api-prod \
  --resource-group testnest-rg \
  --plan testnest-plan-prod \
  --runtime "DOTNETCORE:8.0"
```

## Deployment Flow

### Development Deployment
- **Trigger**: Push to `develop` branch
- **Approval**: None (automatic)
- **Steps**:
  1. Build and Test
  2. Code Quality Analysis
  3. Deploy to Dev environment

### Staging Deployment
- **Trigger**: Push to `master` branch
- **Approval**: Required (configured in environment)
- **Steps**:
  1. Build and Test
  2. Code Quality Analysis
  3. Deploy to Staging environment
  4. Run smoke tests
  5. Wait for approval (if configured)

### Production Deployment
- **Trigger**: After successful Staging deployment
- **Approval**: Required (configured in environment)
- **Steps**:
  1. Deploy to Production environment
  2. Run smoke tests
  3. Send deployment notification

## Approval Process

### Staging Approval
1. Pipeline completes Build and CodeQuality stages
2. Deployment to Staging is triggered automatically
3. Approvers receive notification
4. Approvers review changes and test in Staging
5. Approvers approve or reject deployment
6. If approved, deployment proceeds

### Production Approval
1. Staging deployment completes successfully
2. Production deployment waits for approval
3. Approvers receive notification
4. Approvers review Staging environment
5. Approvers approve or reject deployment
6. If approved, deployment to Production proceeds

## Health Checks

The pipeline includes smoke tests that verify the application health after deployment:

- **Endpoint**: `/health`
- **Expected Response**: HTTP 200 OK
- **Action on Failure**: Deployment fails and rollback should be initiated

## Troubleshooting

### Common Issues

#### 1. Service Connection Failed
- Verify the service connection is active
- Check Azure subscription permissions
- Ensure the service principal has Contributor role

#### 2. Deployment Failed
- Check App Service logs in Azure Portal
- Verify the runtime stack matches (.NET 8.0)
- Ensure connection strings are configured in App Service

#### 3. Health Check Failed
- Verify the application is running
- Check database connection strings
- Review application logs

#### 4. Tests Failed
- Check test results in Azure DevOps
- Run tests locally to reproduce
- Fix failing tests before re-running pipeline

## Security Best Practices

1. **Never commit secrets** - Use Azure Key Vault or Azure DevOps Library
2. **Limit approvers** - Only trusted personnel should approve Production deployments
3. **Use managed identities** - Configure App Services with managed identities for database access
4. **Enable audit logging** - Track all deployments and approvals
5. **Implement rollback strategy** - Be prepared to rollback failed deployments

## Monitoring

After deployment, monitor the following:

1. **Application Insights** - Application performance and errors
2. **Azure App Service Metrics** - CPU, memory, response time
3. **Log Analytics** - Centralized logging and queries
4. **Availability Tests** - Continuous health monitoring

## Next Steps

1. Set up Application Insights for each environment
2. Configure database migrations in the pipeline (Feature 752)
3. Add Docker support and container registry (Feature 753)
4. Implement blue-green deployment strategy
5. Add automated integration tests to the pipeline
