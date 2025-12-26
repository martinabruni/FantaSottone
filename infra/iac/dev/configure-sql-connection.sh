#!/bin/bash
# Script per configurare la Connection String della Web API dopo il primo deployment

echo "🔧 Configurazione Connection String per Web API..."

# Ottieni i valori da Terraform
RESOURCE_GROUP=$(terraform output -raw resource_group_name)
WEB_API_NAME=$(terraform output -raw web_api_name)
KEY_VAULT_NAME=$(terraform output -raw key_vault_name)

# Ottieni l'URI del segreto dal Key Vault
SECRET_ID=$(az keyvault secret show \
  --vault-name "$KEY_VAULT_NAME" \
  --name "SqlConnectionString" \
  --query "id" -o tsv)

echo "📝 Configurazione app setting con riferimento a Key Vault..."

# Configura l'app setting della Web API
az webapp config appsettings set \
  --resource-group "$RESOURCE_GROUP" \
  --name "$WEB_API_NAME" \
  --settings SqlConnectionString="@Microsoft.KeyVault(SecretUri=$SECRET_ID)"

echo "✅ Configurazione completata!"
echo ""
echo "🔍 Verifica la configurazione:"
echo "az webapp config appsettings list -g $RESOURCE_GROUP -n $WEB_API_NAME --query \"[?name=='SqlConnectionString']\""
