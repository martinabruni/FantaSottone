output "resource_group_name" {
  description = "Nome del Resource Group"
  value       = azurerm_resource_group.main.name
}

output "static_web_app_url" {
  description = "URL della Static Web App"
  value       = "https://${azurerm_static_web_app.main.default_host_name}"
}

output "static_web_app_api_key" {
  description = "API key per il deployment della Static Web App"
  value       = azurerm_static_web_app.main.api_key
  sensitive   = true
}

output "web_api_url" {
  description = "URL della Web API"
  value       = "https://${azurerm_linux_web_app.api.default_hostname}"
}

output "web_api_name" {
  description = "Nome della Web API"
  value       = azurerm_linux_web_app.api.name
}

output "key_vault_name" {
  description = "Nome del Key Vault"
  value       = azurerm_key_vault.main.name
}

output "key_vault_uri" {
  description = "URI del Key Vault"
  value       = azurerm_key_vault.main.vault_uri
}

output "sql_server_fqdn" {
  description = "FQDN del SQL Server"
  value       = azurerm_mssql_server.main.fully_qualified_domain_name
}

output "sql_database_name" {
  description = "Nome del SQL Database"
  value       = azurerm_mssql_database.main.name
}

output "application_insights_connection_string" {
  description = "Connection string di Application Insights"
  value       = azurerm_application_insights.main.connection_string
  sensitive   = true
}

output "application_insights_instrumentation_key" {
  description = "Instrumentation key di Application Insights"
  value       = azurerm_application_insights.main.instrumentation_key
  sensitive   = true
}
