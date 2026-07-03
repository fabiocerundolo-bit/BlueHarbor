import { useState, useEffect } from 'react'
import { useApp } from '../context/AppContext'
import PendingShips from '../components/scheduler/PendingShips'
import BerthGrid from '../components/scheduler/BerthGrid'
import AssignModal from '../components/scheduler/AssignModal'
import HarborView from '../components/harbor/HarborView'

export default function SchedulerPage() {
  const { refreshPendingShips, refreshBerths } = useApp()
  const [selectedShip, setSelectedShip] = useState(null)
  const [modalBerth, setModalBerth] = useState(null)
  const [lastAssignment, setLastAssignment] = useState(null)
  const [view, setView] = useState('timeline')

  useEffect(() => {
    refreshPendingShips()
    refreshBerths()
  }, []) // eslint-disable-line react-hooks/exhaustive-deps

  const handleBerthClick = (berth) => {
    if (!selectedShip) return
    setModalBerth(berth)
  }

  const handleModalClose = () => {
    setModalBerth(null)
  }

  const handleAssignSuccess = (result) => {
    setLastAssignment(result)
    setSelectedShip(null)
    setModalBerth(null)
    // Data is already refreshed (with await) by doAssignShip in AppContext.
    // We do not call refreshPendingShips()/refreshBerths() here to avoid
    // race conditions: non-awaited calls could overwrite fresh data with stale data.
  }

  return (
    <div className="flex flex-col gap-5">

      {/* View toggle */}
      <div className="flex items-center gap-2">
        <div className="flex items-center gap-1 bg-slate-100 rounded-lg p-1">
          <button
            onClick={() => setView('timeline')}
            className={`px-4 py-1.5 rounded-md text-sm font-medium transition-all duration-150 flex items-center gap-2 ${
              view === 'timeline'
                ? 'bg-white text-navy shadow-sm'
                : 'text-slate-500 hover:text-slate-700'
            }`}
          >
            <svg className="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M9 17V7m0 10a2 2 0 01-2 2H5a2 2 0 01-2-2V7a2 2 0 012-2h2a2 2 0 012 2m0 10a2 2 0 002 2h2a2 2 0 002-2M9 7a2 2 0 012-2h2a2 2 0 012 2m0 10V7m0 10a2 2 0 002 2h2a2 2 0 002-2V7a2 2 0 00-2-2h-2a2 2 0 00-2 2" />
            </svg>
            Timeline
          </button>
          <button
            onClick={() => setView('harbor')}
            className={`px-4 py-1.5 rounded-md text-sm font-medium transition-all duration-150 flex items-center gap-2 ${
              view === 'harbor'
                ? 'bg-white text-navy shadow-sm'
                : 'text-slate-500 hover:text-slate-700'
            }`}
          >
            <svg className="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M3 21h18M3 10h18M3 7l9-4 9 4M4 10h1v11H4V10zm15 0h1v11h-1V10zm-5 0h1v11h-1V10zm-5 0h1v11h-1V10z" />
            </svg>
            Harbor View
          </button>
        </div>
      </div>

      {/* Success toast */}
      {lastAssignment && (
        <div className="flex items-start gap-3 p-4 bg-emerald-50 border border-emerald-200 rounded-xl">
          <svg className="w-5 h-5 text-emerald-500 flex-shrink-0 mt-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <div className="flex-1">
            <p className="text-sm font-semibold text-emerald-800">Ship assigned successfully</p>
            <p className="text-xs text-emerald-600 mt-0.5">
              Day {lastAssignment.startDay} → Day {lastAssignment.endDay}
              &nbsp;· Berth #{lastAssignment.berthId}
            </p>
          </div>
          <button onClick={() => setLastAssignment(null)} className="text-emerald-400 hover:text-emerald-600">
            <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
      )}

      {/* Pending ships (hidden in harbor view) */}
      {view !== 'harbor' && <PendingShips
        selectedShip={selectedShip}
        onSelect={(ship) => {
          setSelectedShip(ship)
          setLastAssignment(null)
        }}
      />}

      {/* Main view: Timeline or Harbor */}
      {view === 'timeline' ? (
        <BerthGrid
          selectedShip={selectedShip}
          onBerthClick={handleBerthClick}
        />
      ) : (
        <HarborView />
      )}

      {/* Assignment modal */}
      <AssignModal
        ship={selectedShip}
        berth={modalBerth}
        onClose={handleModalClose}
        onSuccess={handleAssignSuccess}
      />
    </div>
  )
}
