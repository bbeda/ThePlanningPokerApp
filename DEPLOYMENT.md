# Deploying Planning Poker to Azure with .NET Aspire

This guide shows how to deploy your Planning Poker application to Azure using .NET Aspire's built-in publishing support.

## Prerequisites

1. **Azure Subscription** - Active Azure subscription required
2. **Azure Developer CLI (azd)** - v1.5.0 or later
3. **.NET 10 SDK** - Already installed for development
4. **Docker Desktop** - Required for containerization (https://www.docker.com/products/docker-desktop/)

## How Aspire Deployment Works

.NET Aspire automatically:
- ✅ Generates Azure infrastructure (Bicep) from your AppHost configuration
- ✅ Creates Container Apps for your services
- ✅ Builds and pushes Docker containers
- ✅ Configures networking and environment variables
- ✅ Sets up monitoring with Application Insights

You don't need to write Bicep templates or Dockerfiles manually!

## Deployment Steps

### Step 1: Install Azure Developer CLI

```powershell
# Windows (PowerShell as Administrator)
winget install microsoft.azd

# Verify installation
azd version  # Should be v1.5.0 or later
```

Or download from: https://aka.ms/azure-dev/install

### Step 2: Ensure Docker Desktop is Running

```bash
docker --version
docker ps  # Should not error
```

### Step 3: Login to Azure

```bash
azd auth login
```

This opens your browser to authenticate with Azure.

### Step 4: Build Frontend Assets

Before deploying, build the Vue.js frontend to static files:

```powershell
# Windows
.\build-frontend.ps1

# Or manually:
cd src\PlanningPoker.Web
npm ci
npm run build
cd ..\..
```

```bash
# Linux/Mac
./build-frontend.sh

# Or manually:
cd src/PlanningPoker.Web
npm ci
npm run build
cd ../..
```

This builds the frontend to `src/PlanningPoker.Api/wwwroot/` which will be included in the API container.

### Step 5: Initialize Aspire Azure Deployment

```bash
# Navigate to project root
cd "D:\source\Planning Poker"

# Initialize with Aspire template
azd init
```

When prompted:
- **How do you want to initialize your app?**: Choose **"Use code in the current directory"**
- **Confirm directory**: Yes
- **Detect Aspire**: Yes (it should detect your AppHost project)
- **Environment name**: Enter a name (e.g., `prod`, `dev`, `staging`)

This creates:
- `.azure/` folder with your environment
- Generates Bicep templates from your AppHost in `infra/` folder

### Step 6: Deploy to Azure

```bash
azd up
```

When prompted:
- **Subscription**: Select your Azure subscription
- **Location**: Choose a region (e.g., `eastus`, `westeurope`, `westus2`)

**First deployment takes ~10-15 minutes** as it:
1. Provisions Azure resources (Container Apps Environment, Container Registry, etc.)
2. Builds .NET API as Docker container (includes wwwroot with built frontend)
3. Pushes containers to Azure Container Registry
4. Deploys to Azure Container Apps
5. Configures Application Insights

### Step 7: Access Your Application

After deployment, azd displays:
```
Endpoint: https://planningpoker-api.{random}.azurecontainerapps.io
```

Open this URL in your browser - the API serves the Vue.js frontend from wwwroot!

To view endpoints anytime:
```bash
azd show
```

## What Gets Deployed

### Azure Resources Created:
- **Resource Group**: `rg-{environment-name}`
- **Container Apps Environment**: Hosts your containers
- **Azure Container Registry**: Stores your Docker images
- **Container App**: `planningpoker-api` (serves both API and frontend)
- **Log Analytics Workspace**: For logs and monitoring
- **Application Insights**: APM and diagnostics (optional)

### Architecture:
```
Internet → Container App (planningpoker-api)
           ├─ /api/* → .NET API endpoints
           └─ /* → Static Vue.js frontend (from wwwroot)
```

Single container = simpler, cheaper, faster!

## Subsequent Deployments

### Deploy Code Changes Only

```bash
# Rebuild frontend
.\build-frontend.ps1  # or ./build-frontend.sh

# Deploy updated containers
azd deploy
```

This only rebuilds/redeploys containers (~2-5 minutes).

### Deploy with Infrastructure Changes

If you modify the AppHost (add services, change configuration):

```bash
azd up
```

This regenerates Bicep and provisions any new resources.

## Monitoring and Logs

### View Live Logs

```bash
# All services
azd logs --follow

# Specific service
azd logs --service planningpoker-api --follow
```

### Azure Portal

1. Go to https://portal.azure.com
2. Find resource group: `rg-{environment-name}`
3. Click **planningpoker-api** Container App
4. View:
   - **Logs** tab → Query logs
   - **Metrics** tab → CPU, memory, requests
   - **Revisions** tab → Deployment history
   - **Console** tab → Debug container

### Application Insights

```bash
# View Application Insights in portal
azd monitor
```

## Configuration

### Environment Variables

Aspire automatically configures:
- `ASPNETCORE_ENVIRONMENT=Production`
- Application Insights connection strings
- Internal service-to-service networking

Add custom environment variables in AppHost:
```csharp
var api = builder.AddProject<Projects.PlanningPoker_Api>("planningpoker-api")
    .WithEnvironment("CUSTOM_VAR", "value")
    .WithExternalHttpEndpoints();
```

### Scaling

Container Apps auto-scales based on HTTP traffic. Default configuration:
- **Min replicas**: 0 (scale to zero when idle)
- **Max replicas**: 10
- **Scale trigger**: HTTP concurrent requests

Modify in Azure Portal → Container App → Scale

## Cost Considerations

### Azure Container Apps Pricing (Consumption):
- ~$0.000012 per vCPU-second
- ~$0.000002 per GiB-second
- **Free tier**: 180,000 vCPU-seconds + 360,000 GiB-seconds per month

### Estimated Monthly Cost:
- **No traffic** (scale to zero): $0-5/month (mostly Container Apps Environment fee)
- **Low traffic** (few sessions/day): $5-15/month
- **Medium traffic** (active use): $20-50/month

### Cost Optimization:

```bash
# Delete when not using
azd down

# Or disable scale-to-zero to reduce cold starts (but costs more):
# Azure Portal → Container App → Scale → Set min replicas to 1
```

## Custom Domain (Optional)

### Add Custom Domain:

1. **Add CNAME Record** in your DNS:
   ```
   CNAME: planningpoker.yourdomain.com → {app-url}.azurecontainerapps.io
   ```

2. **Configure in Azure Portal**:
   - Container App → Custom domains → Add custom domain
   - Enter your domain
   - Choose managed certificate (free HTTPS) or upload your own

3. **Update CORS** (if needed) in `Program.cs`:
   ```csharp
   policy.WithOrigins("https://planningpoker.yourdomain.com")
   ```

## CI/CD with GitHub Actions

### Setup Automated Deployments:

```bash
# Generate GitHub Actions workflow
azd pipeline config

# Select GitHub
# Authorize Azure and GitHub
# Workflow created in .github/workflows/azure-dev.yml
```

Now every push to `main` automatically deploys to Azure!

### Workflow includes:
1. Build frontend
2. Run tests (if any)
3. Deploy with `azd deploy`
4. Run smoke tests

## Production Optimizations

### 1. Enable HTTPS-Only

Already enabled by default with Container Apps!

### 2. Add Health Checks

Your app already has `/health` endpoint. Configure in Portal:
- Container App → Health probes → Add liveness probe
- Path: `/health`

### 3. Configure Alerts

Set up alerts for:
- High CPU/memory usage
- HTTP 5xx errors
- Container restarts

Azure Portal → Container App → Alerts → New alert rule

### 4. Review Logs Regularly

```bash
# Check for errors
azd logs --service planningpoker-api | grep ERROR

# Monitor performance
azd monitor
```

## Troubleshooting

### Issue: Build Fails

**Solution**: Ensure frontend is built before deploying
```bash
.\build-frontend.ps1
azd deploy
```

### Issue: Container Fails to Start

**Solution**: Check logs for errors
```bash
azd logs --service planningpoker-api --follow
```

Common causes:
- Missing environment variables
- Port configuration issues
- Startup errors in Program.cs

### Issue: 502 Bad Gateway

**Causes**:
- Container is starting up (cold start after scale to zero)
- Container crashed
- Health check failing

**Solution**:
```bash
# Check container status
az containerapp show \
  --name planningpoker-api \
  --resource-group rg-{environment} \
  --query "properties.runningStatus"

# View detailed logs
azd logs --service planningpoker-api --follow
```

### Issue: Frontend Not Loading

**Solution**: Ensure wwwroot has files
```bash
# Rebuild frontend
.\build-frontend.ps1

# Redeploy
azd deploy
```

### Issue: API Not Reachable from Frontend

This shouldn't happen since they're in the same container. If you see CORS errors:
- Check CORS configuration in `Program.cs`
- Ensure `app.UseCors()` is called before endpoints

### Debug Mode

```bash
# Deploy with verbose logging
azd deploy --debug

# SSH into container (if enabled)
az containerapp exec \
  --name planningpoker-api \
  --resource-group rg-{environment}
```

## Clean Up

### Delete All Azure Resources

```bash
azd down
```

This removes:
- Resource group
- All containers
- Container registry
- All logs and data

**Warning**: This is permanent and cannot be undone!

### Keep Infrastructure, Remove Containers

```bash
# Just stop the container apps
az containerapp update \
  --name planningpoker-api \
  --resource-group rg-{environment} \
  --min-replicas 0 \
  --max-replicas 0
```

## Multi-Environment Deployment

### Create Separate Environments:

```bash
# Development
azd env new dev
azd up  # Deploys to dev environment

# Production
azd env new prod
azd up  # Deploys to prod environment

# Switch between environments
azd env select dev
azd env select prod
```

Each environment gets its own resource group and resources.

## Advanced: Database Integration

When you add persistence (future):

### Add Cosmos DB:
```csharp
// In Program.cs AppHost
var cosmos = builder.AddAzureCosmosDB("cosmos")
    .AddDatabase("planningpoker");

var api = builder.AddProject<Projects.PlanningPoker_Api>("planningpoker-api")
    .WithReference(cosmos);
```

Aspire automatically provisions Cosmos DB and injects connection strings!

### Add SQL Database:
```csharp
var sql = builder.AddSqlServer("sql")
    .AddDatabase("planningpokerdb");

var api = builder.AddProject<Projects.PlanningPoker_Api>("planningpoker-api")
    .WithReference(sql);
```

## Support & Resources

- **Aspire Docs**: https://learn.microsoft.com/dotnet/aspire/
- **Azure Container Apps**: https://learn.microsoft.com/azure/container-apps/
- **azd CLI**: https://learn.microsoft.com/azure/developer/azure-developer-cli/

## Quick Reference

```bash
# Build frontend
.\build-frontend.ps1

# Deploy
azd up              # First time or infrastructure changes
azd deploy          # Code changes only

# Monitor
azd logs --follow   # Live logs
azd monitor         # Application Insights
azd show            # Endpoints

# Manage
azd env list        # List environments
azd env select dev  # Switch environment
azd down            # Delete everything
```

That's it! Your Planning Poker app is deployed using .NET Aspire's native Azure publishing. 🚀
