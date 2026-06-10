import { createContext, useContext, useState, useCallback, useEffect, useRef } from 'react'
import * as api from '../api/client'

// Creazione del contesto globale per l'applicazione BlueHarbor
const AppContext = createContext(null)

/**
 * Provider globale che avvolge l'intera applicazione React.
 * Gestisce e sincronizza lo stato condiviso tra le pagine di "Operatore" e di "Scheduler".
 */
export function AppProvider({ children }) {
  // ── STATO GLOBALE ──────────────────────────────────────────────────────
  const [role, setRoleState] = useState('Operatore') // Ruolo attivo dell'utente ('Operatore' o 'Scheduler')
  const [currentDay, setCurrentDay] = useState(null)  // Giorno virtuale corrente del porto
  const [ships, setShips] = useState([])             // Elenco di tutte le navi registrate (usato dall'operatore)
  const [pendingShips, setPendingShips] = useState([]) // Navi in attesa di assegnazione (usato dallo scheduler)
  const [berths, setBerths] = useState([])           // Banchine ed occupazioni pianificate (usato dallo scheduler)
  
  // Stati di caricamento per l'interfaccia utente (Loading States)
  const [dayLoading, setDayLoading] = useState(false)
  const [shipsLoading, setShipsLoading] = useState(false)
  const [berthsLoading, setBerthsLoading] = useState(false)
  
  const [error, setError] = useState(null)            // Ultimo messaggio d'errore catturato dall'API

  // useRef per memorizzare il ruolo ed evitare dipendenze superflue nelle callback di useCallback
  const roleRef = useRef(role)
  roleRef.current = role

  // Pulisce l'errore corrente visualizzato nel banner
  const clearError = useCallback(() => setError(null), [])

  // ── GESTIONE GIORNO DI SISTEMA ──────────────────────────────────────────
  
  // Recupera il giorno virtuale corrente dal backend
  const refreshDay = useCallback(async (r) => {
    try {
      const data = await api.fetchCurrentDay(r ?? roleRef.current)
      setCurrentDay(data.currentDay)
    } catch (e) {
      setError(e.message)
    }
  }, [])

  // ── GESTIONE NAVI (Ruolo: Operatore) ────────────────────────────────────
  
  // Ricarica la lista completa di tutte le navi per l'Operatore
  const refreshShips = useCallback(async () => {
    setShipsLoading(true)
    clearError()
    try {
      const data = await api.fetchAllShips('Operatore')
      setShips(data)
    } catch (e) {
      setError(e.message)
    } finally {
      setShipsLoading(false)
    }
  }, [clearError])

  // Registra una nuova nave inviando i dati al backend e inserendola in cima alla lista locale
  const doCreateShip = useCallback(async (name, notes) => {
    clearError()
    const ship = await api.createShip('Operatore', { name, notes })
    setShips((prev) => [ship, ...prev])
    return ship
  }, [clearError])

  // ── GESTIONE PIANIFICAZIONE (Ruolo: Scheduler) ──────────────────────────
  
  // Ricarica la lista delle sole navi in attesa di attracco
  const refreshPendingShips = useCallback(async () => {
    clearError()
    try {
      const data = await api.fetchPendingShips('Scheduler')
      setPendingShips(data)
    } catch (e) {
      setError(e.message)
    }
  }, [clearError])

  // Ricarica la griglia delle banchine con le rispettive prenotazioni
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

  // Esegue l'assegnazione di una nave a una banchina e riaggiorna le liste pendenti e banchine
  const doAssignShip = useCallback(async (shipId, berthId) => {
    clearError()
    const result = await api.assignShip('Scheduler', shipId, berthId)
    await Promise.all([refreshPendingShips(), refreshBerths()])
    return result
  }, [clearError, refreshPendingShips, refreshBerths])

  // Avanza il giorno corrente di 1 unità ed aggiorna lo stato locale
  // Definito dopo le funzioni di aggiornamento in quanto le utilizza come dipendenze
  const doAdvanceDay = useCallback(async () => {
    setDayLoading(true)
    clearError()
    try {
      const data = await api.advanceDay(roleRef.current)
      setCurrentDay(data.newCurrentDay)

      // Quando il giorno avanza, aggiorna i dati relativi al ruolo corrente
      // per mostrare subito lo stato aggiornato (es. navi passate a "Departed")
      if (roleRef.current === 'Operatore') {
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

  // ── CAMBIO DI RUOLO (Sincronizzazione Dati) ─────────────────────────────
  
  // Gestisce lo switch di ruolo tra Operatore e Scheduler, forzando il caricamento dei dati corretti
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

  // ── BOOTSTRAP INIZIALE ──────────────────────────────────────────────────
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

/**
 * Hook personalizzato per consumare il contesto dell'applicazione in modo sicuro.
 */
export const useApp = () => {
  const ctx = useContext(AppContext)
  if (!ctx) throw new Error('useApp must be used within AppProvider')
  return ctx
}

