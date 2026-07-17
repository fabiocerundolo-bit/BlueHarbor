import { useState } from 'react'
import { useApp } from '../../context/AppContext'

export default function CreateShipForm() {
  const { createShip, shipList, shipListLoading, refreshShipList } = useApp()
  const [selectedShipId, setSelectedShipId] = useState('')
  const [shipName, setShipName] = useState('')
  const [notes, setNotes] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState(null)
  const [lastCreated, setLastCreated] = useState(null)

  const selectedShip = shipList.find((item) => String(item.id) === String(selectedShipId)) ?? null

  const handleSubmit = async (e) => {
    e.preventDefault()
    if (!selectedShipId && !shipName.trim()) return
    setLoading(true)
    setError(null)
    setLastCreated(null)
    try {
      // Se l'utente ha digitato solo il nome senza selezionare dalla lista,
      // scegliamo un IdListaNavi casuale dalla lista disponibile (necessario per la FK del backend).
      // Il CustomName sovrascriverà il nome della nave nel DB.
      let idListaNavi = Number(selectedShipId)
      if (!idListaNavi) {
        const randomItem = shipList[Math.floor(Math.random() * shipList.length)]
        idListaNavi = randomItem ? randomItem.id : 1
      }
      const ship = await createShip(idListaNavi, notes.trim() || null, shipName.trim() || null)
      setLastCreated(ship)
      setSelectedShipId('')
      setNotes('')
      setShipName('')
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="card">
      <h2 className="section-title">
        <svg className="w-4 h-4 text-navy" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
          <path strokeLinecap="round" strokeLinejoin="round" d="M12 4v16m8-8H4" />
        </svg>
        Register New Ship
      </h2>

      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-xs font-semibold text-slate-600 mb-1.5 uppercase tracking-wide">
            Ship Name (manual)
          </label>
          <input
            type="text"
            value={shipName}
            onChange={(e) => setShipName(e.target.value)}
            placeholder="Type the ship name..."
            className="input-field"
            disabled={!!selectedShipId}
          />
        </div>

        <div>
          <label className="block text-xs font-semibold text-slate-600 mb-1.5 uppercase tracking-wide">
            — or select from list —
          </label>
          <select
            value={selectedShipId}
            onChange={(e) => setSelectedShipId(e.target.value)}
            className="input-field"
            disabled={shipListLoading || shipList.length === 0 || !!shipName.trim()}
          >
            <option value="">
              {shipListLoading ? 'Loading ship templates...' : 'Select a ship'}
            </option>
            {shipList.map((item) => (
              <option key={item.id} value={item.id}>
                {item.name} · {item.size}
              </option>
            ))}
          </select>
        </div>

        <div>
          <label className="block text-xs font-semibold text-slate-600 mb-1.5 uppercase tracking-wide">
            Notes
          </label>
          <textarea
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            placeholder="Additional information about the ship..."
            rows={3}
            className="input-field resize-none"
          />
        </div>

        <div className="pt-1">
          <p className="text-xs text-slate-400 mb-3 leading-relaxed">
            The system will automatically assign size, arrival day, and duration.
          </p>
          <button type="submit" disabled={loading || (!selectedShipId && !shipName.trim())} className="btn-primary w-full justify-center flex items-center gap-2">
            {loading ? (
              <>
                <svg className="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"/>
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"/>
                </svg>
                Registering...
              </>
            ) : 'Register Ship'}
          </button>
        </div>
      </form>

      {/* Success notification */}
      {lastCreated && (
        <div className="mt-4 p-3 bg-emerald-50 border border-emerald-200 rounded-lg">
          <p className="text-xs font-semibold text-emerald-700 mb-1.5">Ship registered successfully</p>
          <div className="grid grid-cols-2 gap-x-4 gap-y-1 text-xs text-emerald-600">
            <span>Size: <strong>{lastCreated.size}</strong></span>
            <span>Arrival: <strong>Day {lastCreated.arrivalDay}</strong></span>
            <span>Duration: <strong>{lastCreated.durationDays} days</strong></span>
            <span>Status: <strong>{lastCreated.status}</strong></span>
          </div>
        </div>
      )}

      {/* Error notification */}
      {error && (
        <div className="mt-4 p-3 bg-red-50 border border-red-200 rounded-lg">
          <p className="text-xs text-red-600">{error}</p>
        </div>
      )}

      {!shipListLoading && shipList.length === 0 && (
        <div className="mt-4 p-3 bg-amber-50 border border-amber-200 rounded-lg">
          <p className="text-xs text-amber-700">
            No ship templates available. Try refreshing the catalog.
          </p>
          <button
            type="button"
            onClick={() => refreshShipList('Operator')}
            className="mt-2 text-xs font-semibold text-amber-800 hover:text-amber-900"
          >
            Refresh catalog
          </button>
        </div>
      )}
    </div>
  )
}
