# Login to Azure
az login

# Set variables
$resourceGroup = "yourResourceGroupName"
$location = "WestEurope"
$functionAppName = "yourFunctionAppName"

# Deploy Bicep template
az deployment group create --resource-group $resourceGroup --template-file main.bicep

# Publish function app
dotnet publish --output ./publish
func azure functionapp publish $functionAppName --python

Write-Host "Deployment complete."
