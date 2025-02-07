# parameter for resource group
param (
    [string]$resourceGroup,
    [string]$location,
    [string]$prefix
)

# set variables
$solutionDirectory = Resolve-Path "../AzureWeatherJobs"
$functionAppProjects = @(
    "StartWeatherImageJob",
    "FetchGeneratedImages",
    "ImageGenerationJob",
    "ProcessWeatherImageJob"
)
$outputDirectory = "./publish"
$bicepFile = "main.bicep"

foreach ($project in $functionAppProjects) {
    Write-Host "Publishing function app: $project"
    
    # construct the project path and .csproj file path
    $projectPath = Join-Path $solutionDirectory $project
    $csprojFile = "$project.csproj"

    # check the full path
    $fullCsprojPath = Join-Path $projectPath $csprojFile
    Write-Host "Expected .csproj path for ${project}: $fullCsprojPath"
    if (!(Test-Path $fullCsprojPath)) {
        Write-Host "Project file not found: $fullCsprojPath. Skipping project."
        continue
    }

    # publish the project
    dotnet publish $fullCsprojPath -c Release -o "$outputDirectory/$project"
    
    # check if publish was successful
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to publish $project. Exiting."
        exit $LASTEXITCODE
    }

    # verify the published directory exists
    $publishedDir = "$outputDirectory/$project"
    if (!(Test-Path $publishedDir)) {
        Write-Host "Publish output not found for $project at $publishedDir. Skipping zipping."
        continue
    }

    # create a zip package for deployment, including hidden files
    Write-Host "Creating zip package for: $project"
    $sourcePath = "$outputDirectory/$project"
    $destinationPath = "$outputDirectory/$project.zip"

    # gather all non-hidden files and directories, plus explicitly add .azurefunctions folder
    $itemsToZip = Get-ChildItem -Path "$sourcePath\*" -Recurse -Force | Where-Object { 
        -not $_.Attributes.HasFlag([System.IO.FileAttributes]::Hidden) -or $_.FullName -like "*.azurefunctions*" 
    }

    # explicitly include .azurefunctions if it exists
    $azureFunctionsFolder = Join-Path $sourcePath ".azurefunctions"
    if (Test-Path $azureFunctionsFolder) {
        $itemsToZip += Get-ChildItem -Path $azureFunctionsFolder -Recurse -Force
    }

    # remove duplicate paths by selecting unique FullNames
    $itemsToZip = $itemsToZip | Sort-Object FullName -Unique

    # compress the selected unique items into the zip file
    $itemsToZip | Compress-Archive -DestinationPath $destinationPath -Update

    # verify that the zip file was created
    if (!(Test-Path $destinationPath)) {
        Write-Host "Failed to create zip package for $project. Skipping deployment."
        continue
    }
}

# deploy each function app
foreach ($project in $functionAppProjects) {
    $functionAppName = "$prefix$project"
    Write-Host "Deploying function app: $functionAppName"
    
    # check if the zip file exists before attempting deployment
    $zipPath = "$outputDirectory/$project.zip"
    if (!(Test-Path $zipPath)) {
        Write-Host "Deployment zip not found for $project at $zipPath. Skipping deployment."
        continue
    }

    # deploy using the zip package
    az functionapp deployment source config-zip --name $functionAppName --resource-group $resourceGroup --src $zipPath

    # check if the deployment was successful
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to deploy $functionAppName. Exiting."
        exit $LASTEXITCODE
    }
}

Write-Host "All function apps published and deployed successfully!"