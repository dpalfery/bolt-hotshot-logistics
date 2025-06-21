output "resource_group_name" {
  description = "The name of the resource group"
  value       = azurerm_resource_group.launchpad.name
}

output "storage_account_name" {
  description = "The name of the storage account"
  value       = azurerm_storage_account.remote_state.name
} 