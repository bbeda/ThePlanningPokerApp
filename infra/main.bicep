targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the environment that can be used as part of naming resource convention.')
param environmentName string

@minLength(1)
@description('Primary location for all resources.')
param location string

@description('Id of the principal to assign database and application roles.')
param principalId string = ''

// Container image for the API (will be replaced by azd)
param planningpokerApiContainerImage string = ''

// Tags that should be applied to all resources.
var tags = {
  'azd-env-name': environmentName
}

resource rg 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: 'rg-${environmentName}'
  location: location
  tags: tags
}

// Container Registry
module acr './planningpoker-env-acr.module.bicep' = {
  name: 'planningpoker-env-acr'
  scope: rg
  params: {
    location: location
  }
}

// Container Apps Environment
module env './planningpoker-env.module.bicep' = {
  name: 'planningpoker-env'
  scope: rg
  params: {
    location: location
    planningpoker_env_acr_outputs_name: acr.outputs.name
    userPrincipalId: principalId
    tags: tags
  }
}

// API Container App
module api './planningpoker-api-containerapp.module.bicep' = {
  name: 'planningpoker-api'
  scope: rg
  params: {
    location: location
    planningpoker_env_outputs_azure_container_apps_environment_id: env.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_ID
    planningpoker_env_outputs_azure_container_apps_environment_default_domain: env.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN
    planningpoker_env_outputs_azure_container_registry_endpoint: env.outputs.AZURE_CONTAINER_REGISTRY_ENDPOINT
    planningpoker_env_outputs_azure_container_registry_managed_identity_id: env.outputs.AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID
    planningpoker_api_containerimage: planningpokerApiContainerImage != '' ? planningpokerApiContainerImage : 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'
    planningpoker_api_containerport: '8080'
  }
}

// Outputs for azd
output AZURE_CONTAINER_REGISTRY_ENDPOINT string = env.outputs.AZURE_CONTAINER_REGISTRY_ENDPOINT
output AZURE_CONTAINER_REGISTRY_NAME string = env.outputs.AZURE_CONTAINER_REGISTRY_NAME
output AZURE_CONTAINER_APPS_ENVIRONMENT_NAME string = env.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_NAME
output AZURE_CONTAINER_APPS_ENVIRONMENT_ID string = env.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_ID
output AZURE_RESOURCE_GROUP string = rg.name
