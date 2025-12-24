---
agent: agent
---

Perfetto. Ti propongo **mockup funzionali / wireframe testuali**, pensati esattamente per essere tradotti **1:1 in React**.
Niente Figma, niente tool: **layout chiari, componenti evidenti e differenze Creator vs Giocatore ben marcate**.

Userò queste convenzioni:

- `[ ]` = input
- `( )` = bottone
- `⚙` = icona gear (solo creatore)
- `TAB | TAB` = navigazione
- `—` = separatore visivo

---

# 1️⃣ PAGINA DI ACCESSO (Join)

### Vista GENERICA (uguale per tutti)

```
--------------------------------------------------
|            FANTAPARTITA                         |
--------------------------------------------------

Inserisci codice di accesso

[ CODICE ACCESSO ]

( ENTRA NELLA PARTITA )

--------------------------------------------------
```

Comportamento:

- Il codice identifica **match + player**
- Dopo il join → redirect automatico alla **Pagina Partita**

---

# 2️⃣ PAGINA PARTITE (dashboard)

> Qui mostri tutte le partite a cui l’utente ha accesso
> **Differenza chiave: il creatore vede la ⚙**

---

## 👑 Vista CREATORE

```
--------------------------------------------------
| Le mie partite                                  |
--------------------------------------------------

[ + Crea nuova partita ]

--------------------------------------------------
| Fantapartita Champions        STATO: Draft  ⚙ |
| Creatore: TU                                   |
| Giocatori: 6                                   |
--------------------------------------------------

--------------------------------------------------
| Fantapartita Serie A          STATO: Started ⚙|
| Creatore: TU                                   |
| Giocatori: 8                                   |
--------------------------------------------------
```

Azioni:

- Click sulla riga → entra nella partita
- Click su `⚙` → **Pagina Configurazione Partita**

---

## 👤 Vista GIOCATORE NORMALE

```
--------------------------------------------------
| Le mie partite                                  |
--------------------------------------------------

--------------------------------------------------
| Fantapartita Champions        STATO: Started   |
| Creatore: Mario Rossi                          |
| Giocatori: 6                                   |
--------------------------------------------------

--------------------------------------------------
| Fantapartita Serie A          STATO: Started   |
| Creatore: Luca Bianchi                        |
| Giocatori: 8                                   |
--------------------------------------------------
```

Nota:

- **Nessuna ⚙**
- Nessun bottone di creazione

---

# 3️⃣ PAGINA PARTITA – CLASSIFICA

> Questa è la schermata principale visibile **durante la partita**

### Tabs comuni

```
[ CLASSIFICA ] | [ BONUS / MALUS ]
```

---

## 👑 Vista CREATORE – Classifica

```
--------------------------------------------------
| Fantapartita Champions                    ⚙   |
| Stato: STARTED                                  |
--------------------------------------------------

[ CLASSIFICA ] | [ BONUS / MALUS ]

--------------------------------------------------
| # | Giocatore        | Punti                   |
--------------------------------------------------
| 1 | Marco            | 120                     |
| 2 | Luca             | 115                     |
| 3 | Anna             | 110                     |
| 4 | TU               | 105                     |
--------------------------------------------------
```

Note:

- `⚙` visibile **ma disabilitata** se la partita è STARTED
- Classifica aggiornata in realtime / polling

---

## 👤 Vista GIOCATORE – Classifica

```
--------------------------------------------------
| Fantapartita Champions                          |
| Stato: STARTED                                  |
--------------------------------------------------

[ CLASSIFICA ] | [ BONUS / MALUS ]

--------------------------------------------------
| # | Giocatore        | Punti                   |
--------------------------------------------------
| 1 | Marco            | 120                     |
| 2 | TU               | 115                     |
| 3 | Anna             | 110                     |
| 4 | Luca             | 105                     |
--------------------------------------------------
```

Differenze:

- Nessuna ⚙
- Nessuna azione possibile qui

---

# 4️⃣ PAGINA PARTITA – BONUS / MALUS

> Qui avviene l’azione “la prima che…”

---

## 👑 Vista CREATORE – Bonus/Malus

```
--------------------------------------------------
| Fantapartita Champions                          |
--------------------------------------------------

[ CLASSIFICA ] | [ BONUS / MALUS ]

--------------------------------------------------
| BONUS                                           |
--------------------------------------------------

[ +10 ] Segna primo gol        ( DISPONIBILE )
[ +5  ] Assist decisivo        ( PRESO da Luca )

--------------------------------------------------
| MALUS                                           |
--------------------------------------------------

[ -5  ] Ammonizione            ( DISPONIBILE )
[ -10 ] Espulsione             ( PRESO da Anna )
--------------------------------------------------
```

Comportamento:

- Il creatore **può cliccare** come gli altri
- Gli item “PRESO” sono disabilitati

---

## 👤 Vista GIOCATORE – Bonus/Malus

```
--------------------------------------------------
| Fantapartita Champions                          |
--------------------------------------------------

[ CLASSIFICA ] | [ BONUS / MALUS ]

--------------------------------------------------
| BONUS                                           |
--------------------------------------------------

( +10 ) Segna primo gol        [ PRENDI ]
( +5  ) Assist decisivo        [ PRESO ]

--------------------------------------------------
| MALUS                                           |
--------------------------------------------------

( -5  ) Ammonizione            [ PRENDI ]
( -10 ) Espulsione             [ PRESO ]
--------------------------------------------------
```

Click su `[ PRENDI ]`:

- Successo → aggiorna classifica + disabilita item
- Fallimento (qualcuno più veloce) → toast:

  > “Bonus già preso da un altro giocatore”

---

# 5️⃣ PAGINA CONFIGURAZIONE PARTITA (⚙)

> **Accessibile solo al creatore**

---

## 👑 Vista CREATORE – Configurazione (Draft)

```
--------------------------------------------------
| Configurazione Fantapartita                     |
--------------------------------------------------

Nome partita:
[ Fantapartita Champions ]

Punti iniziali:
[ 100 ]

--------------------------------------------------
| Giocatori                                       |
--------------------------------------------------
| Marco  | Codice: A1F3X                          |
| Luca   | Codice: B9K2P                          |
| Anna   | Codice: Q7M8L                          |
| TU     | Creatore                               |

( + Aggiungi giocatore )

--------------------------------------------------
| Bonus / Malus                                   |
--------------------------------------------------

[ +10 ] Segna primo gol        ( RIMUOVI )
[ -5  ] Ammonizione            ( RIMUOVI )

( + Aggiungi bonus/malus )

--------------------------------------------------

( AVVIA PARTITA )
```

---

## 👑 Vista CREATORE – Configurazione (Started)

```
--------------------------------------------------
| Configurazione Fantapartita                     |
--------------------------------------------------

⚠ La partita è iniziata.
Le impostazioni non sono più modificabili.
```

Tutti i campi **read-only**, nessun bottone attivo.

---

## 👤 Vista GIOCATORE NORMALE – Configurazione

🚫 **NON ESISTE**
Il giocatore non può mai accedervi.

---

# Riepilogo rapido (per sviluppo React)

| Pagina         | Creatore      | Giocatore  |
| -------------- | ------------- | ---------- |
| Accesso        | ✔             | ✔          |
| Partite        | ⚙ + crea      | solo lista |
| Classifica     | ⚙ (read-only) | ✔          |
| Bonus/Malus    | ✔             | ✔          |
| Configurazione | ✔             | ✘          |

---

- trasforma **queste pag di queste pagine in component tree React**
- definire **props e state** per ogni vista
- prepararti **routing Vite + protezioni ruolo (creator/player)**

Dimmi da quale pagina vuoi partire per il codice.
