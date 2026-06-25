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

  // Ruolo attivo: viene letto da localStorage per sopravvivere ai refresh di pagina
  const [role, setRoleState] = useState(
    () => localStorage.getItem('bh_role') ?? 'Operatore'
  )
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

  // Ricarica la lista completa di tutte le navi.
  // Accetta un ruolo opzionale: se non specificato usa il ruolo corrente (roleRef).
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

  // Esegue l'assegnazione di una nave a una banchina.
  // Aggiorna le liste dello scheduler (pending + banchine) e sincronizza anche la lista
  // ships dell'operatore direttamente in state, senza fare una chiamata API con role sbagliato.
  // Esegue l'assegnazione e ricarica tutte e tre le liste dal DB:
  // pending ships, banchine e registro navi (usando il ruolo Scheduler, ora autorizzato su GET /api/ships).
  const doAssignShip = useCallback(async (shipId, berthId) => {
    const result = await api.assignShip('Scheduler', shipId, berthId)
    await Promise.all([refreshPendingShips(), refreshBerths(), refreshShips('Scheduler')])
    return result
  }, [refreshPendingShips, refreshBerths, refreshShips])

  // Avanza il giorno corrente di 1 unità ed aggiorna lo stato locale
  // Definito dopo le funzioni di aggiornamento in quanto le utilizza come dipendenze
  const doAdvanceDay = useCallback(async () => {
    setDayLoading(true)
    clearError()
    try {
      const data = await api.advanceDay(roleRef.current)
      setCurrentDay(data.newCurrentDay)

      // Attende 700ms per dare tempo al job Hangfire in background di completare
      // l'aggiornamento degli stati delle navi (es. Assigned -> Departed)
      // prima di ricaricare i dati dal backend
      await new Promise(resolve => setTimeout(resolve, 700))

      // Aggiorna i dati relativi al ruolo corrente per mostrare lo stato aggiornato
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

  // Gestisce lo switch di ruolo tra Operatore e Scheduler, forzando il caricamento dei dati corretti.
  // Persiste il ruolo scelto in localStorage per sopravvivere ai refresh di pagina.
  const setRole = useCallback((r) => {
    localStorage.setItem('bh_role', r)
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
  // Carica i dati appropriati in base al ruolo già salvato in localStorage
  useEffect(() => {
    refreshDay()
    if (roleRef.current === 'Operatore') {
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