// Base URL taken from Vite environment variables, or empty string when using local proxy
const BASE_URL = import.meta.env.VITE_API_URL || ''

// Map to associate application roles with mock backend database users
const ROLE_USERS = {
  Operator: 'operator1',
  Scheduler: 'scheduler1',
}

/**
 * Returns the HTTP headers required for the request, including the "X-Username" header
 * to authenticate requests against the role policies configured in the backend.
 * 
 * @param {string} role The current user's role ('Operator' | 'Scheduler')
 */
function headers(role) {
  return {
    'Content-Type': 'application/json',
    'X-Username': ROLE_USERS[role] ?? 'operator1',
  }
}

/**
 * General utility function for making asynchronous HTTP requests and handling errors.
 * 
 * @param {string} method HTTP method (GET, POST, etc.)
 * @param {string} path API resource path (e.g. '/api/ships')
 * @param {string} role User role used to set the mock authentication header
 * @param {object} [body] JSON data to send in the request body (optional)
 */
async function request(method, path, role, body) {
  const res = await fetch(`${BASE_URL}${path}`, {
    method,
    headers: headers(role),
    body: body !== undefined ? JSON.stringify(body) : undefined,
  })

  // HTTP error handling
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

// Retrieves the current virtual day from the system
export const fetchCurrentDay = (role) =>
  request('GET', '/api/system/day', role)

// Advances the harbor virtual day by 1
export const advanceDay = (role) =>
  request('POST', '/api/system/next-day', role)

// ── Ships (Operator) ────────────────────────────────────────────────────

// Registers a new ship in the system
export const createShip = (role, data) =>
  request('POST', '/api/ships', role, data)

// Retrieves the complete list of all registered ships
export const fetchAllShips = (role) =>
  request('GET', '/api/ships', role)

// ── Scheduler ────────────────────────────────────────────────────────────

// Retrieves the list of ships in "Pending" status
export const fetchPendingShips = (role) =>
  request('GET', '/api/scheduler/pending', role)

// Assigns a specific ship to a specific berth, calculating the time slot
export const assignShip = (role, shipId, berthId) =>
  request('POST', '/api/scheduler/assign', role, { shipId, berthId })

// Retrieves the list of all berths including their assigned occupancy periods
export const fetchBerths = (role) =>
  request('GET', '/api/scheduler/berths', role)
