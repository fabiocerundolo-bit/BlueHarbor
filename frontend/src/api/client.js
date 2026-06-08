const BASE_URL = import.meta.env.VITE_API_URL || ''

const ROLE_USERS = {
  Operatore: 'operatore1',
  Scheduler: 'scheduler1',
}

function headers(role) {
  return {
    'Content-Type': 'application/json',
    'X-Username': ROLE_USERS[role] ?? 'operatore1',
  }
}

async function request(method, path, role, body) {
  const res = await fetch(`${BASE_URL}${path}`, {
    method,
    headers: headers(role),
    body: body !== undefined ? JSON.stringify(body) : undefined,
  })

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
export const fetchCurrentDay = (role) =>
  request('GET', '/api/system/day', role)

export const advanceDay = (role) =>
  request('POST', '/api/system/next-day', role)

// ── Ships (Operatore) ────────────────────────────────────────────────────
export const createShip = (role, data) =>
  request('POST', '/api/ships', role, data)

// NOTE: GET /api/ships is NOT yet implemented in the backend.
// The backend currently only exposes GET /api/ships/{id}.
// This endpoint needs to be added. See connection requirements.
export const fetchAllShips = (role) =>
  request('GET', '/api/ships', role)

// ── Scheduler ────────────────────────────────────────────────────────────
export const fetchPendingShips = (role) =>
  request('GET', '/api/scheduler/pending', role)

export const assignShip = (role, shipId, berthId) =>
  request('POST', '/api/scheduler/assign', role, { shipId, berthId })

// NOTE: GET /api/scheduler/berths is NOT yet implemented in the backend.
// This endpoint needs to return all berths with their active assignments.
// See connection requirements.
export const fetchBerths = (role) =>
  request('GET', '/api/scheduler/berths', role)
