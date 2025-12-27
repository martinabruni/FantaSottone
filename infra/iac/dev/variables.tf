variable "resource_group_name" {
  description = "Nome del Resource Group"
  type        = string
  default     = "rg-webapp-prod"
}

variable "location" {
  description = "Location Azure per le risorse"
  type        = string
  default     = "West Europe"
}

variable "static_app_location" {
  description = "Location per la Static Web App"
  type        = string
  default     = "West Europe"
}

variable "static_app_name" {
  description = "Nome della Static Web App"
  type        = string
  default     = "stapp-frontend-prod"
}

variable "static_app_sku_tier" {
  description = "SKU tier per la Static Web App"
  type        = string
  default     = "Free"
}

variable "static_app_sku_size" {
  description = "SKU size per la Static Web App"
  type        = string
  default     = "Free"
}

variable "app_service_plan_name" {
  description = "Nome dell'App Service Plan"
  type        = string
  default     = "asp-api-prod"
}

variable "app_service_plan_sku" {
  description = "SKU dell'App Service Plan (es: B1, S1, P1v2)"
  type        = string
  default     = "B1"
}

variable "web_api_name" {
  description = "Nome della Web API"
  type        = string
  default     = "api-backend-prod"
}

variable "app_insights_name" {
  description = "Nome di Application Insights"
  type        = string
  default     = "appi-webapp-prod"
}

variable "key_vault_name" {
  description = "Nome del Key Vault (deve essere univoco globalmente)"
  type        = string
  default     = "kv-webapp-prod-001"
}

variable "sql_server_name" {
  description = "Nome del SQL Server (deve essere univoco globalmente)"
  type        = string
  default     = "sql-webapp-prod-001"
}

variable "sql_database_name" {
  description = "Nome del SQL Database"
  type        = string
  default     = "sqldb-webapp-prod"
}

variable "sql_database_sku" {
  description = "SKU del SQL Database (es: Basic, S0, S1, P1)"
  type        = string
  default     = "Basic"
}

variable "sql_admin_username" {
  description = "Username amministratore SQL Server"
  type        = string
  default     = "sqladmin"
  sensitive   = true
}

variable "sql_admin_password" {
  description = "Password amministratore SQL Server"
  type        = string
  sensitive   = true
}

variable "tags" {
  description = "Tags da applicare alle risorse"
  type        = map(string)
  default = {
    Environment = "Production"
    Project     = "WebApp"
    ManagedBy   = "Terraform"
  }
}
