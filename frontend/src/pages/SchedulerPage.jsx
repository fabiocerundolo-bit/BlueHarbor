import { useState, useEffect } from 'react'
import { useApp } from '../context/AppContext'
import PendingShips from '../components/scheduler/PendingShips'
import BerthGrid from '../components/scheduler/BerthGrid'
import AssignModal from '../components/scheduler/AssignModal'

export default function SchedulerPage() {
  const { refreshPendingShips, refreshBerths } = useApp()
  const [selectedShip, setSelectedShip] = useState(null)
  const [modalBerth, setModalBerth] = useState(null)
  const [lastAssignment, setLastAssignment] = useState(null)

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
  }

  return (
    <div className="flex flex-col gap-5">

      {/* Success toast */}
      {lastAssignment && (
        <div className="flex items-start gap-3 p-4 bg-emerald-50 border border-emerald-200 rounded-xl">
          <svg className="w-5 h-5 text-emerald-500 flex-shrink-0 mt-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <div className="flex-1">
            <p className="text-sm font-semibold text-emerald-800">Nave assegnata con successo</p>
            <p className="text-xs text-emerald-600 mt-0.5">
              Giorno {lastAssignment.startDay} → Giorno {lastAssignment.endDay}
              &nbsp;· Banchina #{lastAssignment.berthId}
            </p>
          </div>
          <button onClick={() => setLastAssignment(null)} className="text-emerald-400 hover:text-emerald-600">
            <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
      )}

      {/* Pending ships */}
      <PendingShips
        selectedShip={selectedShip}
        onSelect={(ship) => {
          setSelectedShip(ship)
          setLastAssignment(null)
        }}
      />

      {/* Berth grid */}
      <BerthGrid
        selectedShip={selectedShip}
        onBerthClick={handleBerthClick}
      />

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
