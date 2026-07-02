import { useState } from 'react'
import { useApp } from '../../context/AppContext'

export default function CreateShipForm() {
  const { createShip } = useApp()
  const [name, setName] = useState('')
  const [notes, setNotes] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState(null)
  const [lastCreated, setLastCreated] = useState(null)

  const handleSubmit = async (e) => {
    e.preventDefault()
    if (!name.trim()) return
    setLoading(true)
    setError(null)
    setLastCreated(null)
    try {
      const ship = await createShip(name.trim(), notes.trim() || null)
      setLastCreated(ship)
      setName('')
      setNotes('')
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
            Ship Name <span className="text-red-500">*</span>
          </label>
          <input
            type="text"
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="e.g. MS Adriatic Star"
            className="input-field"
            required
          />
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
          <button type="submit" disabled={loading || !name.trim()} className="btn-primary w-full justify-center flex items-center gap-2">
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
    </div>
  )
}
