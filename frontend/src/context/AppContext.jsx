import { createContext, useContext, useState, useCallback, useEffect, useRef } from 'react'
import * as api from '../api/client'

// Global context creation for the BlueHarbor application
const AppContext = createContext(null)

/**
 * Global provider that wraps the entire React application.
 * Manages and synchronizes shared state between the "Operator" and "Scheduler" pages.
 */
export function AppProvider({ children }) {
  // ── GLOBAL STATE ──────────────────────────────────────────────────────

  // Active role: read from localStorage to survive page refreshes
  const [role, setRoleState] = useState(
    () => localStorage.getItem('bh_role') ?? 'Operator'
  )
  const [currentDay, setCurrentDay] = useState(null)  // Harbor current virtual day
  const [ships, setShips] = useState([])              // List of all registered ships (used by the operator)
  const [shipList, setShipList] = useState([])        // Ship templates available for creation
  const [pendingShips, setPendingShips] = useState([]) // Ships waiting for assignment (used by the scheduler)
  const [berths, setBerths] = useState([])            // Berths and planned occupancies (used by the scheduler)

  // Loading states for the user interface
  const [dayLoading, setDayLoading] = useState(false)
  const [shipsLoading, setShipsLoading] = useState(false)
  const [shipListLoading, setShipListLoading] = useState(false)
  const [berthsLoading, setBerthsLoading] = useState(false)

  const [error, setError] = useState(null)            // Last error message captured from the API

  // useRef to store the role and avoid unnecessary dependencies in useCallback callbacks
  const roleRef = useRef(role)
  roleRef.current = role

  // Clears the current error shown in the banner
  const clearError = useCallback(() => setError(null), [])

  // ── SYSTEM DAY MANAGEMENT ──────────────────────────────────────────────

  // Fetches the current virtual day from the backend
  const refreshDay = useCallback(async (r) => {
    try {
      const data = await api.fetchCurrentDay(r ?? roleRef.current)
      setCurrentDay(data.currentDay)
    } catch (e) {
      setError(e.message)
    }
  }, [])

  // ── SHIPS MANAGEMENT (Role: Operator) ────────────────────────────────

  // Reloads the full list of all ships.
  // Accepts an optional role: if not specified it uses the current role (roleRef).
  const refreshShips = useCallback(async (role) => {
    setShipsLoading(true)
    clearError()
    try {
      const data = await api.fetchAllShips(role ?? roleRef.current)
      setShips(data)
    } catch (e) {
      setError(e.message)
    } finally {
      setShipsLoading(false)
    }
  }, [clearError])

  // Reloads the list of ship templates used by the operator form.
  const refreshShipList = useCallback(async (role) => {
    setShipListLoading(true)
    clearError()
    try {
      const data = await api.fetchShipList(role ?? roleRef.current)
      setShipList(data)
    } catch (e) {
      setError(e.message)
    } finally {
      setShipListLoading(false)
    }
  }, [clearError])

  // Registers a new ship by sending data to the backend and prepending it to the local list
  const doCreateShip = useCallback(async (idListaNavi, notes) => {
    clearError()
    const ship = await api.createShip('Operator', { idListaNavi, notes })
    setShips((prev) => [ship, ...prev])
    return ship
  }, [clearError])

  // ── SCHEDULING MANAGEMENT (Role: Scheduler) ──────────────────────────

  // Reloads the list of ships pending berth assignment
  const refreshPendingShips = useCallback(async () => {
    clearError()
    try {
      const data = await api.fetchPendingShips('Scheduler')
      setPendingShips(data)
    } catch (e) {
      setError(e.message)
    }
  }, [clearError])

  // Reloads the berth grid with their respective assignments
  const refreshBerths = useCallback(async () => {
    setBerthsLoading(true)
    clearError()
    try {
      const data = await api.fetchBerths('Scheduler')
      setBerths(data)
    } catch (e) {
      setError(e.message)
    } finally {
      setBerthsLoading(false)
    }
  }, [clearError])

  // Performs the assignment of a ship to a berth.
  // Updates all three scheduler lists from the DB:
  // pending ships, berths, and ship registry (using the Scheduler role, now authorized on GET /api/ships).
  const doAssignShip = useCallback(async (shipId, berthId) => {
    const result = await api.assignShip('Scheduler', shipId, berthId)
    await Promise.all([refreshPendingShips(), refreshBerths(), refreshShips('Scheduler')])
    return result
  }, [refreshPendingShips, refreshBerths, refreshShips])

  // Advances the current day by 1 unit and updates the local state.
  // Defined after the update functions as it depends on them.
  const doAdvanceDay = useCallback(async () => {
    setDayLoading(true)
    clearError()
    try {
      const data = await api.advanceDay(roleRef.current)
      setCurrentDay(data.newCurrentDay)

      // Wait 700ms to give the Hangfire background job time to complete
      // the ship status update (e.g. Assigned -> Departed)
      // before reloading data from the backend
      await new Promise(resolve => setTimeout(resolve, 700))

      // Update data for the current role to show the updated state
      if (roleRef.current === 'Operator') {
        await refreshShips()
      } else if (roleRef.current === 'Scheduler') {
        await Promise.all([refreshPendingShips(), refreshBerths()])
      }
    } catch (e) {
      setError(e.message)
    } finally {
      setDayLoading(false)
    }
  }, [clearError, refreshShips, refreshPendingShips, refreshBerths])

  // ── ROLE SWITCH (Data Synchronization) ─────────────────────────────

  // Handles switching roles between Operator and Scheduler and forces the correct data to load.
  // Persists the chosen role in localStorage to survive page refreshes.
  const setRole = useCallback((r) => {
    localStorage.setItem('bh_role', r)
    setRoleState(r)
    clearError()
    refreshDay(r)
    if (r === 'Operator') refreshShips()
    if (r === 'Scheduler') {
      refreshPendingShips()
      refreshBerths()
    }
  }, [clearError, refreshDay, refreshShips, refreshPendingShips, refreshBerths])

  // ── INITIAL BOOTSTRAP ──────────────────────────────────────────────
  // Loads appropriate data based on the role already saved in localStorage
  useEffect(() => {
    refreshDay()
    refreshShipList('Operator')
    if (roleRef.current === 'Operator') {
      refreshShips()
    } else {
      refreshPendingShips()
      refreshBerths()
    }
  }, []) // eslint-disable-line react-hooks/exhaustive-deps

  return (
    <AppContext.Provider value={{
      role,
      setRole,
      currentDay,
      ships,
      shipList,
      pendingShips,
      berths,
      dayLoading,
      shipsLoading,
      shipListLoading,
      berthsLoading,
      error,
      clearError,
      refreshDay,
      advanceDay: doAdvanceDay,
      refreshShips,
      refreshShipList,
      createShip: doCreateShip,
      refreshPendingShips,
      refreshBerths,
      assignShip: doAssignShip,
    }}>
      {children}
    </AppContext.Provider>
  )
}

/**
 * Custom hook to safely consume the application context.
 */
export const useApp = () => {
  const ctx = useContext(AppContext)
  if (!ctx) throw new Error('useApp must be used within AppProvider')
  return ctx
}