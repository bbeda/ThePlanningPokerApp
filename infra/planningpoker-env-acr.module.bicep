@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

resource planningpoker_env_acr 'Microsoft.ContainerRegistry/registries@2025-04-01' = {
  name: take('planningpokerenvacr${uniqueString(resourceGroup().id)}', 50)
  location: location
  sku: {
    name: 'Basic'
  }
  tags: {
    'aspire-resource-name': 'planningpoker-env-acr'
  }
}

output name string = planningpoker_env_acr.name

output loginServer string = planningpoker_env_acr.properties.loginServer