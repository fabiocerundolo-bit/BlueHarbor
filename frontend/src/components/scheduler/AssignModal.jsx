import { useState } from 'react'
import { useApp } from '../../context/AppContext'
import SizeBadge from '../SizeBadge'

export default function AssignModal({ ship, berth, onClose, onSuccess }) {
  const { assignShip } = useApp()
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState(null)

  if (!ship || !berth) return null

  const handleConfirm = async () => {
    setLoading(true)
    setError(null)
    try {
      const result = await assignShip(ship.id, berth.id)
      onSuccess(result)
      onClose()
    } catch (err) {
      setError(err.message)
      setLoading(false)
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      {/* Backdrop */}
      <div
        className="absolute inset-0 bg-navy/60 backdrop-blur-sm"
        onClick={!loading ? onClose : undefined}
      />

      {/* Dialog */}
      <div className="relative bg-white rounded-2xl shadow-xl w-full max-w-md mx-4 overflow-hidden">
        {/* Header */}
        <div className="bg-navy px-6 py-4 flex items-center justify-between">
          <h3 className="text-white font-semibold text-base">Conferma Assegnazione</h3>
          <button
            onClick={onClose}
            disabled={loading}
            className="text-white/60 hover:text-white transition-colors"
          >
            <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        {/* Body */}
        <div className="px-6 py-5 space-y-4">
          <p className="text-sm text-slate-600">
            Stai per assegnare la nave alla banchina selezionata. Il sistema calcolerà
            automaticamente il primo slot temporale disponibile.
          </p>

          {/* Ship info */}
          <div className="bg-slate-50 rounded-xl p-4 space-y-2">
            <p className="text-xs font-semibold text-slate-400 uppercase tracking-wide">Nave</p>
            <div className="flex items-center gap-3">
              <SizeBadge size={ship.size} />
              <div>
                <p className="font-semibold text-navy">{ship.name}</p>
                <p className="text-xs text-slate-500">
                  Arrivo: Giorno {ship.arrivalDay} · Durata: {ship.durationDays} giorni
                </p>
              </div>
            </div>
          </div>

          {/* Arrow */}
          <div className="flex justify-center">
            <svg className="w-5 h-5 text-slate-300" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M19 9l-7 7-7-7" />
            </svg>
          </div>

          {/* Berth info */}
          <div className="bg-navy-50 rounded-xl p-4 space-y-2">
            <p className="text-xs font-semibold text-slate-400 uppercase tracking-wide">Banchina</p>
            <div className="flex items-center gap-3">
              <SizeBadge size={berth.size} />
              <div>
                <p className="font-semibold text-navy">{berth.name}</p>
                <p className="text-xs text-slate-500">Capacità: navi {berth.size}</p>
              </div>
            </div>
          </div>

          {error && (
            <div className="p-3 bg-red-50 border border-red-200 rounded-lg text-xs text-red-600">
              {error}
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="px-6 pb-5 flex gap-3 justify-end">
          <button onClick={onClose} disabled={loading} className="btn-secondary">
            Annulla
          </button>
          <button onClick={handleConfirm} disabled={loading} className="btn-primary flex items-center gap-2">
            {loading ? (
              <>
                <svg className="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"/>
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"/>
                </svg>
                Assegnazione...
              </>
            ) : 'Conferma Assegnazione'}
          </button>
        </div>
      </div>
    </div>
  )
}
