param (
    [string]$subscriptionId,
    [string]$webAppName,
    [string]$resourceGroup,
    [string]$registryUrl,
    [string]$base64DockerCompose,
    [string]$mainServiceName,  # Parameter for the main service name (optional)
    [string]$targetPort  # Additional parameter for target port
)

# Help option to explain the parameters needed to run this script
if ($subscriptionId -eq "help" -or $webAppName -eq "help" -or $resourceGroup -eq "help" -or $registryUrl -eq "help" -or $base64DockerCompose -eq "help" -or $mainServiceName -eq "help" -or $targetPort -eq "help") {
    Write-Output "`nThis script updates Azure Web App with Docker Compose configuration and supports different authentication types."
    Write-Output "`n"
    Write-Output "`nParameters:"
    Write-Output "`n  subscriptionId               : Azure Subscription ID"
    Write-Output "`n  webAppName                   : Name of the Azure Web App"
    Write-Output "`n  resourceGroup                : Resource Group of the Azure Web App"
    Write-Output "`n  registryUrl                  : URL of the Docker Registry"
    Write-Output "`n  base64DockerCompose          : Base64 encoded Docker Compose YAML"
    Write-Output "`n  mainServiceName              : Name of the main service in Docker Compose (optional)"
    Write-Output "`n  targetPort                   : Target port for the services"
    Write-Output "`n"
    Write-Output "`nCommand Format:"
    Write-Output "`n./update-webapp.ps1 -subscriptionId <subscriptionId> -webAppName <webAppName> -resourceGroup <resourceGroup> -registryUrl <registryUrl> -base64DockerCompose <base64DockerCompose> -mainServiceName <mainServiceName> -targetPort <targetPort>"
    Write-Output "`n"
    Write-Output "`nSample Command:"
    Write-Output "`n./update-webapp.ps1 -subscriptionId 'your_subscription_id' -webAppName 'your_web_app_name' -resourceGroup 'your_resource_group' -registryUrl 'your_registry_url' -base64DockerCompose 'your_base64_docker_compose' -mainServiceName 'your_main_service_name' -targetPort 'your_target_port'"
    exit
}

# Function to convert SecureString to plain text
function Convert-SecureStringToPlainText {
    param (
        [SecureString]$secureString
    )
    $ptr = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureString)
    try {
        return [System.Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr)
    }
    finally {
        [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr)
    }
}

# Login to Azure and select subscription
Write-Output "`nLogging in to Azure..."
az login
Write-Output "`nSetting subscription to $subscriptionId..."
az account set --subscription $subscriptionId

# Check if the staging slot exists
$slotExists = az webapp deployment slot list --name $webAppName --resource-group $resourceGroup --query "[?name=='staging']" --output tsv

if (-not $slotExists) {
    # Create a staging slot if it doesn't exist
    Write-Output "`nCreating staging slot as it doesn't exist..."
    az webapp deployment slot create --name $webAppName --resource-group $resourceGroup --slot staging --configuration-source $webAppName
} else {
    Write-Output "`nSlot \"staging" already exists, modifying it.."
}

# Print the base64 Docker Compose YAML before decoding
Write-Output "`nBase64 Docker Compose YAML: $base64DockerCompose"

# Decode base64 value to get Docker Compose YAML
Write-Output "`nDecoding base64 Docker Compose YAML..."
$dockerComposeYaml = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($base64DockerCompose))
$dockerComposeYaml = $dockerComposeYaml -replace "(\r?\n){2,}", "`n"
Write-Output "`nDecoded Docker Compose YAML:`n$dockerComposeYaml"
Write-Output "`nDocker Compose YAML decoded"

# Check if powershell-yaml module is installed, install it if missing
if (-not (Get-Module -ListAvailable -Name powershell-yaml)) {
    Write-Output "`nInstalling powershell-yaml module..."
    Install-Module -Name powershell-yaml -Force -Scope CurrentUser
}
Import-Module powershell-yaml

# Parse Docker Compose YAML and extract image details
$dockerCompose = ConvertFrom-Yaml -Yaml $dockerComposeYaml

# Define the unsupported fields and their corresponding messages
$unsupportedFields = @{
    "build" = "Use pre-built images from a container registry. Refer - https://mcr.microsoft.com/"
    "network" = "Use Azure Virtual Network integration features instead. Refer - https://learn.microsoft.com/en-us/azure/app-service/overview-vnet-integration"
    "depends_on" = "Ensure the dependent services are running before deploying the container."
    "restart" = "Implement custom restart logic within the container or use Azure's scaling features. Refer - https://learn.microsoft.com/en-us/azure/container-apps/scale-app?pivots=azure-cli"
    "health_check" = "Use Azure Application Insights or other monitoring tools for health checks. Refer - Application Insights overview - Azure Monitor | Microsoft Learn "
    "ulimits" = "Configure resource limits directly in the container or use Azure's resource management features. Refer - https://learn.microsoft.com/en-us/azure/azure-resource-manager/management/overview"
    "secrets" = "Use Azure Key Vault to manage secrets securely. Refer - https://azure.microsoft.com/en-us/products/key-vault"
}

# Iterate over each service and remove unsupported fields
foreach ($service in $dockerCompose.services.Values) {
    foreach ($field in $unsupportedFields.Keys) {
        if ($service.PSObject.Properties[$field]) {
            $service.PSObject.Properties.Remove($field)
            Write-Host "$field is not supported. $($unsupportedFields[$field])"
        }
    }
}

# Initialize an empty hashtable to store service names and images
$services = @{}

foreach ($serviceName in $dockerCompose.services.Keys) {
    $image = $dockerCompose.services[$serviceName].image
    if ($image -notmatch "/") {
        $image = "index.docker.io/$image"  # Prepend DockerHub FQDN if no registry is specified
    }
    $isMain = $false
    $startUpCommand = $dockerCompose.services[$serviceName].startUpCommand
    if ($mainServiceName -and $serviceName -eq $mainServiceName) {
        $isMain = $true
    }

    # Extract volume mounts from the configuration
    $volumeMounts = @()
    if ($null -ne $dockerCompose.services[$serviceName].volumes) {
        $volumeMounts = @($dockerCompose.services[$serviceName].volumes | ForEach-Object {
            $volumeSubPath = $_.Split(':')[0]
            $containerMountPath = $_.Split(':')[1]
            if ($volumeSubPath -and $containerMountPath -and $volumeSubPath -notmatch '{WEBAPP_STORAGE_HOME}|{WEBSITES_ENABLED_APP_SERVICE_STORAGE}') {
                @{
                    volumeSubPath = $volumeSubPath
                    containerMountPath = $containerMountPath
                    readOnly = $false
                }
            }
        })
    }

    # Store service details in the hashtable
    $services[$serviceName] = @{
        image = if ($image) { $image } else { "" }
        startUpCommand = if ($startUpCommand) { $startUpCommand } else { "" }
        isMain = if ($isMain) { $isMain } else { $false }
        volumeMounts = $volumeMounts
        targetPort = if ($isMain -and $targetPort) { $targetPort } else { "" }
    }
}

# Print the parsed services and images for debugging
Write-Output "`nParsed services and images:"
foreach ($service in $services.Keys) {
    $image = $services[$service].image
    $isMain = $services[$service].isMain
    $targetPort = if ($isMain -and $services[$service].targetPort) { "$($services[$service].targetPort)" } else { "" }
    Write-Output ("Found service: {0}, Image: {1}, IsMain: {2}, TargetPort: {3}" -f $service, $image, $isMain, $targetPort)
}

Write-Output "`nFetching web app information.."
# Fetch web app information
$webAppInfo = az webapp show --resource-group $resourceGroup --name $webAppName | ConvertFrom-Json
$linuxFxVersion = $webAppInfo.siteConfig.linuxFxVersion
$acrUseManagedIdentityCreds = $webAppInfo.siteConfig.acrUseManagedIdentityCreds
$acrUserManagedIdentityID = $webAppInfo.siteConfig.acrUserManagedIdentityID

# Parse docker image from LinuxFxVersion
$authType = "Anonymous"
$userName = $null
$passwordSecret = $null
$userManagedIdentityClientId = $null

Write-output $userManagedIdentityClientId
if ($acrUseManagedIdentityCreds -eq "true") {
    Write-Output "`nACR is using managed identity credentials."

    # Check if the main site has a user-assigned managed identity
    $mainSiteUserAssignedIdentity = az webapp identity show --resource-group $resourceGroup --name $webAppName --query "userAssignedIdentities" --output tsv

    if ($mainSiteUserAssignedIdentity) {
        Write-Output "Main site is using a user-assigned managed identity."

        # Retrieve the fully qualified resource ID for the user-assigned managed identity
        $userAssignedIdentityResourceId = az identity show --resource-group $resourceGroup --name $acrUserManagedIdentityID --query id --output tsv
        Write-Output "`nACR User Managed Identity ID: $userAssignedIdentityResourceId"
        $authType = "UserAssigned"
        $userManagedIdentityClientId = $userAssignedIdentityResourceId

        # Assign the user-assigned managed identity to the staging slot
        Write-Output "`nAssigning user-assigned managed identity to the staging slot..."
        az webapp identity assign --resource-group $resourceGroup --name $webAppName --slot staging --identities $userAssignedIdentityResourceId

        # Grant permissions to the user-assigned managed identity
        Write-Output "`nGranting permissions to the user-assigned managed identity..."
        az role assignment create --assignee $userAssignedIdentityResourceId --role "AcrPull" --scope "/subscriptions/$subscriptionId/resourceGroups/$resourceGroup/providers/Microsoft.ContainerRegistry/registries/sitecontainerssampleacr"
    } else {
        Write-Output "Main site is using a system-assigned managed identity."
        $authType = "SystemIdentity"

        # Enable system-assigned managed identity for the staging slot
        Write-Output "`nEnabling system-assigned managed identity for the staging slot..."
        az webapp identity assign --resource-group $resourceGroup --name $webAppName --slot staging

        # Retrieve the principal ID for the system-assigned managed identity of the staging slot
        $systemAssignedIdentityPrincipalIdStaging = (az webapp identity show --resource-group $resourceGroup --name $webAppName --slot staging --query principalId --output tsv)
        Write-Output "`nStaging slot system-assigned managed identity principal ID: $systemAssignedIdentityPrincipalIdStaging"

        # Grant permissions to the system-assigned managed identity of the staging slot
        Write-Output "`nGranting permissions to the system-assigned managed identity of the staging slot..."
        az role assignment create --assignee $systemAssignedIdentityPrincipalIdStaging --role "AcrPull" --scope "/subscriptions/$subscriptionId/resourceGroups/$resourceGroup/providers/Microsoft.ContainerRegistry/registries/sitecontainerssampleacr"
    }
} else {
    Write-Output "`nACR is not using managed identity credentials, fetching user credentials."
    $appSettings = az webapp config appsettings list --resource-group $resourceGroup --name $webAppName | ConvertFrom-Json
    $dockerRegistryServerUsername = ($appSettings | Where-Object { $_.name -eq "DOCKER_REGISTRY_SERVER_USERNAME" }).value
    $dockerRegistryServerPassword = ($appSettings | Where-Object { $_.name -eq "DOCKER_REGISTRY_SERVER_PASSWORD" }).value

    # Prompt for username and password if not set
    if (-not $dockerRegistryServerUsername) {
        $dockerRegistryServerUsername = Read-Host "Please provide the Docker registry server username"
    }
    if (-not $dockerRegistryServerPassword) {
        $dockerRegistryServerPassword = Read-Host -Prompt "Please provide the Docker registry server password" -AsSecureString
        # Convert SecureString to plain text
        $plainTextPassword = Convert-SecureStringToPlainText -secureString $dockerRegistryServerPassword
        $passwordSecret = $plainTextPassword
    }
    if ($dockerRegistryServerUsername) {
        Write-Output "`nACR is using user credentials."
        $authType = "UserCredentials"
        $userName = $dockerRegistryServerUsername
    }
}

# Generate the sitecontainers spec JSON file
$siteContainersSpec = @()

foreach ($service in $services.Keys) {
    $containerSpec = @{
        name = $service
        properties = @{
            image = $services[$service].image
            targetPort = if ($services[$service].targetPort) { $services[$service].targetPort } else { $null }
            isMain = $services[$service].isMain
        }
    }

    # Add environment variables if any (replace with actual logic if needed)
    if ($services[$service].environmentVariables) {
        $containerSpec.properties.environmentVariables = @(
            $services[$service].environmentVariables | ForEach-Object {
                @{
                    name = $_.name
                    value = $_.value
                }
            }
        )
    }

    # Add volume mounts if any
    if ($services[$service].volumeMounts) {
        $containerSpec.properties.volumeMounts = @(
            $services[$service].volumeMounts | ForEach-Object {
                @{
                    containerMountPath = $_.containerMountPath
                    readOnly = $_.readOnly
                    volumeSubPath = $_.volumeSubPath
                }
            }
        )
    }

    # Add authentication type and related properties
    if ($authType -eq "SystemIdentity") {
        $containerSpec.properties.authType = "SystemIdentity"
    } elseif ($authType -eq "UserAssigned") {
        $containerSpec.properties.authType = "UserAssigned"
        $containerSpec.properties.userManagedIdentityClientId = $userManagedIdentityClientId
    } elseif ($authType -eq "UserCredentials") {
        $containerSpec.properties.authType = "UserCredentials"
        $containerSpec.properties.userName = $userName
        $containerSpec.properties.passwordSecret = $passwordSecret
    }

    # Add startup command if available
    if ($services[$service].startUpCommand) {
        $containerSpec.properties.startUpCommand = $services[$service].startUpCommand
    }

    $siteContainersSpec += $containerSpec
}

# Convert the spec to JSON and save it to a temporary file
$siteContainersSpecJson = ConvertTo-Json -InputObject $siteContainersSpec -Depth 10 -Compress
$tempSpecFilePath = "$env:TEMP\sitecontainersspec.json"
Set-Content -Path $tempSpecFilePath -Value $siteContainersSpecJson

Write-Output "`nGenerated sitecontainers spec JSON file at ${tempSpecFilePath}:"
Write-Output $siteContainersSpecJson

# Use the az CLI command to create the sitecontainers
Write-Output "`nCreating sitecontainers using az CLI..."
az webapp sitecontainers create `
    --name $webAppName `
    --resource-group $resourceGroup `
    --slot staging `
    --sitecontainers-spec-file $tempSpecFilePath

# Update linuxFxVersion to sitecontainers
Write-Output "`nUpdating linuxFxVersion to sitecontainers for the staging slot..."
$retryCount = 0
$maxRetries = 3
do {
    az webapp config set --resource-group $resourceGroup --name $webAppName --slot staging --linux-fx-version "sitecontainers"
    Start-Sleep -Seconds 5
    $currentLinuxFxVersion = az webapp config show --resource-group $resourceGroup --name $webAppName --slot staging --query "linuxFxVersion" --output tsv
    if ($currentLinuxFxVersion -eq "sitecontainers") {
        Write-Output "`nSuccessfully updated linuxFxVersion to sitecontainers."
        break
    }
    $retryCount++
    Write-Output "`nRetrying to update linuxFxVersion... Attempt $retryCount of $maxRetries."
} while ($retryCount -lt $maxRetries)

if ($currentLinuxFxVersion -ne "sitecontainers") {
    Write-Output "`nFailed to update linuxFxVersion to sitecontainers after $maxRetries attempts."
    exit 1
}

# Clean up the temporary JSON file
Remove-Item -Path $tempSpecFilePath -Force

# Restart the web app
Write-Output "`nRestarting the web app $webAppName..."
az webapp restart --name $webAppName --resource-group $resourceGroup --slot "staging"
Write-Output "`nWeb app $webAppName restarted"