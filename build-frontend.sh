#!/bin/bash
# Build frontend for production deployment
echo "Building Vue.js frontend..."

cd src/PlanningPoker.Web

# Install dependencies if needed
if [ ! -d "node_modules" ]; then
    echo "Installing npm dependencies..."
    npm ci
fi

# Build for production
echo "Building frontend assets..."
npm run build

echo "[SUCCESS] Frontend built successfully to src/PlanningPoker.Api/wwwroot"

cd ../..
