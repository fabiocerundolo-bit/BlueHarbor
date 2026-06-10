// URL base ricavato dalle variabili di ambiente di Vite, oppure stringa vuota se in modalità proxy locale
const BASE_URL = import.meta.env.VITE_API_URL || ''

// Mappa per associare i ruoli dell'applicazione agli utenti del database mock del backend
const ROLE_USERS = {
  Operatore: 'operatore1',
  Scheduler: 'scheduler1',
}

/**
 * Restituisce le intestazioni HTTP necessarie per la richiesta, includendo l'header "X-Username"
 * per autenticare le richieste sulle policy di ruolo configurate nel backend.
 * 
 * @param {string} role Il ruolo dell'utente corrente ('Operatore' | 'Scheduler')
 */
function headers(role) {
  return {
    'Content-Type': 'application/json',
    'X-Username': ROLE_USERS[role] ?? 'operatore1',
  }
}

/**
 * Funzione di utilità generale per effettuare richieste HTTP asincrone ed elaborare gli errori.
 * 
 * @param {string} method Metodo HTTP (GET, POST, ecc.)
 * @param {string} path Percorso della risorsa API (es. '/api/ships')
 * @param {string} role Ruolo dell'utente per impostare l'header di mock authentication
 * @param {object} [body] Dati JSON da inviare nel corpo della richiesta (opzionale)
 */
async function request(method, path, role, body) {
  const res = await fetch(`${BASE_URL}${path}`, {
    method,
    headers: headers(role),
    body: body !== undefined ? JSON.stringify(body) : undefined,
  })

  // Gestione degli errori HTTP
  if (!res.ok) {
    let msg = `HTTP ${res.status}`
    try {
      const text = await res.text()
      if (text) msg = text
    } catch {}
    throw new Error(msg)
  }

  const text = await res.text()
  return text ? JSON.parse(text) : null
}

// ── System ──────────────────────────────────────────────────────────────

// Recupera il giorno virtuale corrente dal sistema
export const fetchCurrentDay = (role) =>
  request('GET', '/api/system/day', role)

// Avanza il giorno virtuale del porto di 1
export const advanceDay = (role) =>
  request('POST', '/api/system/next-day', role)

// ── Ships (Operatore) ────────────────────────────────────────────────────

// Registra una nuova nave nel sistema
export const createShip = (role, data) =>
  request('POST', '/api/ships', role, data)

// Recupera l'elenco completo di tutte le navi registrate
export const fetchAllShips = (role) =>
  request('GET', '/api/ships', role)

// ── Scheduler ────────────────────────────────────────────────────────────

// Recupera l'elenco delle navi in stato "Pending"
export const fetchPendingShips = (role) =>
  request('GET', '/api/scheduler/pending', role)

// Assegna una nave specifica ad una determinata banchina calcolandone lo slot temporale
export const assignShip = (role, shipId, berthId) =>
  request('POST', '/api/scheduler/assign', role, { shipId, berthId })

// Recupera la lista di tutte le banchine comprensive dei relativi periodi di occupazione assegnati
export const fetchBerths = (role) =>
  request('GET', '/api/scheduler/berths', role)

