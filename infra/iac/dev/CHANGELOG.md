# Changelog - Migrazione a RBAC per Key Vault

## [2.0.0] - 2024-12-26

### 🎉 Changed (BREAKING)
- **Key Vault**: Migrato da Access Policies a RBAC authorization
  - `enable_rbac_authorization = true` ora obbligatorio
  - Rimosso completamente il blocco `access_policy`
  - Tutti i permessi ora gestiti via Azure RBAC

### ✨ Added
- **RBAC Assignments**:
  - `azurerm_role_assignment.kv_admin_current_user`: Utente Terraform → Key Vault Administrator
  - `azurerm_role_assignment.kv_secrets_user_api`: Web API → Key Vault Secrets User
  
- **Scripts**:
  - `verify-keyvault-rbac.sh`: Verifica completa configurazione RBAC
  - `cleanup.sh`: Script di cleanup per reset completo infrastruttura
  - `post-terraform-setup.sh`: Già esistente, ora con supporto RBAC

- **Documentazione**:
  - `README.md`: Guida completa setup e troubleshooting
  - `KEY_VAULT_RBAC_MIGRATION.md`: Guida dettagliata migrazione
  - `terraform.tfvars.example`: Template configurazione
  - `.gitignore`: Per proteggere secrets

### 🔧 Fixed
- Rimossi conflitti tra Access Policies e RBAC
- Aggiunti `depends_on` appropriati per garantire ordine creazione risorse
- Secret creation ora aspetta propagazione RBAC assignments

### 🗑️ Removed
- Blocco `access_policy` dal Key Vault (incompatibile con RBAC)
- Dipendenze implicite che causavano race conditions

### ⚠️ Migration Guide

#### Se hai già un deployment con Access Policies:

**Opzione A - Fresh Deploy (SOLO DEV):**
```bash
# Backup secrets PRIMA di procedere!
./cleanup.sh dev
terraform init
terraform apply
./post-terraform-setup.sh dev
```

**Opzione B - In-Place Migration (PROD):**
```bash
# 1. Crea RBAC assignments
terraform apply -target=azurerm_role_assignment.kv_admin_current_user
terraform apply -target=azurerm_role_assignment.kv_secrets_user_api

# 2. Rimuovi access_policy dal codice

# 3. Apply completo
terraform apply

# 4. Verifica
./verify-keyvault-rbac.sh prod
```

#### Verifica Post-Migrazione:

```bash
# RBAC abilitato?
az keyvault show --name <kv-name> --query "properties.enableRbacAuthorization"
# Deve ritornare: true

# Nessun Access Policy?
az keyvault show --name <kv-name> --query "properties.accessPolicies | length(@)"
# Deve ritornare: 0

# Role assignments corretti?
./verify-keyvault-rbac.sh dev
```

---

## [1.0.0] - 2024-XX-XX

### Initial Release
- Setup iniziale con Access Policies (metodo deprecato)
- Configurazione base Azure: Static Web App, Web API, SQL, Key Vault
- Script di deployment base

---

## Confronto Versioni

### v1.0.0 (Access Policy) ❌
```hcl
resource "azurerm_key_vault" "main" {
  enable_rbac_authorization = false  # o omesso
  
  access_policy {
    tenant_id = data.azurerm_client_config.current.tenant_id
    object_id = data.azurerm_client_config.current.object_id
    
    secret_permissions = ["Get", "List", "Set", "Delete"]
  }
}
```

**Problemi**:
- Gestione frammentata permessi
- Audit limitato
- Non scala bene
- Deprecato da Microsoft

### v2.0.0 (RBAC) ✅
```hcl
resource "azurerm_key_vault" "main" {
  enable_rbac_authorization = true
}

resource "azurerm_role_assignment" "kv_admin" {
  scope                = azurerm_key_vault.main.id
  role_definition_name = "Key Vault Administrator"
  principal_id         = data.azurerm_client_config.current.object_id
  depends_on           = [azurerm_key_vault.main]
}
```

**Vantaggi**:
- Gestione centralizzata con Azure AD
- Audit completo in Activity Log
- Scala a subscription/resource group
- Best practice Microsoft
- Future-proof

---

## Checklist Migrazione

- [ ] Backup di tutti i secrets esistenti
- [ ] Update `main.tf` con nuova configurazione
- [ ] Rimuovi `access_policy` blocks
- [ ] Aggiungi `azurerm_role_assignment` resources
- [ ] Update `depends_on` per secret creation
- [ ] Test in ambiente DEV
- [ ] Esegui `./verify-keyvault-rbac.sh dev`
- [ ] Documenta eventuali custom permissions
- [ ] Pianifica migrazione PROD
- [ ] Notifica team delle modifiche

---

## Rollback

In caso di problemi, per tornare a Access Policies:

```hcl
resource "azurerm_key_vault" "main" {
  enable_rbac_authorization = false
  
  access_policy {
    tenant_id = data.azurerm_client_config.current.tenant_id
    object_id = data.azurerm_client_config.current.object_id
    
    secret_permissions = ["Get", "List", "Set", "Delete", "Purge", "Recover"]
  }
}
```

⚠️ **ATTENZIONE**: Il rollback richiede `terraform destroy` del Key Vault (perché non si può cambiare `enable_rbac_authorization` in-place)

---

## Support

Per problemi o domande:
1. Controlla [README.md](README.md)
2. Esegui `./verify-keyvault-rbac.sh dev`
3. Consulta [KEY_VAULT_RBAC_MIGRATION.md](KEY_VAULT_RBAC_MIGRATION.md)
4. Apri un issue con l'output dello script di verifica
