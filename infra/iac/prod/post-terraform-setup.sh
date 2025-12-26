#!/bin/bash
#
# Script da eseguire DOPO terraform apply
# Configura la connection string nell'app setting della Web API
#
set -e

ENVIRONMENT="${1:-dev}"

if [ "$ENVIRONMENT" = "dev" ]; then
    RESOURCE_GROUP="rg-fantastn-webapp-dev"
    WEB_API_NAME="api-fantastn-backend-dev"
    KEY_VAULT_NAME="kv-fantastn-app-dev"
elif [ "$ENVIRONMENT" = "prod" ]; then
    RESOURCE_GROUP="rg-fantastn-webapp-prod"
    WEB_API_NAME="api-fantastn-backend-prod"
    KEY_VAULT_NAME="kv-fantastn-app-prod"
else
    echo "❌ Ambiente non valido: $ENVIRONMENT"
    echo "Uso: ./post-terraform-setup.sh [dev|prod]"
    exit 1
fi

echo "🔧 Post-Terraform Setup"
echo "======================="
echo ""
echo "📍 Ambiente: $ENVIRONMENT"
echo "🔐 Key Vault: $KEY_VAULT_NAME"
echo "🌐 Web API: $WEB_API_NAME"
echo ""

# Ottieni l'URI del secret
echo "📝 Recupero URI del secret..."
SECRET_ID=$(az keyvault secret show \
    --vault-name "$KEY_VAULT_NAME" \
    --name "SqlConnectionString" \
    --query "id" -o tsv)

if [ -z "$SECRET_ID" ]; then
    echo "❌ Secret SqlConnectionString non trovato"
    exit 1
fi

echo "✅ Secret URI: $SECRET_ID"
echo ""

# Configura app setting
echo "⚙️  Configurazione app setting..."
az webapp config appsettings set \
    --resource-group "$RESOURCE_GROUP" \
    --name "$WEB_API_NAME" \
    --settings SqlConnectionString="@Microsoft.KeyVault(SecretUri=$SECRET_ID)" \
    --output none

echo "✅ App setting configurato"
echo ""

# Riavvia Web API
echo "🔄 Riavvio Web API..."
az webapp restart \
    --resource-group "$RESOURCE_GROUP" \
    --name "$WEB_API_NAME" \
    --output none

echo "✅ Web API riavviata"
echo ""

# Ottieni URL
WEB_API_URL=$(az webapp show \
    --resource-group "$RESOURCE_GROUP" \
    --name "$WEB_API_NAME" \
    --query "defaultHostName" -o tsv)

echo "✅ Setup completato!"
echo ""
echo "🧪 Test l'API:"
echo "   curl https://$WEB_API_URL/health"
echo ""

# Health check automatico
read -p "🧪 Vuoi eseguire un health check ora? [y/N] " -n 1 -r
echo ""

if [[ $REPLY =~ ^[Yy]$ ]]; then
    echo "⏳ Attendo 15 secondi..."
    sleep 15
    
    for i in {1..5}; do
        HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" "https://$WEB_API_URL/health" || echo "000")
        
        if [ "$HTTP_CODE" = "200" ]; then
            echo "✅ Backend HEALTHY (HTTP $HTTP_CODE)"
            echo "🎉 Tutto funziona!"
            exit 0
        else
            echo "⏳ Attempt $i/5 - HTTP $HTTP_CODE"
            sleep 5
        fi
    done
    
    echo "❌ Health check fallito"
    echo "🔍 Controlla i logs:"
    echo "   az webapp log tail -g $RESOURCE_GROUP -n $WEB_API_NAME"
    exit 1
fi
