# Project README: Azure Functions Solution

This repository contains an Azure Functions solution consisting of several independent projects, each serving a specific purpose. Below is a detailed description of the solution and its components.

## Table of Contents
- [Overview](#overview)
- [Solution Structure](#solution-structure)
- [Project Details](#project-details)
  - [FetchGeneratedImages](#fetchgeneratedimages)
  - [ImageGenerationJob](#imagegenerationjob)
  - [ProcessWeatherImageJob](#processweatherimagejob)
  - [StartWeatherImageJob](#startweatherimagejob)
- [Setup and Configuration](#setup-and-configuration)
- [Deployment Instructions](#deployment-instructions)

---

## Overview
This solution consists of multiple Azure Function projects designed to perform specific tasks, such as handling image generation, processing weather data, and fetching generated images. These functions are written in .NET and are intended to work together within the Azure cloud environment.

## Solution Structure
Below is the folder structure of the solution:

```plaintext
AzureWeatherJobs/
|-- FetchGeneratedImages/
|   |-- FunctionApp.cs
|   |-- local.settings.json
|   |-- ...
|
|-- ImageGenerationJob/
|   |-- FunctionApp.cs
|   |-- local.settings.json
|   |-- ...
|
|-- ProcessWeatherImageJob/
|   |-- FunctionApp.cs
|   |-- local.settings.json
|   |-- ...
|
|-- StartWeatherImageJob/
    |-- FunctionApp.cs
    |-- local.settings.json
    |-- ...

deployment/
|-- deploy.ps1/
|-- main.bicep/

http/
|-- generated-images.http
|-- start-job.http
```

## Project Details

### FetchGeneratedImages
- **Purpose**: Handles the retrieval of previously generated images from Azure storage.
- **Key Files**:
  - `FunctionApp.cs`: Main function logic.
  - `local.settings.json`: Local configuration settings for development.

### ImageGenerationJob
- **Purpose**: Manages the image generation process by triggering jobs based on input parameters.
- **Key Files**:
  - `FunctionApp.cs`: Contains the logic for starting and monitoring image generation jobs.
  - `local.settings.json`: Development-specific settings.

### ProcessWeatherImageJob
- **Purpose**: Processes images related to weather data by applying overlays.
- **Key Files**:
  - `FunctionApp.cs`: Implements image processing workflows.
  - `local.settings.json`: Environment-specific configurations for local testing.

### StartWeatherImageJob
- **Purpose**: Orchestrates the start of a weather-related image generation job, handling input data and queuing processes.
- **Key Files**:
  - `FunctionApp.cs`: Core logic for initializing weather image generation.
  - `local.settings.json`: Development configurations.

## Setup and Configuration
1. **Prerequisites**:
   - .NET SDK installed on your development machine.
   - Azure CLI for deploying and managing Azure resources.
   - Azure Storage Account for handling function triggers and data storage.

2. **Local Development**:
   - Clone the repository: `git clone <repository-url>`
   - Navigate to each project folder and restore dependencies: `dotnet restore`
   - Update `local.settings.json` with configuration values.
   - Start the Azure Functions runtime locally: `func start`

3. **Configuration Files**:
   - `host.json`: Used to define global function app settings.
   - `local.settings.json`: Stores local development settings (not included in deployment).

## Deployment Instructions
1. **Publish to Azure**:
   - Build the project: `dotnet build`
   - Deploy using Azure CLI:
     ```bash
     ./deploy.ps1 -resourceGroup <Name of resource group> -location <Azure region location> -prefix <Prefix of the resources>

     ```

2. **Configuration in Azure**:
   - Ensure all required Application Settings are defined in the Azure Portal.
   - Link to the appropriate Azure Storage Account and other resources.

3. **Testing**:
   - Use tools like Postman or curl to test HTTP-triggered functions.
   - Monitor logs in the Azure Portal or via Azure CLI.

