---
name: uiux
---

# UI/UX — Linee guida obbligatorie

## Libreria componenti

- Utilizza **solo** componenti **shadcn/ui** come libreria UI (nessun altro kit o libreria di componenti).

## Bottoni più “vivi” e riconoscibili

- Dai **più colore** ai bottoni, rendendo chiara la gerarchia visiva e la tipologia di azione.
- Applica un pattern coerente per varianti di azione:
  - **info** (azioni neutre/consultazione)
  - **success** (conferma/creazione/avvio)
  - **warning** (azioni potenzialmente rischiose/modifiche importanti)
  - **error** (azioni distruttive o irreversibili)

## Animazioni e micro-interazioni

- I bottoni devono avere micro-interazioni evidenti e coerenti:
  - **hover**: variazione di colore e/o intensità, leggera elevazione (shadow) e/o trasformazione (es. `translateY(-1px)`).
  - **active**: “press” effect (es. riduzione lieve della scala e shadow più contenuta).
  - **focus-visible**: ring ben visibile (accessibilità).
  - **disabled**: stato chiaramente distinguibile (opacità + cursore + niente hover).
- Mantieni animazioni **fluide e rapide** (transizioni brevi) e consistenti su tutta l’app.
- Centralizza lo styling con un sistema riusabile (es. `ActionButton` o `buttonVariants`) per evitare duplicazioni.
