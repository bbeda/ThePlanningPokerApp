# Azure Deployment - Quick Start (.NET Aspire)

## Prerequisites

- Azure subscription
- Azure Developer CLI: `winget install microsoft.azd`
- Docker Desktop running

## First Time Deployment

```bash
# 1. Login
azd auth login

# 2. Build frontend
cd "D:\source\Planning Poker"
.\build-frontend.ps1

# 3. Initialize (Aspire will auto-generate infrastructure!)
azd init
# Choose: "Use code in current directory"
# Confirm Aspire detection: Yes
# Environment name: prod (or dev)

# 4. Deploy
azd up
# Select subscription
# Select location (e.g., eastus)
# Wait ~10-15 minutes
```

## Daily Workflow

```bash
# Deploy code changes
.\build-frontend.ps1
azd deploy

# View logs
azd logs --follow

# Show endpoint URL
azd show
```

## What You Get

- Single Container App (API + Frontend in one container)
- Auto-scaling (0-10 replicas)
- HTTPS with managed certificate
- Application Insights monitoring
- Cost: ~$5-20/month (scale-to-zero enabled)

## Architecture

```
Internet → planningpoker-api Container App
           ├─ /api/* → .NET Backend
           └─ /* → Vue.js Frontend (static files)
```

## Key Commands

```bash
azd up          # Deploy everything
azd deploy      # Code changes only
azd logs        # View logs
azd down        # Delete all resources
azd monitor     # Open Application Insights
```

## Important Notes

1. **Always build frontend before deploying**:
   ```bash
   .\build-frontend.ps1
   ```

2. **Frontend is served from API's wwwroot** - no separate container needed!

3. **Aspire auto-generates Bicep** from your AppHost - no manual infrastructure!

4. **Development vs Production**:
   - Dev: AppHost runs separate Vite dev server
   - Prod: AppHost serves built frontend from wwwroot

## Multi-Environment

```bash
# Create environments
azd env new dev
azd env new staging
azd env new prod

# Switch and deploy
azd env select prod
azd up
```

## Troubleshooting

```bash
# Detailed logs
azd logs --service planningpoker-api --follow

# Debug mode
azd deploy --debug

# Rebuild everything
.\build-frontend.ps1
azd up
```

## Clean Up

```bash
# Delete all Azure resources
azd down
```

---

For detailed guide, see **DEPLOYMENT.md**
