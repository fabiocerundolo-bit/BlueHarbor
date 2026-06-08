import { useApp } from '../../context/AppContext'
import SizeBadge from '../SizeBadge'

export default function PendingShips({ selectedShip, onSelect }) {
  const { pendingShips, refreshPendingShips } = useApp()

  return (
    <div className="card flex flex-col">
      <div className="flex items-center justify-between mb-4">
        <h2 className="section-title mb-0">
          <svg className="w-4 h-4 text-navy" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          Navi in Attesa
          {pendingShips.length > 0 && (
            <span className="ml-1 bg-amber-100 text-amber-700 text-xs font-semibold px-2 py-0.5 rounded-full">
              {pendingShips.length}
            </span>
          )}
        </h2>
        <button onClick={refreshPendingShips} className="btn-secondary py-1.5 text-xs flex items-center gap-1.5">
          <svg className="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
          </svg>
          Aggiorna
        </button>
      </div>

      {selectedShip && (
        <div className="mb-3 px-3 py-2 bg-harbor-light border border-harbor-border rounded-lg flex items-center justify-between">
          <span className="text-xs text-harbor-accent font-semibold">
            Assegnazione: <span className="text-navy">{selectedShip.name}</span>
            &nbsp;·&nbsp;seleziona una banchina compatibile ({selectedShip.size})
          </span>
          <button onClick={() => onSelect(null)} className="text-slate-400 hover:text-slate-600 ml-2">
            <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
      )}

      {pendingShips.length === 0 ? (
        <div className="flex items-center justify-center py-8">
          <div className="text-center">
            <svg className="w-10 h-10 text-emerald-200 mx-auto mb-2" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <p className="text-sm font-medium text-slate-400">Nessuna nave in attesa</p>
          </div>
        </div>
      ) : (
        <div className="overflow-x-auto -mx-5 -mb-5">
          <table className="w-full text-left">
            <thead>
              <tr>
                <th className="table-header">Nome</th>
                <th className="table-header">Dim.</th>
                <th className="table-header">Arrivo</th>
                <th className="table-header">Durata</th>
                <th className="table-header w-24"></th>
              </tr>
            </thead>
            <tbody>
              {pendingShips.map((ship) => {
                const isSelected = selectedShip?.id === ship.id
                return (
                  <tr
                    key={ship.id}
                    className={`transition-colors cursor-pointer ${
                      isSelected
                        ? 'bg-harbor-light border-l-2 border-harbor-accent'
                        : 'hover:bg-slate-50'
                    }`}
                    onClick={() => onSelect(isSelected ? null : ship)}
                  >
                    <td className="table-cell font-semibold text-navy pl-4">{ship.name}</td>
                    <td className="table-cell">
                      <SizeBadge size={ship.size} />
                    </td>
                    <td className="table-cell tabular-nums text-slate-600">
                      Giorno {ship.arrivalDay}
                    </td>
                    <td className="table-cell tabular-nums text-slate-600">
                      {ship.durationDays} gg
                    </td>
                    <td className="table-cell">
                      <span className={`text-xs font-medium px-2 py-0.5 rounded ${
                        isSelected
                          ? 'bg-harbor-accent text-white'
                          : 'text-harbor-accent hover:text-blue-700'
                      }`}>
                        {isSelected ? 'Selezionata' : 'Seleziona →'}
                      </span>
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
