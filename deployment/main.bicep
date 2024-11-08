resource storageAccount 'Microsoft.Storage/storageAccounts@2021-02-01' = {
  name: 'storageAccountName'
  location: 'WestEurope'
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
}

resource functionApp 'Microsoft.Web/sites@2021-02-01' = {
  name: 'functionAppName'
  location: 'WestEurope'
  kind: 'functionapp'
  properties: {
    serverFarmId: 'hostingPlan'
  }
}
