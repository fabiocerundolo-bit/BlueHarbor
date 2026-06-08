import { useApp } from '../../context/AppContext'
import StatusBadge from '../StatusBadge'
import SizeBadge from '../SizeBadge'

export default function ShipTable() {
  const { ships, shipsLoading, refreshShips, currentDay } = useApp()

  return (
    <div className="card flex flex-col min-h-0 flex-1">
      <div className="flex items-center justify-between mb-4">
        <h2 className="section-title mb-0">
          <svg className="w-4 h-4 text-navy" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
          </svg>
          Registro Navi
          {ships.length > 0 && (
            <span className="ml-1 bg-navy-50 text-navy text-xs font-semibold px-2 py-0.5 rounded-full">
              {ships.length}
            </span>
          )}
        </h2>
        <button
          onClick={refreshShips}
          disabled={shipsLoading}
          className="btn-secondary py-1.5 text-xs flex items-center gap-1.5"
        >
          <svg
            className={`w-3.5 h-3.5 ${shipsLoading ? 'animate-spin' : ''}`}
            fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}
          >
            <path strokeLinecap="round" strokeLinejoin="round" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
          </svg>
          Aggiorna
        </button>
      </div>

      {shipsLoading && ships.length === 0 ? (
        <div className="flex-1 flex items-center justify-center py-16">
          <div className="text-center">
            <svg className="w-8 h-8 animate-spin text-navy-200 mx-auto mb-3" fill="none" viewBox="0 0 24 24">
              <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"/>
              <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"/>
            </svg>
            <p className="text-sm text-slate-400">Caricamento navi...</p>
          </div>
        </div>
      ) : ships.length === 0 ? (
        <div className="flex-1 flex items-center justify-center py-16">
          <div className="text-center">
            <svg className="w-12 h-12 text-slate-200 mx-auto mb-3" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M3 15a4 4 0 004 4h9a5 5 0 10-.1-9.999 5.002 5.002 0 10-9.78 2.096A4.001 4.001 0 003 15z" />
            </svg>
            <p className="text-sm font-medium text-slate-400">Nessuna nave registrata</p>
            <p className="text-xs text-slate-300 mt-1">Usa il form per registrare la prima nave</p>
          </div>
        </div>
      ) : (
        <div className="overflow-auto -mx-5 -mb-5 mt-0">
          <table className="w-full text-left">
            <thead>
              <tr>
                <th className="table-header">#</th>
                <th className="table-header">Nome</th>
                <th className="table-header">Dim.</th>
                <th className="table-header">Arrivo</th>
                <th className="table-header">Durata</th>
                <th className="table-header">Stato</th>
                <th className="table-header">Banchina</th>
                <th className="table-header">Note</th>
              </tr>
            </thead>
            <tbody>
              {ships.map((ship) => (
                <tr key={ship.id} className="hover:bg-slate-50 transition-colors">
                  <td className="table-cell text-slate-400 font-mono text-xs">{ship.id}</td>
                  <td className="table-cell font-semibold text-navy">{ship.name}</td>
                  <td className="table-cell">
                    <SizeBadge size={ship.size} />
                  </td>
                  <td className="table-cell tabular-nums">
                    <span className={`font-medium ${currentDay !== null && ship.arrivalDay <= currentDay ? 'text-harbor-success' : 'text-slate-600'}`}>
                      Giorno {ship.arrivalDay}
                    </span>
                  </td>
                  <td className="table-cell tabular-nums">
                    {ship.durationDays} gg
                  </td>
                  <td className="table-cell">
                    <StatusBadge status={ship.status} />
                  </td>
                  <td className="table-cell text-slate-500">
                    {ship.assignedBerthId ? (
                      <span className="font-medium text-navy-400">
                        #{ship.assignedBerthId}
                        {ship.startDay && ` · g.${ship.startDay}`}
                      </span>
                    ) : (
                      <span className="text-slate-300">—</span>
                    )}
                  </td>
                  <td className="table-cell text-slate-400 max-w-xs truncate">
                    {ship.notes || <span className="text-slate-200">—</span>}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
