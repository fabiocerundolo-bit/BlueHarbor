import { useApp } from '../../context/AppContext'

const DAY_W = 38    // px per day column
const LABEL_W = 160 // px for berth label column
const ROW_H = 48    // px per berth row
const DAYS_BEFORE = 4
const DAYS_AFTER = 35

const SIZE_COLORS = {
  XL: { header: 'bg-red-100 text-red-700 border-red-200',   row: 'border-red-100' },
  L:  { header: 'bg-orange-100 text-orange-700 border-orange-200', row: 'border-orange-100' },
  M:  { header: 'bg-blue-100 text-blue-700 border-blue-200', row: 'border-blue-100' },
  S:  { header: 'bg-violet-100 text-violet-700 border-violet-200', row: 'border-violet-100' },
}

function groupBerthsBySize(berths) {
  const order = ['XL', 'L', 'M', 'S']
  return order
    .map((size) => ({ size, berths: berths.filter((b) => b.size === size) }))
    .filter((g) => g.berths.length > 0)
}

export default function BerthGrid({ selectedShip, onBerthClick }) {
  const { berths, berthsLoading, currentDay, refreshBerths } = useApp()

  if (berthsLoading && berths.length === 0) {
    return (
      <div className="card flex items-center justify-center py-16">
        <div className="text-center">
          <svg className="w-8 h-8 animate-spin text-navy-200 mx-auto mb-3" fill="none" viewBox="0 0 24 24">
            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"/>
            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"/>
          </svg>
          <p className="text-sm text-slate-400">Loading berths...</p>
        </div>
      </div>
    )
  }

  const cd = currentDay ?? 1
  const startDay = Math.max(1, cd - DAYS_BEFORE)
  const endDay = cd + DAYS_AFTER
  const totalDays = endDay - startDay + 1
  const totalGridW = LABEL_W + totalDays * DAY_W
  const groups = groupBerthsBySize(berths)

  const isAssignmentVisible = (a) => a.endDay >= startDay && a.startDay <= endDay
  const clipLeft = (a) => Math.max(a.startDay, startDay)
  const clipRight = (a) => Math.min(a.endDay, endDay)

  return (
    <div className="card flex flex-col min-h-0">
      <div className="flex items-center justify-between mb-4">
        <h2 className="section-title mb-0">
          <svg className="w-4 h-4 text-navy" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M9 17V7m0 10a2 2 0 01-2 2H5a2 2 0 01-2-2V7a2 2 0 012-2h2a2 2 0 012 2m0 10a2 2 0 002 2h2a2 2 0 002-2M9 7a2 2 0 012-2h2a2 2 0 012 2m0 10V7m0 10a2 2 0 002 2h2a2 2 0 002-2V7a2 2 0 00-2-2h-2a2 2 0 00-2 2" />
          </svg>
          Berth Schedule
          <span className="text-xs font-normal text-slate-400 ml-1">
            (days {startDay} – {endDay})
          </span>
        </h2>
        <button onClick={refreshBerths} className="btn-secondary py-1.5 text-xs flex items-center gap-1.5">
          <svg className={`w-3.5 h-3.5 ${berthsLoading ? 'animate-spin' : ''}`} fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
          </svg>
          Refresh
        </button>
      </div>

      {berths.length === 0 ? (
        <div className="flex items-center justify-center py-12">
          <div className="text-center">
            <p className="text-sm font-medium text-slate-400">Berth data unavailable</p>
            <p className="text-xs text-slate-300 mt-1">
              The <code className="bg-slate-100 px-1 rounded">GET /api/scheduler/berths</code> endpoint must be added to the backend
            </p>
          </div>
        </div>
      ) : (
        <div className="overflow-x-auto berth-scroll -mx-1 pb-2">
          <div style={{ minWidth: `${totalGridW}px` }}>

            {/* ── Day header ───────────────────────────────────────── */}
            <div className="flex sticky top-0 z-10 bg-white border-b border-slate-200">
              {/* Berth label spacer */}
              <div style={{ width: LABEL_W, minWidth: LABEL_W }}
                className="flex-shrink-0 px-3 py-2 text-xs font-semibold text-slate-400 uppercase tracking-wide bg-slate-50 border-r border-slate-200">
                Berth
              </div>
              {/* Day numbers */}
              {Array.from({ length: totalDays }, (_, i) => startDay + i).map((day) => {
                const isToday = day === cd
                const isPast = day < cd
                return (
                  <div
                    key={day}
                    style={{ width: DAY_W, minWidth: DAY_W }}
                    className={`flex-shrink-0 text-center text-xs py-2 border-r select-none
                      ${isToday
                        ? 'bg-navy text-white font-bold border-navy'
                        : isPast
                          ? 'text-slate-300 bg-slate-50 border-slate-100'
                          : 'text-slate-500 bg-white border-slate-100'
                      }`}
                  >
                    {day}
                    {isToday && <div className="text-[9px] leading-none opacity-70 mt-0.5">today</div>}
                  </div>
                )
              })}
            </div>

            {/* ── Size groups ──────────────────────────────────────── */}
            {groups.map(({ size, berths: groupBerths }) => {
              const sc = SIZE_COLORS[size]
              const isCompatible = selectedShip?.size === size
              return (
                <div key={size}>
                  {/* Group header */}
                  <div className={`flex items-center border-b ${sc.row}`}>
                    <div
                      style={{ width: LABEL_W, minWidth: LABEL_W }}
                      className={`flex-shrink-0 px-3 py-1.5 text-xs font-semibold uppercase tracking-wider ${sc.header} border-r`}
                    >
                      {size} Berths
                    </div>
                    <div className="flex-1 bg-slate-50/50 py-1.5 px-3 text-xs text-slate-400">
                      {groupBerths.length} {groupBerths.length === 1 ? 'berth' : 'berths'}
                    </div>
                  </div>

                  {/* Berth rows */}
                  {groupBerths.map((berth) => {
                    const canAssign = isCompatible
                    return (
                      <div
                        key={berth.id}
                        data-testid={canAssign ? 'berth-clickable' : 'berth-row'}
                        className={`flex border-b border-slate-100 ${
                          canAssign
                            ? 'cursor-pointer hover:bg-harbor-light ring-inset ring-2 ring-harbor-accent/30'
                            : selectedShip
                              ? 'opacity-40'
                              : ''
                        }`}
                        style={{ height: ROW_H }}
                        onClick={() => canAssign && onBerthClick(berth)}
                        title={canAssign ? `Assign ${selectedShip.name} to ${berth.name}` : undefined}
                      >
                        {/* Berth label */}
                        <div
                          style={{ width: LABEL_W, minWidth: LABEL_W, height: ROW_H }}
                          className={`flex-shrink-0 flex items-center gap-2 px-3 border-r border-slate-100
                            ${canAssign ? 'bg-harbor-light/50' : 'bg-white'}`}
                        >
                          {canAssign && (
                            <span className="w-1.5 h-1.5 rounded-full bg-harbor-accent flex-shrink-0 animate-pulse" />
                          )}
                          <span className="text-xs font-semibold text-navy truncate">{berth.name}</span>
                        </div>

                        {/* Timeline area */}
                        <div className="flex-1 relative overflow-hidden" style={{ height: ROW_H }}>
                          {/* Day column lines */}
                          {Array.from({ length: totalDays }, (_, i) => startDay + i).map((day) => (
                            <div
                              key={day}
                              className={`absolute top-0 bottom-0 border-r ${
                                day === cd
                                  ? 'bg-navy/5 border-navy/20'
                                  : day < cd
                                    ? 'bg-slate-50/50 border-slate-100'
                                    : 'border-slate-100'
                              }`}
                              style={{ left: (day - startDay) * DAY_W, width: DAY_W }}
                            />
                          ))}

                          {/* Assignment blocks */}
                          {(berth.assignments ?? [])
                            .filter(isAssignmentVisible)
                            .map((a) => {
                              const left = (clipLeft(a) - startDay) * DAY_W + 2
                              const width = (clipRight(a) - clipLeft(a) + 1) * DAY_W - 4
                              const isDeparted = a.status === 'Departed'
                              return (
                                <div
                                  key={a.id}
                                  className={`absolute top-1.5 rounded-md flex items-center px-2 overflow-hidden
                                    text-white text-xs font-medium truncate
                                    ${isDeparted ? 'bg-slate-400' : 'bg-navy hover:bg-navy-400'} transition-colors`}
                                  style={{ left, width, height: ROW_H - 12 }}
                                  title={`${a.shipName} · days ${a.startDay}–${a.endDay}`}
                                >
                                  {width > 40 && (
                                    <span className="truncate">{a.shipName}</span>
                                  )}
                                </div>
                              )
                            })}
                        </div>
                      </div>
                    )
                  })}
                </div>
              )
            })}

            {/* Legend */}
            <div className="flex items-center gap-6 px-4 py-3 border-t border-slate-100 bg-slate-50/50">
              <div className="flex items-center gap-1.5 text-xs text-slate-500">
                <div className="w-5 h-3 rounded bg-navy" />
                Assigned
              </div>
              <div className="flex items-center gap-1.5 text-xs text-slate-500">
                <div className="w-5 h-3 rounded bg-slate-400" />
                Departed
              </div>
              <div className="flex items-center gap-1.5 text-xs text-slate-500">
                <div className="w-5 h-3 rounded bg-navy/5 border border-navy/20" />
                Current day
              </div>
              {selectedShip && (
                <div className="flex items-center gap-1.5 text-xs text-harbor-accent font-medium">
                  <span className="w-2 h-2 rounded-full bg-harbor-accent animate-pulse" />
                  Compatible berths are clickable
                </div>
              )}
            </div>

          </div>
        </div>
      )}
    </div>
  )
}
