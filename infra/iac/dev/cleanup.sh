#!/bin/bash
#
# Script di cleanup per rimuovere completamente l'infrastruttura
# Utile in caso di errori durante il setup o per reset completo
#
# ⚠️ ATTENZIONE: Questo script ELIMINA TUTTO
#
set -e

ENVIRONMENT="${1:-dev}"

if [ "$ENVIRONMENT" = "dev" ]; then
    RESOURCE_GROUP="rg-webapp-dev"
    KEY_VAULT_NAME="kv-webapp-dev-001"
elif [ "$ENVIRONMENT" = "prod" ]; then
    echo "❌ Cleanup di PROD non permesso via script"
    echo "   Per sicurezza, usa: terraform destroy"
    exit 1
else
    echo "❌ Ambiente non valido: $ENVIRONMENT"
    echo "Uso: ./cleanup.sh [dev|prod]"
    exit 1
fi

echo "⚠️  ATTENZIONE: CLEANUP COMPLETO"
echo "=================================="
echo ""
echo "Questo script eliminerà:"
echo "  - Resource Group: $RESOURCE_GROUP"
echo "  - Tutti i servizi al suo interno"
echo "  - Key Vault e tutti i secrets (purge completo)"
echo "  - Terraform state locale"
echo ""

read -p "Sei SICURO di voler procedere? [yes/NO] " -r
echo ""

if [[ ! $REPLY =~ ^[Yy][Ee][Ss]$ ]]; then
    echo "❌ Cleanup annullato"
    exit 0
fi

echo "🗑️  Avvio cleanup..."
echo ""

# ========================================
# 1. Soft Delete del Key Vault (via Terraform destroy)
# ========================================
echo "1️⃣  Terraform destroy..."
if terraform destroy -auto-approve; then
    echo "   ✅ Terraform destroy completato"
else
    echo "   ⚠️  Terraform destroy fallito (potrebbe essere già stato eliminato)"
fi
echo ""

# ========================================
# 2. Purge del Key Vault (elimina anche da soft-delete)
# ========================================
echo "2️⃣  Purge del Key Vault..."

# Verifica se il Key Vault esiste in soft-delete
DELETED_KV=$(az keyvault list-deleted \
    --query "[?name=='$KEY_VAULT_NAME'].name" \
    -o tsv 2>/dev/null || echo "")

if [ -n "$DELETED_KV" ]; then
    echo "   🔍 Key Vault trovato in soft-delete, eseguo purge..."
    
    if az keyvault purge --name "$KEY_VAULT_NAME" --no-wait; then
        echo "   ✅ Purge Key Vault avviato (completamento in background)"
    else
        echo "   ⚠️  Purge fallito (potrebbe richiedere permessi elevati)"
    fi
else
    echo "   ℹ️  Key Vault non trovato in soft-delete"
fi
echo ""

# ========================================
# 3. Verifica eliminazione Resource Group
# ========================================
echo "3️⃣  Verifica eliminazione Resource Group..."

RG_EXISTS=$(az group exists --name "$RESOURCE_GROUP" || echo "false")

if [ "$RG_EXISTS" = "true" ]; then
    echo "   ⚠️  Resource Group ancora esistente, elimino manualmente..."
    
    if az group delete \
        --name "$RESOURCE_GROUP" \
        --yes \
        --no-wait; then
        echo "   ✅ Eliminazione Resource Group avviata"
    else
        echo "   ❌ Eliminazione fallita"
    fi
else
    echo "   ✅ Resource Group già eliminato"
fi
echo ""

# ========================================
# 4. Cleanup Terraform state locale
# ========================================
echo "4️⃣  Cleanup Terraform state locale..."

if [ -f "terraform.tfstate" ]; then
    mv terraform.tfstate terraform.tfstate.backup.$(date +%Y%m%d_%H%M%S)
    echo "   ✅ State file salvato come backup"
fi

if [ -f "terraform.tfstate.backup" ]; then
    mv terraform.tfstate.backup terraform.tfstate.backup.old.$(date +%Y%m%d_%H%M%S)
    echo "   ✅ Backup state salvato"
fi

if [ -f "terraform.tfplan" ]; then
    rm -f terraform.tfplan
    echo "   ✅ Plan file eliminato"
fi

echo ""

# ========================================
# RIEPILOGO
# ========================================
echo "=========================================="
echo "✅ Cleanup completato!"
echo "=========================================="
echo ""
echo "📝 Cosa è stato fatto:"
echo "   ✅ terraform destroy eseguito"
echo "   ✅ Key Vault purge avviato (se esistente)"
echo "   ✅ Resource Group eliminato/in eliminazione"
echo "   ✅ State file locale backuppato"
echo ""
echo "⏳ Nota: L'eliminazione completa potrebbe richiedere alcuni minuti"
echo ""
echo "🧪 Per verificare lo stato:"
echo "   az group exists --name $RESOURCE_GROUP"
echo "   az keyvault list-deleted --query \"[?name=='$KEY_VAULT_NAME']\""
echo ""
echo "🚀 Per rifare il deploy:"
echo "   terraform init"
echo "   terraform plan"
echo "   terraform apply"
echo "   ./post-terraform-setup.sh $ENVIRONMENT"
