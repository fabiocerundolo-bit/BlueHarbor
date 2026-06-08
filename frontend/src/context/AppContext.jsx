import { createContext, useContext, useState, useCallback, useEffect, useRef } from 'react'
import * as api from '../api/client'

const AppContext = createContext(null)

export function AppProvider({ children }) {
  const [role, setRoleState] = useState('Operatore')
  const [currentDay, setCurrentDay] = useState(null)
  const [ships, setShips] = useState([])
  const [pendingShips, setPendingShips] = useState([])
  const [berths, setBerths] = useState([])
  const [dayLoading, setDayLoading] = useState(false)
  const [shipsLoading, setShipsLoading] = useState(false)
  const [berthsLoading, setBerthsLoading] = useState(false)
  const [error, setError] = useState(null)
  const roleRef = useRef(role)
  roleRef.current = role

  const clearError = useCallback(() => setError(null), [])

  // ── Day ────────────────────────────────────────────────────────────────
  const refreshDay = useCallback(async (r) => {
    try {
      const data = await api.fetchCurrentDay(r ?? roleRef.current)
      setCurrentDay(data.currentDay)
    } catch (e) {
      setError(e.message)
    }
  }, [])

  const doAdvanceDay = useCallback(async () => {
    setDayLoading(true)
    clearError()
    try {
      const data = await api.advanceDay(roleRef.current)
      setCurrentDay(data.newCurrentDay)
    } catch (e) {
      setError(e.message)
    } finally {
      setDayLoading(false)
    }
  }, [clearError])

  // ── Ships (Operatore) ─────────────────────────────────────────────────
  const refreshShips = useCallback(async () => {
    setShipsLoading(true)
    clearError()
    try {
      const data = await api.fetchAllShips('Operatore')
      setShips(data)
    } catch (e) {
      // Graceful: endpoint may not exist yet
      setError(e.message)
    } finally {
      setShipsLoading(false)
    }
  }, [clearError])

  const doCreateShip = useCallback(async (name, notes) => {
    clearError()
    const ship = await api.createShip('Operatore', { name, notes })
    // Prepend to local list without needing a full refresh
    setShips((prev) => [ship, ...prev])
    return ship
  }, [clearError])

  // ── Scheduler ─────────────────────────────────────────────────────────
  const refreshPendingShips = useCallback(async () => {
    clearError()
    try {
      const data = await api.fetchPendingShips('Scheduler')
      setPendingShips(data)
    } catch (e) {
      setError(e.message)
    }
  }, [clearError])

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

  const doAssignShip = useCallback(async (shipId, berthId) => {
    clearError()
    const result = await api.assignShip('Scheduler', shipId, berthId)
    await Promise.all([refreshPendingShips(), refreshBerths()])
    return result
  }, [clearError, refreshPendingShips, refreshBerths])

  // ── Role switch ────────────────────────────────────────────────────────
  const setRole = useCallback((r) => {
    setRoleState(r)
    clearError()
    refreshDay(r)
    if (r === 'Operatore') refreshShips()
    if (r === 'Scheduler') {
      refreshPendingShips()
      refreshBerths()
    }
  }, [clearError, refreshDay, refreshShips, refreshPendingShips, refreshBerths])

  // ── Bootstrap ─────────────────────────────────────────────────────────
  useEffect(() => {
    refreshDay()
    refreshShips()
  }, []) // eslint-disable-line react-hooks/exhaustive-deps

  return (
    <AppContext.Provider value={{
      role,
      setRole,
      currentDay,
      ships,
      pendingShips,
      berths,
      dayLoading,
      shipsLoading,
      berthsLoading,
      error,
      clearError,
      refreshDay,
      advanceDay: doAdvanceDay,
      refreshShips,
      createShip: doCreateShip,
      refreshPendingShips,
      refreshBerths,
      assignShip: doAssignShip,
    }}>
      {children}
    </AppContext.Provider>
  )
}

export const useApp = () => {
  const ctx = useContext(AppContext)
  if (!ctx) throw new Error('useApp must be used within AppProvider')
  return ctx
}
