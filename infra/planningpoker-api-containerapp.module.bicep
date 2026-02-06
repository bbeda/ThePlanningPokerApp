@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param planningpoker_env_outputs_azure_container_apps_environment_default_domain string

param planningpoker_env_outputs_azure_container_apps_environment_id string

param planningpoker_api_containerimage string

param planningpoker_api_containerport string

param planningpoker_env_outputs_azure_container_registry_endpoint string

param planningpoker_env_outputs_azure_container_registry_managed_identity_id string

resource planningpoker_api 'Microsoft.App/containerApps@2025-02-02-preview' = {
  name: 'planningpoker-api'
  location: location
  tags: {
    'azd-service-name': 'planningpoker-api'
  }
  properties: {
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: int(planningpoker_api_containerport)
        transport: 'http'
      }
      registries: [
        {
          server: planningpoker_env_outputs_azure_container_registry_endpoint
          identity: planningpoker_env_outputs_azure_container_registry_managed_identity_id
        }
      ]
      runtime: {
        dotnet: {
          autoConfigureDataProtection: true
        }
      }
    }
    environmentId: planningpoker_env_outputs_azure_container_apps_environment_id
    template: {
      containers: [
        {
          image: planningpoker_api_containerimage
          name: 'planningpoker-api'
          env: [
            {
              name: 'OTEL_DOTNET_EXPERIMENTAL_OTLP_EMIT_EXCEPTION_LOG_ATTRIBUTES'
              value: 'true'
            }
            {
              name: 'OTEL_DOTNET_EXPERIMENTAL_OTLP_EMIT_EVENT_LOG_ATTRIBUTES'
              value: 'true'
            }
            {
              name: 'OTEL_DOTNET_EXPERIMENTAL_OTLP_RETRY'
              value: 'in_memory'
            }
            {
              name: 'ASPNETCORE_FORWARDEDHEADERS_ENABLED'
              value: 'true'
            }
            {
              name: 'HTTP_PORTS'
              value: planningpoker_api_containerport
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${planningpoker_env_outputs_azure_container_registry_managed_identity_id}': { }
    }
  }
}
